/*
   Tuesday, May 17, 20164:12:36 PM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
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
ALTER TABLE Inventory.StockAdjustment ADD
	OrderItemId int NULL
GO
ALTER TABLE Inventory.StockAdjustment ADD CONSTRAINT
	FK_StockAdjustment_OrderItem FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockAdjustment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
