/*
   Friday, 18 September 20152:01:00 PM
   User: sa
   Server: DEVDANIEL\SQLSERVER2014
   Database: HMS_v2_EasternHealth
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
ALTER TABLE Inventory.GeneralLedger ADD
	LedgerType int NOT NULL CONSTRAINT DF_GeneralLedger_LedgerType DEFAULT 0
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


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
EXECUTE sp_rename N'Inventory.GeneralLedgerTier.TierType', N'Tmp_LedgerType_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.GeneralLedgerTier.Tmp_LedgerType_1', N'LedgerType', 'COLUMN' 
GO
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION

UPDATE Inventory.GeneralLedger
SET LedgerType = 1
WHERE LedgerId IN (
	SELECT DISTINCT ChildId
	FROM Inventory.GeneralLedgerClosure
	WHERE Depth > 1)

ALTER TABLE [Inventory].[GeneralLedger] DROP CONSTRAINT [DF_GeneralLedger_LedgerType]
GO

COMMIT