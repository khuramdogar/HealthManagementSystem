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
ALTER TABLE Inventory.ProductImportData ADD
	CreatedBy varchar(50) NULL,
	CreatedOn datetime NOT NULL CONSTRAINT DF_ProductImportData_CreatedOn DEFAULT GETDATE(),
	LastModifiedBy varchar(50) NULL,
	LastModifiedOn datetime NULL,
	DeletedBy varchar(50) NULL,
	DeletedOn datetime NULL
GO
ALTER TABLE Inventory.ProductImportData SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
