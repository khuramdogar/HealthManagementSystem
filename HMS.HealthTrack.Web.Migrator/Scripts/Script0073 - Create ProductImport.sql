/*
   Wednesday, 24 June 201511:02:29 AM
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
ALTER TABLE Inventory.ProductImportSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.ProductImport
	(
	ProductImportId int NOT NULL IDENTITY (1, 1),
	ProductImportSetId int NOT NULL,
	Processed bit NOT NULL,
	ProcessedOn datetime NULL,
   ProductId varchar(20) NULL,
	SPC varchar(50) NULL,
	LPC varchar(50) NULL,
   UPN varchar(50) NULL,
	Description varchar(500) NULL,
	Notes varchar(MAX) NULL,
	GLC varchar(50) NULL,
	Manufacturer varchar(50) NULL,
	Supplier varchar(50) NULL,
	Category varchar(50) NULL,
	MinimumOrder varchar(10) NULL,
	OrderMultiple varchar(10) NULL,
	ReorderThreshold varchar(10) NULL,
	TargetStockLevel varchar(10) NULL,
	PublicUnitPrice varchar(10) NULL,
	PrivateUnitPrice varchar(10) NULL,
	Consignment varchar(10) NULL,
	Sterile varchar(10) NULL,
   Invalid bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	DF_ProductImport_Processed DEFAULT 0 FOR Processed
GO

ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
   DF_ProductImport_Invalid DEFAULT 0 FOR Invalid
GO

ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	PK_ProductImport PRIMARY KEY CLUSTERED 
	(
	ProductImportId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.ProductImport ADD CONSTRAINT
	FK_ProductImport_ProductImportSet FOREIGN KEY
	(
	ProductImportSetId
	) REFERENCES Inventory.ProductImportSet
	(
	ProductImportSetId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
