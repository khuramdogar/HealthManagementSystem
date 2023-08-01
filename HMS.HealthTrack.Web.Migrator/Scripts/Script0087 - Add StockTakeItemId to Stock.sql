/*
   Thursday, 13 August 20152:01:29 PM
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
ALTER TABLE Inventory.StockTakeItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock ADD
	StockTakeItemId int NULL
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Stock_StockTakeItem FOREIGN KEY
	(
	StockTakeItemId
	) REFERENCES Inventory.StockTakeItem
	(
	StockTakeItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
