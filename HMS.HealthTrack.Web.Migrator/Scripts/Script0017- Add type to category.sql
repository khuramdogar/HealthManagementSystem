/*
   Thursday, 19 February 20152:12:33 PM
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
ALTER TABLE Inventory.Category ADD
	CategoryType int NOT NULL CONSTRAINT DF_Category_CategoryType DEFAULT 0
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
