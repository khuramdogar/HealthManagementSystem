/*
   Thursday, 5 March 20151:50:57 PM
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
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_RequiresSerialNumber
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_RequiresBatchNumber
GO
ALTER TABLE Inventory.Product
	DROP COLUMN RequiresSerialNumber, RequiresBatchNumber
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
