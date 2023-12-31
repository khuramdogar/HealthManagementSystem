/*
   Friday, 6 March 201512:03:48 PM
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
EXECUTE sp_rename N'Inventory.ProductStockRequest.Quantity', N'Tmp_RequestedQuantity', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductStockRequest.Tmp_RequestedQuantity', N'RequestedQuantity', 'COLUMN' 
GO
ALTER TABLE Inventory.ProductStockRequest ADD
	ApprovedQuantity int NULL
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

UPDATE Inventory.ProductStockRequest
SET ApprovedQuantity = RequestedQuantity