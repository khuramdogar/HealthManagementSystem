DELETE FROM Inventory.ProductImport
GO
DELETE FROM Inventory.ProductImportData
GO

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
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.ProductImport.ProductId', N'Tmp_InternalProductId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductImport.Tmp_InternalProductId', N'InternalProductId', 'COLUMN' 
GO
ALTER TABLE Inventory.ProductImport ADD
	ProductId int NULL
GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	FK_ProductImport_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

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
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductImport ADD
	LedgerId int NULL
GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	FK_ProductImport_GeneralLedger FOREIGN KEY
	(
	LedgerId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
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
ALTER TABLE Inventory.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductImport ADD
	SupplierId int NULL
GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	FK_ProductImport_Supplier FOREIGN KEY
	(
	SupplierId
	) REFERENCES Inventory.Supplier
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
