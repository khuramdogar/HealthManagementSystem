/*
   Monday, 16 March 201511:31:15 AM
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
ALTER TABLE dbo.Company SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Supplier
	(
	company_ID int NOT NULL,
	CreatedOn datetime NOT NULL,
	CreatedBy varchar(100) NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy varchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Supplier ADD CONSTRAINT
	DF_Supplier_CreatedOn DEFAULT (GETDATE()) FOR CreatedOn
GO
ALTER TABLE Inventory.Supplier ADD CONSTRAINT
	PK_Supplier PRIMARY KEY CLUSTERED 
	(
	company_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.Supplier ADD CONSTRAINT
	FK_Supplier_Company FOREIGN KEY
	(
	company_ID
	) REFERENCES dbo.Company
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
