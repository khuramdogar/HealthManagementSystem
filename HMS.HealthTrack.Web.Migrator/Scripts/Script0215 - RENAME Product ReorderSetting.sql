/*
   Thursday, February 18, 201611:43:05 AM
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
EXECUTE sp_rename N'Inventory.Product.ReorderSetting', N'Tmp_AutoReorderSetting', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.Product.Tmp_AutoReorderSetting', N'AutoReorderSetting', 'COLUMN' 
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
