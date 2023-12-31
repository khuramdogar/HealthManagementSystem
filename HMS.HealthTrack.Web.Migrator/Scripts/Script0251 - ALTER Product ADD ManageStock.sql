/*
   Friday, May 13, 201610:55:44 AM
   User: 
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
ALTER TABLE Inventory.Product ADD
	ManageStock bit NOT NULL CONSTRAINT DF_Product_ManageStock DEFAULT 0
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

-- Product set to Specify Levels are considered Managed.
UPDATE Inventory.Product
SET ManageStock = 1
WHERE AutoReorderSetting = 0

UPDATE Inventory.Product
SET ManageStock = 1, AutoReorderSetting = 0
WHERE AutoReorderSetting = 2