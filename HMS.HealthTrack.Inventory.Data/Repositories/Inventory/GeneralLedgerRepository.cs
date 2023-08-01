using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using LinqKit;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class GeneralLedgerRepository : IGeneralLedgerRepository
   {
      private readonly IDbContextInventoryContext _context;

      public GeneralLedgerRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public int GetLedgerDepth(int ledgerId)
      {
         return _context.GeneralLedgerClosures.Where(glc => glc.ChildId == ledgerId).Max(glc => glc.Depth);
      }

      public List<GeneralLedgerClosure> GetParentClosures(int ledgerId)
      {
         return _context.GeneralLedgerClosures.Where(glc => glc.ChildId == ledgerId).ToList();
      }

      public void Add(GeneralLedger ledger, string username)
      {
         ledger.CreatedBy = username;
         ledger.LastModifiedBy = username;
         ledger.LastModifiedOn = DateTime.Now;
         _context.GeneralLedgers.Add(ledger);
      }

      public GeneralLedger Find(int id)
      {
         return _context.GeneralLedgers.Include(gl => gl.GeneralLedgerChildren).Include(gl => gl.GeneralLedgerTier).SingleOrDefault(gl => gl.LedgerId == id);
      }

      public IQueryable<int> FindWithDepth(int depth)
      {
         return _context.GeneralLedgerClosures.Where(glc => glc.Depth == depth).Select(glc => glc.ChildId);
      }

      public int SubTreeSize(int ledgerId)
      {
         return _context.GeneralLedgerClosures.Count(glc => glc.ParentId == ledgerId);
      }

      public void Update(GeneralLedger ledger, string username)
      {
         var existing = Find(ledger.LedgerId);
         if (existing == null) return;

         ledger.LastModifiedBy = username;
         ledger.LastModifiedOn = DateTime.Now;

         _context.Entry(existing).CurrentValues.SetValues(ledger);
         _context.Entry(existing).Property(p => p.CreatedOn).IsModified = false;
      }

      public void Remove(int ledgerId, string username)
      {
         var generalLedgers = from gl in _context.GeneralLedgers
            join glc in _context.GeneralLedgerClosures on gl.LedgerId equals glc.ChildId
            where glc.ParentId == ledgerId
            select gl;

         foreach (var ledger in generalLedgers) RemoveGeneralLedger(ledger, username);
      }

      public IQueryable<GeneralLedger> FindAll()
      {
         return
            _context.GeneralLedgers.Include(gl => gl.GeneralLedgerChildren)
               .Include(gl => gl.GeneralLedgerTier)
               .Where(gl => !gl.DeletedOn.HasValue);
      }

      public IQueryable<GeneralLedger> FindRoots()
      {
         var rootIds = _context.GeneralLedgerClosureRoots.Select(glcr => glcr.LedgerId);
         return
            _context.GeneralLedgers.Include(gl => gl.GeneralLedgerChildren.Select(glc => glc.GeneralLedgerChild))
               .Include(gl => gl.GeneralLedgerTier)
               .Where(gl => rootIds.Contains(gl.LedgerId));
      }

      public IQueryable<int> FindRootIds()
      {
         return _context.GeneralLedgerClosureRoots.Select(r => r.LedgerId);
      }

      public int FindSubtreeRoot(int ledgerId)
      {
         var rootIds = _context.GeneralLedgerClosureRoots.Select(glcr => glcr.LedgerId);
         var rootId =
            _context.GeneralLedgerClosures.Single(
               glc => rootIds.Contains(glc.ParentId) && glc.ChildId == ledgerId);
         return rootId.ParentId;
      }

      public IQueryable<int> Search(int ledgerType, string phrase)
      {
         var ledgerIds = (from gl in _context.GeneralLedgers
            join glc in _context.GeneralLedgerClosures on gl.LedgerId equals glc.ChildId
            join glt in _context.GeneralLedgerTiers on gl.TierId equals glt.TierId
            where glt.LedgerType == ledgerType && gl.Name.Contains(phrase)
            select glc.ParentId).Distinct();
         return ledgerIds;
      }

      public IQueryable<int> Search(Expression<Func<GeneralLedger, bool>> predicate)
      {
         var ledgers = _context.GeneralLedgers.AsExpandable().Where(predicate);

         var ledgerIds = (from gl in ledgers
            join glc in _context.GeneralLedgerClosures on gl.LedgerId equals glc.ChildId
            select glc.ParentId).Distinct();
         return ledgerIds;
      }

      public IQueryable<GeneralLedger> GetGeneralLedgers(int ledgerType)
      {
         var ledgers =
            _context.GeneralLedgers
               .Include(gl => gl.GeneralLedgerParents)
               .Include(gl => gl.GeneralLedgerChildren)
               .Include(gl => gl.GeneralLedgerTier)
               .Where(gl => gl.GeneralLedgerTier.LedgerType == ledgerType && !gl.DeletedOn.HasValue);
         return ledgers;
      }

      public IQueryable<GeneralLedgerClosure> FindAllClosures()
      {
         return _context.GeneralLedgerClosures.Include(glc => glc.GeneralLedgerParent);
      }

      public IQueryable<GeneralLedger> GetLedgers(int ledgerType, int tier, int? parentId)
      {
         var ledgers = from glt in _context.GeneralLedgerTiers
            join gl in _context.GeneralLedgers on glt.TierId equals gl.TierId
            where glt.LedgerType == ledgerType && glt.Tier == tier
            select gl;

         if (parentId.HasValue)
         {
            var children = FindAllClosures().Where(c => c.ParentId == parentId.Value).Select(c => c.ChildId);
            ledgers = ledgers.Where(l => children.Contains(l.LedgerId));
         }

         return ledgers;
      }

      private void RemoveGeneralLedger(GeneralLedger ledger, string username)
      {
         ledger.DeletedBy = username;
         ledger.DeletedOn = DateTime.Now;
      }
   }

   public interface IGeneralLedgerRepository
   {
      int GetLedgerDepth(int ledgerId);
      List<GeneralLedgerClosure> GetParentClosures(int ledgerId);
      void Add(GeneralLedger ledger, string username);
      void Update(GeneralLedger ledger, string username);
      void Remove(int ledgerId, string username);
      IQueryable<GeneralLedger> FindAll();
      IQueryable<GeneralLedger> FindRoots();
      GeneralLedger Find(int id);
      IQueryable<int> FindWithDepth(int depth);
      int SubTreeSize(int ledgerId);
      int FindSubtreeRoot(int ledgerId);
      IQueryable<int> Search(int ledgerType, string phrase);
      IQueryable<int> Search(Expression<Func<GeneralLedger, bool>> predicate);
      IQueryable<GeneralLedger> GetGeneralLedgers(int ledgerType);
      IQueryable<int> FindRootIds();
      IQueryable<GeneralLedgerClosure> FindAllClosures();
      IQueryable<GeneralLedger> GetLedgers(int ledgerType, int tier, int? parentId);
   }
}