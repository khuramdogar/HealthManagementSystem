/*
   Wednesday, 15 July 20153:46:59 PM
   User: 
   Server: DEV-NEIL
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
ALTER TABLE Inventory.Consumption
	DROP CONSTRAINT FK_Consumption_StockTakeAdjustment
GO
DROP TABLE Inventory.StockTakeAdjustment
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockTakeItem ADD
	PreviousStockLevel int NULL,
	NewStockLevel int NULL,
	Adjustment int NULL,
	ProcessedOn datetime NULL,
	StockSurplusEntryId int NULL
GO
ALTER TABLE Inventory.StockTakeItem ADD CONSTRAINT
	FK_StockTakeItem_Stock FOREIGN KEY
	(
	StockSurplusEntryId
	) REFERENCES Inventory.Stock
	(
	StockId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockTakeItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Consumption ADD CONSTRAINT
	FK_Consumption_StockTakeItem FOREIGN KEY
	(
	StockWriteOffId
	) REFERENCES Inventory.StockTakeItem
	(
	StockTakeItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Consumption SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
