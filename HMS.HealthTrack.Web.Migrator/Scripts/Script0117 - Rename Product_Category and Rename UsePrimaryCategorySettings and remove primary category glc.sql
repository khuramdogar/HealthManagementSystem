/*
   Tuesday, 1 September 201511:28:10 AM
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
ALTER TABLE Inventory.Product_Category
	DROP CONSTRAINT FK_Inventory_Master_Category_Inventory_Product
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product_Category
	DROP CONSTRAINT FK_Inventory_Master_Category_Inventory_Category
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.Product_Category', N'ProductCategory', 'OBJECT' 
GO
EXECUTE sp_rename N'Inventory.ProductCategory.Inv_ID', N'Tmp_ProductId_2', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductCategory.Tmp_ProductId_2', N'ProductId', 'COLUMN' 
GO
ALTER TABLE Inventory.ProductCategory ADD CONSTRAINT
	FK_Inventory_ProductCategory_Inventory_Category FOREIGN KEY
	(
	CategoryId
	) REFERENCES Inventory.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductCategory ADD CONSTRAINT
	FK_Inventory_ProductCategory_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductCategory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
/*
   Tuesday, 1 September 201511:32:49 AM
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
EXECUTE sp_rename N'Inventory.Product.UsePrimaryCategorySettings', N'Tmp_UseCategorySettings', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.Product.Tmp_UseCategorySettings', N'UseCategorySettings', 'COLUMN' 
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Product_UsePrimaryCategoryGLC
GO
ALTER TABLE Inventory.Product
	DROP COLUMN UsePrimaryCategoryGLC
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE Inventory.Product
	DROP CONSTRAINT FK_Product_Category
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product
	DROP COLUMN PrimaryCategoryId
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
