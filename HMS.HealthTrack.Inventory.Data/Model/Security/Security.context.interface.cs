﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace HMS.HealthTrack.Web.Data.Model.Security
{
   public interface IDbContextSecurity : IObjectContextAdapter
   {
      DbSet<HealthTrackAuthorisation> HealthTrackAuthorisations { get; set; }
      DbSet<HealthTrackPermission> HealthTrackPermissions { get; set; }
      DbSet<HealthTrackUser> HealthTrackUsers { get; set; }
      DbSet<HealthTrackGroup> HealthTrackGroups { get; set; }
      DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
   }
}