/*
   Friday, 22 January 20163:20:05 PM
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
ALTER TABLE Inventory.StockDeductionReason ADD
	Disabled bit NOT NULL CONSTRAINT DF_StockDeductionReason_Disabled DEFAULT 0
GO
ALTER TABLE Inventory.StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
