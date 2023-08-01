using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Security;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   internal static class GroupUtils
   {
      public static List<string> GetGroupsForUser(string username, IQueryable<HealthTrackGroup> allGroups)
      {
         var groups = new List<string>();
         AppendGroups(username, allGroups, ref groups);
         return groups;
      }

      private static void AppendGroups(string memberId, IQueryable<HealthTrackGroup> allGroups, ref List<string> groups)
      {
         var groupsFound = (from g in allGroups where g.member_ID.Equals(memberId, StringComparison.InvariantCultureIgnoreCase) select g.base_ID).ToList();
         foreach (var group in groupsFound)
            if (!groups.Contains(group))
               groups.Add(group.ToLower());

         foreach (var group in groupsFound) AppendGroups(group, allGroups, ref groups);
      }
   }
}