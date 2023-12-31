/*
   Wednesday, 24 June 201510:48:03 AM
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
CREATE TABLE Inventory.ProductImportSet
	(
	ProductImportSetId int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NULL,
	ImportedOn datetime NOT NULL,
   Status int NOT NULL
	)  ON [PRIMARY]
GO

ALTER TABLE Inventory.ProductImportSet ADD CONSTRAINT
   DF_ProductImportSet_Status DEFAULT 0 FOR Status
GO

ALTER TABLE Inventory.ProductImportSet ADD CONSTRAINT
	PK_ProductImportSet PRIMARY KEY CLUSTERED 
	(
	ProductImportSetId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE Inventory.ProductImportSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
