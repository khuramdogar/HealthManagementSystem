/*
   Thursday, 23 April 20153:42:56 PM
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
ALTER TABLE Inventory.Product ADD
	UsePrimaryCategoryGLC bit NOT NULL CONSTRAINT DF_Product_UsePrimaryCategoryGLC DEFAULT 0
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
