/*
   Wednesday, 24 June 20152:10:41 PM
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
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.StockTake
   (
   StockTakeId int NOT NULL IDENTITY (1, 1),
   StockDateDate datetime NULL,
   Status int NOT NULL,
   Message varchar(100) NULL,
   CreatedOn datetime NOT NULL,
   CreatedBy varchar(50) NULL,
   ModifiedOn datetime NULL,
   ModifiedBy varchar(50) NULL,
   DeletedOn datetime NULL,
   DeletedBy varchar(50) NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockTake ADD CONSTRAINT
   PK_StockTake PRIMARY KEY CLUSTERED 
   (
   StockTakeId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockTake SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.StockTakeItem
   (
   StockTakeItemId int NOT NULL IDENTITY (1, 1),
   StokeTakeId int NOT NULL,
   SPC varchar(50) NULL,
   ProductId int NULL,
   Status int NOT NULL,
   Message varchar(200) NULL,
   CreatedOn datetime NOT NULL,
   CreatedBy varchar(50) NULL,
   ModifiedOn datetime NULL,
   ModifiedBy varchar(50) NULL,
   DeletedOn datetime NULL,
   DeletedBy varchar(50) NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockTakeItem ADD CONSTRAINT
   PK_StockTakeItem PRIMARY KEY CLUSTERED 
   (
   StockTakeItemId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockTakeItem ADD CONSTRAINT
   FK_StockTakeItem_StockTake FOREIGN KEY
   (
   StokeTakeId
   ) REFERENCES Inventory.StockTake
   (
   StockTakeId
   ) ON UPDATE  NO ACTION 
    ON DELETE  NO ACTION 
   
GO
ALTER TABLE Inventory.StockTakeItem ADD CONSTRAINT
   FK_StockTakeItem_Product FOREIGN KEY
   (
   ProductId
   ) REFERENCES Inventory.Product
   (
   ProductId
   ) ON UPDATE  NO ACTION 
    ON DELETE  NO ACTION 
   
GO
ALTER TABLE Inventory.StockTakeItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
