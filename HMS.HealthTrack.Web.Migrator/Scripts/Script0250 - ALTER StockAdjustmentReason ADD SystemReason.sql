/*
   Friday, May 13, 20169:45:16 AM
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
ALTER TABLE Inventory.StockAdjustmentReason ADD
	IsSystemReason bit NOT NULL CONSTRAINT DF_StockAdjustmentReason_SystemReason DEFAULT 0
GO
ALTER TABLE Inventory.StockAdjustmentReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

UPDATE Inventory.StockAdjustmentReason
SET IsSystemReason = 1
WHERE Name = 'Stock management write off'