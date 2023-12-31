/*
   Thursday, April 28, 201611:04:36 AM
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
EXECUTE sp_rename N'Inventory.StockDeductionReason', N'StockAdjustmentReason', 'OBJECT' 
GO
EXECUTE sp_rename N'Inventory.StockAdjustmentReason.StockDeductionReasonId', N'Tmp_StockAdjustmentReasonId_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockAdjustmentReason.Tmp_StockAdjustmentReasonId_1', N'StockAdjustmentReasonId', 'COLUMN' 
GO
ALTER TABLE Inventory.StockAdjustmentReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
