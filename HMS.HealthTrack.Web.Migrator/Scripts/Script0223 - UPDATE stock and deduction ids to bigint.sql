/*
   Monday, 14 March 201610:52:24 AM
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
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Inventory_Stock_Inventory_Product
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Inventory_Stock_Location
GO
ALTER TABLE Inventory.Location SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT FK_StockDeduction_GeneralLedger
GO
ALTER TABLE Inventory.GeneralLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT FK_StockDeduction_StockDeductionReason
GO
ALTER TABLE Inventory.StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_ProductStockRequest
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_OrderItem
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Stock_OrderItem
GO
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT FK_Consumption_StockTakeItem
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Stock_StockTakeItem
GO
ALTER TABLE Inventory.StockTakeItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT DF_Inventory_Stock_Quantity
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT DF_Inventory_Stock_CreatedOn
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT DF_Inventory_Stock_StockStatus
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT DF_Inventory_Stock_ReceivedQuantity
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT DF_Stock_IsNegative
GO
CREATE TABLE Inventory.Tmp_Stock
	(
	StockId bigint NOT NULL IDENTITY (1, 1),
	ProductId int NOT NULL,
	Quantity int NOT NULL,
	ReceivedOn datetime NOT NULL,
	BoughtPrice money NULL,
	BatchNumber varchar(50) NULL,
	ExpiresOn datetime NULL,
	SerialNumber varchar(150) NULL,
	CreatedOn datetime NULL,
	CreatedBy varchar(100) NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy varchar(100) NULL,
	DeletedOn datetime NULL,
	DeletedBy varchar(100) NULL,
	DeletionReason varchar(500) NULL,
	StockStatus int NOT NULL,
	Owner int NULL,
	StoredAt int NOT NULL,
	PriceModelOnReceipt int NULL,
	TaxRateOnReceipt money NULL,
	SellPrice money NULL,
	ReceivedQuantity int NOT NULL,
	OrderItemId int NULL,
	StockTakeItemId int NULL,
	IsNegative bit NOT NULL,
	ReceivedBy varchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Stock SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE Inventory.Tmp_Stock ADD CONSTRAINT
	DF_Inventory_Stock_Quantity DEFAULT ((1)) FOR Quantity
GO
ALTER TABLE Inventory.Tmp_Stock ADD CONSTRAINT
	DF_Inventory_Stock_CreatedOn DEFAULT (getdate()) FOR CreatedOn
GO
ALTER TABLE Inventory.Tmp_Stock ADD CONSTRAINT
	DF_Inventory_Stock_StockStatus DEFAULT ((0)) FOR StockStatus
GO
ALTER TABLE Inventory.Tmp_Stock ADD CONSTRAINT
	DF_Inventory_Stock_ReceivedQuantity DEFAULT ((1)) FOR ReceivedQuantity
GO
ALTER TABLE Inventory.Tmp_Stock ADD CONSTRAINT
	DF_Stock_IsNegative DEFAULT ((0)) FOR IsNegative
GO
SET IDENTITY_INSERT Inventory.Tmp_Stock ON
GO
IF EXISTS(SELECT * FROM Inventory.Stock)
	 EXEC('INSERT INTO Inventory.Tmp_Stock (StockId, ProductId, Quantity, ReceivedOn, BoughtPrice, BatchNumber, ExpiresOn, SerialNumber, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy, DeletedOn, DeletedBy, DeletionReason, StockStatus, Owner, StoredAt, PriceModelOnReceipt, TaxRateOnReceipt, SellPrice, ReceivedQuantity, OrderItemId, StockTakeItemId, IsNegative, ReceivedBy)
		SELECT CONVERT(bigint, StockId), ProductId, Quantity, ReceivedOn, BoughtPrice, BatchNumber, ExpiresOn, SerialNumber, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy, DeletedOn, DeletedBy, DeletionReason, StockStatus, Owner, StoredAt, PriceModelOnReceipt, TaxRateOnReceipt, SellPrice, ReceivedQuantity, OrderItemId, StockTakeItemId, IsNegative, ReceivedBy FROM Inventory.Stock WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_Stock OFF
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT FK_Inventory_Consumption_Inventory_Stock
GO
DROP TABLE Inventory.Stock
GO
EXECUTE sp_rename N'Inventory.Tmp_Stock', N'Stock', 'OBJECT' 
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	PK_Inventory_Stock PRIMARY KEY CLUSTERED 
	(
	StockId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Inventory_Stock_Location FOREIGN KEY
	(
	StoredAt
	) REFERENCES Inventory.Location
	(
	LocationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Inventory_Stock_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Stock_StockTakeItem FOREIGN KEY
	(
	StockTakeItemId
	) REFERENCES Inventory.StockTakeItem
	(
	StockTakeItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Stock ADD CONSTRAINT
	FK_Stock_OrderItem FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT DF_Consumption_CreatedOn
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT DF_Inventory_Consumption_QuantityConsumed
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT DF_Inventory_Consumption_Status
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT DF_Inventory_Consumption_Source
GO
CREATE TABLE Inventory.Tmp_StockDeduction
	(
	StockDeductionId bigint NOT NULL IDENTITY (1, 1),
	StockId bigint NOT NULL,
	ConsumedOn datetime NULL,
	ConsumedBy varchar(100) NULL,
	CreatedBy varchar(100) NULL,
	CreatedOn datetime NOT NULL,
	DeletedOn datetime NULL,
	DeletedBy varchar(100) NULL,
	DeletionReason varchar(500) NULL,
	QuantityConsumed int NOT NULL,
	Status int NOT NULL,
	PatientId int NULL,
	ClinicalRecordId bigint NULL,
	Source int NOT NULL,
	LastModifiedBy varchar(100) NULL,
	LastModifiedOn datetime NULL,
	StockWriteOffId int NULL,
	Reason int NULL,
	GeneralLedgerId int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockDeduction SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE Inventory.Tmp_StockDeduction ADD CONSTRAINT
	DF_Consumption_CreatedOn DEFAULT (getdate()) FOR CreatedOn
GO
ALTER TABLE Inventory.Tmp_StockDeduction ADD CONSTRAINT
	DF_Inventory_Consumption_QuantityConsumed DEFAULT ((1)) FOR QuantityConsumed
GO
ALTER TABLE Inventory.Tmp_StockDeduction ADD CONSTRAINT
	DF_Inventory_Consumption_Status DEFAULT ((0)) FOR Status
GO
ALTER TABLE Inventory.Tmp_StockDeduction ADD CONSTRAINT
	DF_Inventory_Consumption_Source DEFAULT ((0)) FOR Source
GO
SET IDENTITY_INSERT Inventory.Tmp_StockDeduction ON
GO
IF EXISTS(SELECT * FROM Inventory.StockDeduction)
	 EXEC('INSERT INTO Inventory.Tmp_StockDeduction (StockDeductionId, StockId, ConsumedOn, ConsumedBy, CreatedBy, CreatedOn, DeletedOn, DeletedBy, DeletionReason, QuantityConsumed, Status, PatientId, ClinicalRecordId, Source, LastModifiedBy, LastModifiedOn, StockWriteOffId, Reason, GeneralLedgerId)
		SELECT CONVERT(bigint, StockDeductionId), CONVERT(bigint, StockId), ConsumedOn, ConsumedBy, CreatedBy, CreatedOn, DeletedOn, DeletedBy, DeletionReason, QuantityConsumed, Status, PatientId, ClinicalRecordId, Source, LastModifiedBy, LastModifiedOn, StockWriteOffId, Reason, GeneralLedgerId FROM Inventory.StockDeduction WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_StockDeduction OFF
GO
ALTER TABLE Inventory.OrderItemSource
	DROP CONSTRAINT FK_OrderItemSource_Consumption
GO
DROP TABLE Inventory.StockDeduction
GO
EXECUTE sp_rename N'Inventory.Tmp_StockDeduction', N'StockDeduction', 'OBJECT' 
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	PK_Inventory_Consumption PRIMARY KEY CLUSTERED 
	(
	StockDeductionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_Consumption_StockTakeItem FOREIGN KEY
	(
	StockWriteOffId
	) REFERENCES Inventory.StockTakeItem
	(
	StockTakeItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_StockDeduction_StockDeductionReason FOREIGN KEY
	(
	Reason
	) REFERENCES Inventory.StockDeductionReason
	(
	StockDeductionReasonId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_StockDeduction_GeneralLedger FOREIGN KEY
	(
	GeneralLedgerId
	) REFERENCES Inventory.GeneralLedger
	(
	LedgerId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_Inventory_Consumption_Inventory_Stock FOREIGN KEY
	(
	StockId
	) REFERENCES Inventory.Stock
	(
	StockId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_OrderItemSource
	(
	ItemSourceId int NOT NULL IDENTITY (1, 1),
	OrderItemId int NOT NULL,
	ConsumptionId bigint NULL,
	StockRequestId int NULL,
	Quantity int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_OrderItemSource SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_OrderItemSource ON
GO
IF EXISTS(SELECT * FROM Inventory.OrderItemSource)
	 EXEC('INSERT INTO Inventory.Tmp_OrderItemSource (ItemSourceId, OrderItemId, ConsumptionId, StockRequestId, Quantity)
		SELECT ItemSourceId, OrderItemId, CONVERT(bigint, ConsumptionId), StockRequestId, Quantity FROM Inventory.OrderItemSource WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_OrderItemSource OFF
GO
DROP TABLE Inventory.OrderItemSource
GO
EXECUTE sp_rename N'Inventory.Tmp_OrderItemSource', N'OrderItemSource', 'OBJECT' 
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	PK_OrderItemSource PRIMARY KEY CLUSTERED 
	(
	ItemSourceId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_OrderItem FOREIGN KEY
	(
	OrderItemId
	) REFERENCES Inventory.OrderItem
	(
	OrderItemId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_Consumption FOREIGN KEY
	(
	ConsumptionId
	) REFERENCES Inventory.StockDeduction
	(
	StockDeductionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_ProductStockRequest FOREIGN KEY
	(
	StockRequestId
	) REFERENCES Inventory.ProductStockRequest
	(
	StockRequestId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
