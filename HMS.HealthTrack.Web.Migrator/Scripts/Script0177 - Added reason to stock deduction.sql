/*
   Friday, 4 December 201510:55:00 AM
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
ALTER TABLE Inventory.StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction ADD
	Reason int NULL
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_StockDeduction_StockDeductionReason FOREIGN KEY
	(
	Reason
	) REFERENCES Inventory.StockDeductionReason
	(
	StockDeductionReasonId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
