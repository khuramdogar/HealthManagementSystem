/*
   Friday, 10 July 20153:37:24 PM
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
ALTER TABLE Inventory.StockTakeAdjustment
	DROP CONSTRAINT FK_StockTakeAdjustment_Consumption
GO
ALTER TABLE Inventory.Consumption ADD
	StockWriteOffId int NULL
GO
ALTER TABLE Inventory.Consumption SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Consumption ADD CONSTRAINT
	FK_Consumption_StockTakeAdjustment FOREIGN KEY
	(
	StockWriteOffId
	) REFERENCES Inventory.StockTakeAdjustment
	(
	StockTakeAdjustmentId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockTakeAdjustment
	DROP COLUMN StockWriteOffId
GO
ALTER TABLE Inventory.StockTakeAdjustment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
