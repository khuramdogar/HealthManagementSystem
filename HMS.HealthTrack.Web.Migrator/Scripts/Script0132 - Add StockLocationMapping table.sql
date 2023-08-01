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
ALTER TABLE dbo.Location SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.StockLocationMapping
	(
	LocationId int NOT NULL,
	HealthTrackLocationId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockLocationMapping ADD CONSTRAINT
	FK_StockLocationMapping_HealthTrackLocation FOREIGN KEY
	(
	HealthTrackLocationId
	) REFERENCES dbo.Location
	(
	location_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockLocationMapping ADD CONSTRAINT
	FK_StockLocationMapping_Location FOREIGN KEY
	(
	LocationId
	) REFERENCES Inventory.Location
	(
	LocationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockLocationMapping ADD CONSTRAINT
	PK_StockLocationMapping PRIMARY KEY CLUSTERED 
	(
	LocationId,
	HealthTrackLocationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockLocationMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
