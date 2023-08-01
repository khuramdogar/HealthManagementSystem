/*
   Thursday, 22 January 20155:46:02 PM
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
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Inventory_Stock_Inventory_Order
GO
ALTER TABLE Inventory.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

UPDATE Inventory.Stock
SET InventoryOrderID = NULL
GO

BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.Stock.InventoryOrderId', N'Tmp_OrderItemId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.Stock.Tmp_OrderItemId', N'OrderItemId', 'COLUMN' 
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Stock_OrderItem FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
