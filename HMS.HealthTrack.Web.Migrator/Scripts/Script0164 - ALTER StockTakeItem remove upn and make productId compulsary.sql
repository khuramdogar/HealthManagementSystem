/*
   Tuesday, 17 November 20155:30:57 PM
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
ALTER TABLE Inventory.StockTakeItem
	DROP COLUMN UPN
GO
ALTER TABLE Inventory.StockTakeItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


BEGIN TRANSACTION

DELETE FROM Inventory.StockTakeItem
WHERE ProductId IS NULL


ALTER TABLE Inventory.StockTakeItem
ALTER COLUMN ProductId int NOT NULL

COMMIT
