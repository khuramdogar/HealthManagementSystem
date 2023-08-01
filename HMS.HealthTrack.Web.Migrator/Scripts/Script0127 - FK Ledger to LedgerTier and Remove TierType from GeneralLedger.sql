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
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.GeneralLedger ADD
	TierId int NULL
GO
ALTER TABLE Inventory.GeneralLedger ADD CONSTRAINT
	FK_GeneralLedger_GeneralLedgerTier FOREIGN KEY
	(
	TierId
	) REFERENCES Inventory.GeneralLedgerTier
	(
	TierId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

UPDATE Inventory.GeneralLedger
SET TierId = gltier.TierId
FROM Inventory.GeneralLedger gl
INNER JOIN Inventory.GeneralLedgerClosure glc on gl.LedgerId = glc.ChildId
INNER JOIN Inventory.GeneralLedgerClosureRoots glcr on glc.ParentId = glcr.LedgerId
INNER JOIN Inventory.GeneralLedgerType glt on gl.LedgerType = glt.LedgerTypeId
INNER JOIN Inventory.GeneralLedgerTier gltier on glt.LedgerTypeId = gltier.LedgerType AND glc.Depth + 1 = gltier.Tier
GO

delete from inventory.GeneralLedgerClosure
where parentId in (
	select ledgerid
	from inventory.generalledger
	where deletedon IS NOT NULL)

delete from inventory.generalledger
where deletedon IS NOT NULL

BEGIN TRANSACTION

ALTER TABLE Inventory.GeneralLedger
ALTER COLUMN TierId int NOT NULL
GO 

COMMIT

BEGIN TRANSACTION
GO
ALTER TABLE Inventory.GeneralLedger
	DROP CONSTRAINT FK_GeneralLedger_GeneralLedgerType
GO
ALTER TABLE Inventory.GeneralLedgerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.GeneralLedger
	DROP COLUMN LedgerType
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
