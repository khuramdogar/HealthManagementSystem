/*
   Tuesday, 18 August 20151:05:00 PM
   User: 
   Server: devdaniel
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
ALTER TABLE Inventory.ProductImportSet ADD CONSTRAINT
	DF_ProductImportSet_ImportedOn DEFAULT GETDATE() FOR ImportedOn
GO
ALTER TABLE Inventory.ProductImportSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
