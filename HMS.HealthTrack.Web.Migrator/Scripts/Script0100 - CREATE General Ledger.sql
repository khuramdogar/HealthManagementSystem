/*
   Wednesday, 26 August 20158:13:25 PM
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
CREATE TABLE Inventory.GeneralLedger
	(
	LedgerId int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NULL,
	ParentId int NULL,
	Code varchar(50) NULL,
	AlternateCode varchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.GeneralLedger ADD CONSTRAINT
	PK_GeneralLedger PRIMARY KEY CLUSTERED 
	(
	LedgerId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.GeneralLedger ADD CONSTRAINT
	FK_GeneralLedger_GeneralLedger FOREIGN KEY
	(
	ParentId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
