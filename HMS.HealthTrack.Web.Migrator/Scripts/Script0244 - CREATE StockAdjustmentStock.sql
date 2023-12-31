/*
   Monday, May 9, 201612:03:44 PM
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
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockAdjustment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.StockAdjustmentStock
	(
	StockAdjustmentId bigint NOT NULL,
	StockId bigint NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockAdjustmentStock ADD CONSTRAINT
	PK_StockAdjustmentStock PRIMARY KEY CLUSTERED 
	(
	StockAdjustmentId,
	StockId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockAdjustmentStock ADD CONSTRAINT
	FK_StockAdjustmentStock_StockAdjustmentStock FOREIGN KEY
	(
	StockAdjustmentId,
	StockId
	) REFERENCES Inventory.StockAdjustmentStock
	(
	StockAdjustmentId,
	StockId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockAdjustmentStock ADD CONSTRAINT
	FK_StockAdjustmentStock_StockAdjustment FOREIGN KEY
	(
	StockAdjustmentId
	) REFERENCES Inventory.StockAdjustment
	(
	StockAdjustmentId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockAdjustmentStock ADD CONSTRAINT
	FK_StockAdjustmentStock_Stock FOREIGN KEY
	(
	StockId
	) REFERENCES Inventory.Stock
	(
	StockId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockAdjustmentStock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
