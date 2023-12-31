/*
   Friday, 21 August 201510:41:22 AM
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
EXECUTE sp_rename N'Inventory.ProductImportSet', N'ProductImportData', 'OBJECT' 
GO
EXECUTE sp_rename N'Inventory.ProductImportData.ProductImportSetId', N'Tmp_ProductImportDataId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductImportData.Tmp_ProductImportDataId', N'ProductImportDataId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductImport.ProductImportSetId', N'Tmp_ProductImportDataId_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductImport.Tmp_ProductImportDataId_1', N'ProductImportDataId', 'COLUMN' 
GO

ALTER TABLE Inventory.ProductImportData SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
ALTER TABLE Inventory.ProductImport
	DROP CONSTRAINT FK_ProductImport_ProductImportSet
GO
ALTER TABLE Inventory.ProductImportData SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	FK_ProductImport_ProductImportData FOREIGN KEY
	(
	ProductImportDataId
	) REFERENCES Inventory.ProductImportData
	(
	ProductImportDataId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

EXEC sp_rename 'Inventory.ProductImportData.PK_ProductImportSet', 'PK_ProductImportData'
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
