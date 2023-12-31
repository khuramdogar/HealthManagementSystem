/*
   Friday, April 22, 20169:59:39 AM
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
EXECUTE sp_rename N'Inventory.StockDeduction.StockDeductionId', N'Tmp_StockAdjustmentId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.ConsumedOn', N'Tmp_AdjustedOn_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.ConsumedBy', N'Tmp_AdjustedBy_2', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.QuantityConsumed', N'Tmp_Quantity_3', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.Tmp_StockAdjustmentId', N'StockAdjustmentId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.Tmp_AdjustedOn_1', N'AdjustedOn', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.Tmp_AdjustedBy_2', N'AdjustedBy', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.Tmp_Quantity_3', N'Quantity', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction', N'StockAdjustment', 'OBJECT' 
GO

EXECUTE sp_rename N'Inventory.OrderItemSource.ConsumptionId', N'Tmp_StockAdjustmentId_1', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.OrderItemSource.Tmp_StockAdjustmentId_1', N'StockAdjustmentId', 'COLUMN' 
GO

COMMIT