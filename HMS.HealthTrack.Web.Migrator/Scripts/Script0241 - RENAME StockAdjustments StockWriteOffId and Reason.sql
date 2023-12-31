/*
   Tuesday, May 3, 201612:12:59 PM
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
EXECUTE sp_rename N'Inventory.StockAdjustment.StockWriteOffId', N'Tmp_StockTakeItemId_2', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockAdjustment.Reason', N'Tmp_StockAdjustmentReasonId_3', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockAdjustment.Tmp_StockTakeItemId_2', N'StockTakeItemId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockAdjustment.Tmp_StockAdjustmentReasonId_3', N'StockAdjustmentReasonId', 'COLUMN' 
GO
ALTER TABLE Inventory.StockAdjustment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
