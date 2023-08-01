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

namespace HMS.HealthTrack.Web.Data.Model.Clinical
{
   public interface IDbContextClinicalContext : IObjectContextAdapter
   {
      DbSet<MR_Container> MR_Container { get; set; }
      DbSet<Booking> Bookings { get; set; }
      DbSet<List_Core> List_Core { get; set; }
      DbSet<Patient> Patients { get; set; }
      DbSet<ExternalFeed> ExternalFeeds { get; set; }
      DbSet<PatientMapping> PatientMappings { get; set; }
      DbSet<PatientContainerMRN> PatientContainerMRNs { get; set; }
      DbSet<ContainerPaymentClass> ContainerPaymentClasses { get; set; }
      DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
   }
}