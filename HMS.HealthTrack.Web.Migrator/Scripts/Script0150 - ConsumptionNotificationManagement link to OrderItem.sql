DELETE FROM Inventory.ConsumptionNotificationManagement
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
ALTER TABLE Inventory.ConsumptionNotificationManagement
	DROP CONSTRAINT FK_ConsumptionNotificationManagement_Order
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
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.InventoryOrderId', N'Tmp_OrderItemId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.Tmp_OrderItemId', N'OrderItemId', 'COLUMN' 
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement ADD CONSTRAINT
	FK_ConsumptionNotificationManagement_Order FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
