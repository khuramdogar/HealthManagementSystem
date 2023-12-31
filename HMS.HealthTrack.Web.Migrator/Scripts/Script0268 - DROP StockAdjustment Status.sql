/*
   Friday, May 27, 20163:47:00 PM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
   Database: HealthTrack_Web_v2
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
ALTER TABLE Inventory.StockAdjustment
	DROP CONSTRAINT DF_Inventory_Consumption_Status
GO
ALTER TABLE Inventory.StockAdjustment
	DROP COLUMN Status
GO
ALTER TABLE Inventory.StockAdjustment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
