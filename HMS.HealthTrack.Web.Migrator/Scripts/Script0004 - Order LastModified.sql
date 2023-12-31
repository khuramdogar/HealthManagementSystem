/*
   Thursday, 22 January 201511:52:02 AM
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
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT FK_Inventory_Order_Inventory_Location
GO
ALTER TABLE Inventory.Location SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT FK_Inventory_Order_Inventory_CostCenter
GO
ALTER TABLE Inventory.CostCenter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT DF_Inventory_Order_StatusId
GO
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT DF_Inventory_Order_PartialShipping
GO
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT DF_Inventory_Order_IsUrgent
GO
CREATE TABLE Inventory.Tmp_Order
	(
	InventoryOrderId int NOT NULL IDENTITY (1, 1),
	DateCreated datetime NOT NULL,
	CreatedBy varchar(100) NULL,
	StatusId int NOT NULL,
	PartialShipping bit NOT NULL,
	Name varchar(200) NULL,
	Notes varchar(MAX) NULL,
	CostCenterId int NULL,
	DeliveryLocationId int NULL,
	NeedBy datetime NULL,
	IsUrgent bit NOT NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy varchar(100) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Order SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE Inventory.Tmp_Order ADD CONSTRAINT
	DF_Order_DateCreated DEFAULT (GETDATE()) FOR DateCreated
GO
ALTER TABLE Inventory.Tmp_Order ADD CONSTRAINT
	DF_Inventory_Order_StatusId DEFAULT ((1)) FOR StatusId
GO
ALTER TABLE Inventory.Tmp_Order ADD CONSTRAINT
	DF_Inventory_Order_PartialShipping DEFAULT ((0)) FOR PartialShipping
GO
ALTER TABLE Inventory.Tmp_Order ADD CONSTRAINT
	DF_Inventory_Order_IsUrgent DEFAULT ((0)) FOR IsUrgent
GO
SET IDENTITY_INSERT Inventory.Tmp_Order ON
GO
IF EXISTS(SELECT * FROM Inventory.[Order])
	 EXEC('INSERT INTO Inventory.Tmp_Order (InventoryOrderId, DateCreated, CreatedBy, StatusId, PartialShipping, Name, Notes, CostCenterId, DeliveryLocationId, NeedBy, IsUrgent)
		SELECT InventoryOrderId, DateCreated, CreatedBy, StatusId, PartialShipping, Name, Notes, CostCenterId, DeliveryLocationId, NeedBy, IsUrgent FROM Inventory.[Order] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_Order OFF
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Inventory_Stock_Inventory_Order
GO
ALTER TABLE Inventory.ProductStockRequest
	DROP CONSTRAINT FK_Inventory_ProductStockRequest_Inventory_Order
GO
ALTER TABLE Inventory.OrderItem
	DROP CONSTRAINT FK_Inventory_Order_Item_Inventory_Order
GO
DROP TABLE Inventory.[Order]
GO
EXECUTE sp_rename N'Inventory.Tmp_Order', N'Order', 'OBJECT' 
GO
ALTER TABLE Inventory.[Order] ADD CONSTRAINT
	PK_Inventory_Order PRIMARY KEY CLUSTERED 
	(
	InventoryOrderId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.[Order] ADD CONSTRAINT
	FK_Inventory_Order_Inventory_CostCenter FOREIGN KEY
	(
	CostCenterId
	) REFERENCES Inventory.CostCenter
	(
	CostCenterId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.[Order] ADD CONSTRAINT
	FK_Inventory_Order_Inventory_Location FOREIGN KEY
	(
	DeliveryLocationId
	) REFERENCES Inventory.Location
	(
	LocationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItem ADD CONSTRAINT
	FK_Inventory_Order_Item_Inventory_Order FOREIGN KEY
	(
	InventoryOrderId
	) REFERENCES Inventory.[Order]
	(
	InventoryOrderId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductStockRequest ADD CONSTRAINT
	FK_Inventory_ProductStockRequest_Inventory_Order FOREIGN KEY
	(
	InventoryOrderId
	) REFERENCES Inventory.[Order]
	(
	InventoryOrderId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Inventory_Stock_Inventory_Order FOREIGN KEY
	(
	InventoryOrderId
	) REFERENCES Inventory.[Order]
	(
	InventoryOrderId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
