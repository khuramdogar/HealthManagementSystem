/*
   Thursday, 28 January 20163:22:31 PM
   User: 
   Server: DEV-NEIL\SQL2012
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
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction ADD
	GeneralLedgerId int NULL
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_StockDeduction_GeneralLedger FOREIGN KEY
	(
	GeneralLedgerId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
