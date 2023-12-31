/*
   Friday, 13 February 20154:26:04 PM
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
ALTER TABLE Inventory.StockSetItem
	DROP CONSTRAINT FK_Inventory_StockSetItem_Inventory_StockSet
GO
ALTER TABLE Inventory.StockSet SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockSetItem
	DROP CONSTRAINT FK_Inventory_StockSetItem_Inventory_Product
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_StockSetItem
	(
	StockSetItemId int NOT NULL IDENTITY (1, 1),
	ProductId int NOT NULL,
	StockSetId int NOT NULL,
	Quantity int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockSetItem SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_StockSetItem OFF
GO
IF EXISTS(SELECT * FROM Inventory.StockSetItem)
	 EXEC('INSERT INTO Inventory.Tmp_StockSetItem (ProductId, StockSetId, Quantity)
		SELECT ProductId, StockSetId, Quantity FROM Inventory.StockSetItem WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Inventory.StockSetItem
GO
EXECUTE sp_rename N'Inventory.Tmp_StockSetItem', N'StockSetItem', 'OBJECT' 
GO
ALTER TABLE Inventory.StockSetItem ADD CONSTRAINT
	PK_StockSetItem PRIMARY KEY CLUSTERED 
	(
	StockSetItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX Unique_StockSet_Product ON Inventory.StockSetItem
	(
	StockSetId,
	ProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.StockSetItem ADD CONSTRAINT
	FK_Inventory_StockSetItem_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockSetItem ADD CONSTRAINT
	FK_Inventory_StockSetItem_Inventory_StockSet FOREIGN KEY
	(
	StockSetId
	) REFERENCES Inventory.StockSet
	(
	StockSetId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
