/*
   Wednesday, May 25, 20164:51:01 PM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
   Database: HealthTrack_Web
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.IgnoredBy', N'Tmp_ArchivedBy_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.IgnoredOn', N'Tmp_ArchivedOn_2', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.Tmp_ArchivedBy_1', N'ArchivedBy', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.Tmp_ArchivedOn_2', N'ArchivedOn', 'COLUMN' 
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
