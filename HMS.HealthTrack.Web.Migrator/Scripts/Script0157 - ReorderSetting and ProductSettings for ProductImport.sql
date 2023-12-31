/*
   Thursday, 29 October 20156:02:49 PM
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
ALTER TABLE Inventory.ProductImport ADD
	ReorderSetting varchar(50) NULL,
	ProductSettings varchar(500) NULL,
   UseCategorySettings varchar(10) NULL
GO
ALTER TABLE Inventory.ProductImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
