/*
   Tuesday, 3 March 20152:57:32 PM
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
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_ProductStockRequest
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_Consumption
GO
ALTER TABLE Inventory.Consumption SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_OrderItem
GO
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_OrderItemSource
	(
	ItemSourceId int NOT NULL IDENTITY (1, 1),
	OrderItemId int NOT NULL,
	ConsumptionId int NULL,
	StockRequestId int NULL,
	Quantity int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_OrderItemSource SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_OrderItemSource ON
GO
IF EXISTS(SELECT * FROM Inventory.OrderItemSource)
	 EXEC('INSERT INTO Inventory.Tmp_OrderItemSource (ItemSourceId, OrderItemId, ConsumptionId, StockRequestId, Quantity)
		SELECT ItemSourceId, OrderItemId, ConsumptionId, StockRequestId, Quantity FROM Inventory.OrderItemSource WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_OrderItemSource OFF
GO
DROP TABLE Inventory.OrderItemSource
GO
EXECUTE sp_rename N'Inventory.Tmp_OrderItemSource', N'OrderItemSource', 'OBJECT' 
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	PK_OrderItemSource PRIMARY KEY CLUSTERED 
	(
	ItemSourceId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_OrderItem FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_Consumption FOREIGN KEY
	(
	ConsumptionId
	) REFERENCES Inventory.Consumption
	(
	ConsumptionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_ProductStockRequest FOREIGN KEY
	(
	StockRequestId
	) REFERENCES Inventory.ProductStockRequest
	(
	StockRequestId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
