/*
   Tuesday, 20 September 201610:36:35 AM
   User: sa
   Server: bolton
   Database: HealthTrack_Web_DevNeil
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
ALTER TABLE Inventory.StockAdjustmentStock
	DROP CONSTRAINT FK_StockAdjustmentStock_StockAdjustmentStock
GO
ALTER TABLE Inventory.StockAdjustmentStock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
