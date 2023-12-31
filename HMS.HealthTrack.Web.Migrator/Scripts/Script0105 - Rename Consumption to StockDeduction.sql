/*
   Thursday, 27 August 20154:05:08 PM
   User: 
   Server: DEV-NEIL
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
EXECUTE sp_rename N'Inventory.Consumption', N'StockDeduction', 'OBJECT' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.ConsumptionId', N'Tmp_StockDeductionId', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.StockDeduction.Tmp_StockDeductionId', N'StockDeductionId', 'COLUMN' 
GO
ALTER TABLE Inventory.StockDeduction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
