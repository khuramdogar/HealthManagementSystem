/*
   Thursday, 27 August 201511:34:13 AM
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
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.GeneralLedgerClosure
	(
	ParentId int NOT NULL,
	ChildId int NOT NULL,
	Depth int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.GeneralLedgerClosure ADD CONSTRAINT
	PK_GeneralLedgerClosure PRIMARY KEY CLUSTERED 
	(
	ParentId,
	ChildId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.GeneralLedgerClosure ADD CONSTRAINT
	FK_GeneralLedgerClosure_GeneralLedger FOREIGN KEY
	(
	ParentId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedgerClosure ADD CONSTRAINT
	FK_GeneralLedgerClosure_GeneralLedger1 FOREIGN KEY
	(
	ChildId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedgerClosure SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

/*
   Thursday, 27 August 201511:35:15 AM
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
ALTER TABLE Inventory.GeneralLedger
	DROP CONSTRAINT FK_GeneralLedger_GeneralLedger
GO
ALTER TABLE Inventory.GeneralLedger
	DROP COLUMN ParentId
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
