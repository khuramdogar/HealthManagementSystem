using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class CategoryRepository : ICategoryRepository
   {
      private readonly IDbContextInventoryContext _context;

      public CategoryRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public Category Find(int id)
      {
         var cat = _context.Categories.SingleOrDefault(xx => xx.CategoryId == id);
         return cat;
      }

      public Category FindByName(string name)
      {
         var category = _context.Categories.FirstOrDefault(c => c.CategoryName.Equals(name));
         return category ?? _context.Categories.Local.FirstOrDefault(c => c.CategoryName.Equals(name));
      }

      public IQueryable<Category> FindAll()
      {
         return _context.Categories.Where(xx => xx.DeletedOn == null);
      }

      public void Remove(int categoryId, string user)
      {
         var categories = from c in _context.Categories
            join cc in _context.CategoryClosures on c.CategoryId equals cc.ChildId
            where cc.ParentId == categoryId
            select c;

         foreach (var category in categories) RemoveCategory(category, user);
      }

      public void Update(Category category, int? parentId, string username)
      {
         var existing = _context.Categories.SingleOrDefault(xx => xx.CategoryId == category.CategoryId);
         if (existing == null) return;

         var closures = GetClosures(category.CategoryId);
         var parent = closures.SingleOrDefault(c => c.Depth == 1);

         // If parent is new or different to current, move the subtree
         if (!parentId.HasValue || parent == null || parentId.Value != parent.ParentId) MoveTree(category.CategoryId, parentId);

         // Product settings
         var selectedSettings = (parentId.HasValue && !category.Disinherit
                                   ? Find(parentId.Value).StockSettings
                                   : category.StockSettings) ?? new Collection<StockSetting>();

         var settingIds = selectedSettings.Select(ns => ns.SettingId).ToArray();
         var settings = _context.StockSettings.Where(ss => settingIds.Contains(ss.SettingId)).ToArray();

         // Remove the settings which are not in the list of new settings
         RemoveSettings(existing, settingIds, username); // Remove from parent
         AddNewSettings(existing, settingIds, username, settings);

         foreach (var child in existing.CategoryChildren.Where(cc => cc.Depth > 0).Select(cc => cc.CategoryChildren)) UpdateSettings(child, settingIds, username, settings);
         existing.Disinherit = parentId.HasValue && category.Disinherit;
         existing.CategoryName = category.CategoryName;
         existing.LastModifiedDate = DateTime.Now;
         existing.LastModifiedUser = username;
         _context.Entry(existing).Property(p => p.UserCreated).IsModified = false;
         _context.Entry(existing).Property(p => p.CreationDate).IsModified = false;
      }

      public IQueryable<Category> FindParents(int? id)
      {
         var categories = _context.Categories;

         if (id.HasValue)
         {
            // return categories for which the current id is not a parent of
            var children =
               _context.CategoryClosures.Where(cc => cc.ParentId == id.Value).Select(cc => cc.ChildId);
            return categories.Where(c => !children.Contains(c.CategoryId) && c.CategoryId != id.Value);
         }

         return categories;
      }

      public IQueryable<int> Search(string phrase)
      {
         var categories = (from c in _context.Categories
            join cc in _context.CategoryClosures on c.CategoryId equals cc.ChildId
            where c.CategoryName.Contains(phrase)
            select cc.ParentId).Distinct();
         return categories;
      }

      public void Add(Category category, int? parentId, string username)
      {
         category.LastModifiedDate = DateTime.Now;
         category.UserCreated = username;
         category.LastModifiedUser = username;

         foreach (var stockSetting in category.StockSettings) _context.StockSettings.Attach(stockSetting);

         _context.Categories.Add(category);
         Commit(); // Need ID to insert closures

         var closures = new List<CategoryClosure>();
         if (parentId.HasValue)
         {
            // Add closure entity for every parent of the ParentId
            var parentClosures = GetClosures(parentId.Value).ToList();
            closures.AddRange(parentClosures.Select(closure => new CategoryClosure
            {
               ParentId = closure.ParentId,
               Depth = closure.Depth + 1,
               ChildId = category.CategoryId
            }));
         }

         // Add self reference
         closures.Add(new CategoryClosure
         {
            ChildId = category.CategoryId,
            Depth = 0,
            ParentId = category.CategoryId
         });

         // Add closures to db
         foreach (var closure in closures) _context.CategoryClosures.Add(closure);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<Category> Find(Expression<Func<Category, bool>> predicate)
      {
         return _context.Categories.Where(predicate);
      }

      public bool IsUniqueName(string name)
      {
         return
            _context.Categories.Count(category => category.CategoryName == name && category.DeletedOn == null) == 0;
      }

      public bool IsUniqueName(string name, int categoryId)
      {
         var categories =
            _context.Categories.Where(category => category.CategoryName == name);
         if (categories.Count() == 1 && categories.Single().CategoryId == categoryId) return true;
         return !categories.Any();
      }

      public IQueryable<Category> FindRoots()
      {
         var roots = from c in _context.Categories.Include(c => c.CategoryChildren.Select(cc => cc.CategoryChildren)).Include(c => c.StockSettings)
            join cr in _context.CategoryClosureRoots on c.CategoryId equals cr.CategoryId
            select c;
         return roots;
      }

      private void RemoveCategory(Category category, string username)
      {
         var deletionTime = DateTime.Now;
         category.DeletedBy = username;
         category.DeletedOn = deletionTime;
      }

      private void UpdateSettings(Category category, string[] settingIds, string username, StockSetting[] settings)
      {
         if (category.Disinherit) return;
         RemoveSettings(category, settingIds, username);
         AddNewSettings(category, settingIds, username, settings);

         foreach (var child in category.CategoryChildren.Where(cc => cc.Depth > 0).Select(cc => cc.CategoryChildren)) UpdateSettings(child, settingIds, username, settings);
      }

      private static void AddNewSettings(Category category, IEnumerable<string> stockSettingIds, string username, StockSetting[] settings)
      {
         foreach (var newSetting in stockSettingIds.Where(newSetting => category.StockSettings.All(e => e.SettingId != newSetting)))
         {
            category.StockSettings.Add(settings.Single(s => s.SettingId == newSetting));
            category.LastModifiedUser = username;
            category.LastModifiedDate = DateTime.Now;
         }
      }

      private void RemoveSettings(Category category, IEnumerable<string> settingIds, string username)
      {
         // Remove the settings which are not in the list of new settings
         var itemsToRemove = (from e in category.StockSettings
            where !settingIds.Contains(e.SettingId)
            select e).ToList();
         if (!itemsToRemove.Any()) return;

         category.LastModifiedUser = username;
         category.LastModifiedDate = DateTime.Now;
         foreach (var setting in itemsToRemove)
            // Remove the settings which are not in the list of new settings
            category.StockSettings.Remove(setting);
      }

      private void MoveTree(int categoryId, int? destinationId)
      {
         // Remove tree from graph
         var categoryTree = _context.CategoryClosures.Where(cc => cc.ParentId == categoryId).ToArray();
         var categoryTreeIds = categoryTree.Select(c => c.ChildId).ToArray();

         // Remove closures where child is part of the subtree but the parent is not
         var toRemove =
            _context.CategoryClosures.Where(
               cc => categoryTreeIds.Contains(cc.ChildId) && !categoryTreeIds.Contains(cc.ParentId));
         foreach (var closure in toRemove) _context.CategoryClosures.Remove(closure);

         if (!destinationId.HasValue) return; // Moved to root, does not require reconnecting

         // Reconnect to graph at specified node
         var parentTree = GetClosures(destinationId.Value).ToArray();
         var toInsert = (from closure in parentTree
            from node in categoryTree
            select new CategoryClosure
            {
               ChildId = node.ChildId,
               Depth = closure.Depth + node.Depth + 1,
               ParentId = closure.ParentId
            }).ToList();

         foreach (var closure in toInsert) _context.CategoryClosures.Add(closure);
      }

      private IQueryable<CategoryClosure> GetClosures(int categoryId)
      {
         return _context.CategoryClosures.Where(cc => cc.ChildId == categoryId);
      }
   }

   public interface ICategoryRepository
   {
      Category Find(int id);
      Category FindByName(string name);
      IQueryable<Category> FindAll();
      void Remove(int categoryId, string user);
      void Add(Category category, int? parentId, string username);
      void Commit();
      IQueryable<Category> Find(Expression<Func<Category, bool>> predicate);
      bool IsUniqueName(string name);
      bool IsUniqueName(string name, int categoryId);
      IQueryable<Category> FindRoots();
      void Update(Category category, int? parentId, string username);
      IQueryable<Category> FindParents(int? id);
      IQueryable<int> Search(string phrase);
   }
}