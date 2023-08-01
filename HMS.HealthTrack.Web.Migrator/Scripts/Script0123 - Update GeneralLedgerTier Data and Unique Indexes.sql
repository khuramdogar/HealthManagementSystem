/****** Object:  Index [UK_GeneralLedgerTier]    Script Date: 09/28/2015 10:43:51 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Inventory].[GeneralLedgerTier]') AND name = N'UK_GeneralLedgerTier')
ALTER TABLE [Inventory].[GeneralLedgerTier] DROP CONSTRAINT [UK_GeneralLedgerTier]
GO


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
ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	UK_GeneralLedgerTier UNIQUE NONCLUSTERED 
	(
	Tier,
	LedgerType
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
UPDATE [Inventory].[GeneralLedgerTier]
SET Tier = 1 
WHERE Name = 'Account' AND LedgerType = 2

UPDATE [Inventory].[GeneralLedgerTier]
SET Tier = 2
WHERE Name = 'Sub Account' AND LedgerType = 2

UPDATE [Inventory].[GeneralLedgerTier]
SET Tier = 3
WHERE Name = 'Product' AND LedgerType = 2
COMMIT