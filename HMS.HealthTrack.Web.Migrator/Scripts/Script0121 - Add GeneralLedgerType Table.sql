/*
   Friday, 18 September 20152:38:47 PM
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
CREATE TABLE Inventory.GeneralLedgerType
	(
	LedgerTypeId int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NOT NULL,
   DisplayOrder int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.GeneralLedgerType ADD CONSTRAINT
	PK_GeneralLedgerType PRIMARY KEY CLUSTERED 
	(
	LedgerTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.GeneralLedgerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

INSERT INTO [Inventory].[GeneralLedgerType]
           ([Name],
           [DisplayOrder])
     VALUES
           ('Order',
           1)
GO


INSERT INTO [Inventory].[GeneralLedgerType]
           ([Name],
           [DisplayOrder])
     VALUES
           ('Product'
           ,2)
GO


UPDATE [Inventory].GeneralLedger
SET LedgerType = LedgerType + 1

UPDATE [Inventory].[GeneralLedgerTier]
SET LedgerType = LedgerType + 1


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
ALTER TABLE Inventory.GeneralLedgerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.GeneralLedger ADD CONSTRAINT
	FK_GeneralLedger_GeneralLedgerType FOREIGN KEY
	(
	LedgerType
	) REFERENCES Inventory.GeneralLedgerType
	(
	LedgerTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE Inventory.GeneralLedgerType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	FK_GeneralLedgerTier_GeneralLedgerType FOREIGN KEY
	(
	LedgerType
	) REFERENCES Inventory.GeneralLedgerType
	(
	LedgerTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
