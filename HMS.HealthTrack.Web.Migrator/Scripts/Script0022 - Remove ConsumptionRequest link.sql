/*
   Tuesday, 24 February 20154:21:16 PM
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
ALTER TABLE Inventory.ProductStockRequest
	DROP CONSTRAINT FK_Inventory_ProductStockRequest_Inventory_Consumption
GO
ALTER TABLE Inventory.Consumption SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductStockRequest
	DROP COLUMN ConsumptionId
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
