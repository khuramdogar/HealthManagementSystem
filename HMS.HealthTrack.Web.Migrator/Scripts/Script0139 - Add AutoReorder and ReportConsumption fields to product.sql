/*
   Friday, 23 October 201511:11:57 AM
   User: 
   Server: DEV-NEIL\SQL2012
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
ALTER TABLE Inventory.Product ADD
	ReorderSetting int NULL,
	ReportConsumption bit NULL
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'Inventory.Product', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'Inventory.Product', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'Inventory.Product', 'Object', 'CONTROL') as Contr_Per 