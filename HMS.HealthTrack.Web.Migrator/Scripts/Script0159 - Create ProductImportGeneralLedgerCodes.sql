/*
   Wednesday, 4 November 20155:27:11 PM
   User: sa
   Server: DEVDANIEL\SQLSERVER2014
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
ALTER TABLE Inventory.GeneralLedgerTier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.ProductImportGeneralLedgerCodes
	(
	ProductImportId int NOT NULL,
	TierId int NOT NULL,
	Code varchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.ProductImportGeneralLedgerCodes ADD CONSTRAINT
	PK_ProductImportGeneralLedgerCodes PRIMARY KEY CLUSTERED 
	(
	ProductImportId,
	TierId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.ProductImportGeneralLedgerCodes ADD CONSTRAINT
	FK_ProductImportGeneralLedgerCodes_ProductImport FOREIGN KEY
	(
	ProductImportId
	) REFERENCES Inventory.ProductImport
	(
	ProductImportId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImportGeneralLedgerCodes ADD CONSTRAINT
	FK_ProductImportGeneralLedgerCodes_GeneralLedgerTier FOREIGN KEY
	(
	TierId
	) REFERENCES Inventory.GeneralLedgerTier
	(
	TierId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImportGeneralLedgerCodes SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
