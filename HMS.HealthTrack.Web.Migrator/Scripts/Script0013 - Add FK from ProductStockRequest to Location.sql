/*
   Wednesday, 4 February 20153:00:29 PM
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
ALTER TABLE Inventory.Location SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductStockRequest ADD CONSTRAINT
	FK_ProductStockRequest_Location FOREIGN KEY
	(
	RequestLocationId
	) REFERENCES Inventory.Location
	(
	LocationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
