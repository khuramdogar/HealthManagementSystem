/*
   Wednesday, 26 August 20157:27:17 PM
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
ALTER TABLE Inventory.GlcSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.GeneralLedgerTier
	(
	TierId int NOT NULL IDENTITY (1, 1),
	Tier int NOT NULL,
	Name varchar(50) NULL,
	TierType int NOT NULL,
	SettingId int NOT NULL,
	Mandatory bit NOT NULL,
	CreatedBy varchar(50) NULL,
	CreatedOn datetime NOT NULL,
	LastModifiedBy varchar(50) NULL,
	LastModifiedOn datetime NULL,
	DeletedBy varchar(50) NULL,
	DeletedOn datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	DF_GeneralLedgerTier_Mandatory DEFAULT 0 FOR Mandatory
GO
ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	DF_GeneralLedgerTier_CreatedOn DEFAULT GETDATE() FOR CreatedOn
GO
ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	PK_GeneralLedgerTier PRIMARY KEY CLUSTERED 
	(
	TierId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	UK_GeneralLedgerTier UNIQUE NONCLUSTERED 
	(
	Tier,
	DeletedOn
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE Inventory.GeneralLedgerTier ADD CONSTRAINT
	FK_GeneralLedgerTier_GlcSetting FOREIGN KEY
	(
	SettingId
	) REFERENCES Inventory.GlcSetting
	(
	SettingId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
