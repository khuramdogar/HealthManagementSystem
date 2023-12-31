/*
   Friday, 23 January 201511:20:53 AM
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
ALTER TABLE Inventory.ProductStockRequest
	DROP CONSTRAINT FK_Inventory_ProductStockRequest_Inventory_Order
GO
ALTER TABLE Inventory.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
UPDATE Inventory.ProductStockRequest
SET InventoryOrderId = NULL
GO
COMMIT

BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.ProductStockRequest.InventoryOrderId', N'Tmp_OrderItemId_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ProductStockRequest.Tmp_OrderItemId_1', N'OrderItemId', 'COLUMN' 
GO
ALTER TABLE Inventory.ProductStockRequest ADD CONSTRAINT
	FK_Inventory_ProductStockRequest_Inventory_Order FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
