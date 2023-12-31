/*
   Friday, 30 January 201511:49:22 AM
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
ALTER TABLE Inventory.Consumption
	DROP CONSTRAINT FK_Inventory_Consumption_Inventory_Stock
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Consumption
	DROP CONSTRAINT DF_Inventory_Consumption_QuantityConsumed
GO
ALTER TABLE Inventory.Consumption
	DROP CONSTRAINT DF_Inventory_Consumption_Status
GO
ALTER TABLE Inventory.Consumption
	DROP CONSTRAINT DF_Inventory_Consumption_Source
GO
CREATE TABLE Inventory.Tmp_Consumption
	(
	ConsumptionId int NOT NULL IDENTITY (1, 1),
	StockId int NOT NULL,
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
	PaymentClass tinyint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Consumption SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE Inventory.Tmp_Consumption ADD CONSTRAINT
	DF_Consumption_CreatedOn DEFAULT (getdate()) FOR CreatedOn
GO
ALTER TABLE Inventory.Tmp_Consumption ADD CONSTRAINT
	DF_Inventory_Consumption_QuantityConsumed DEFAULT ((1)) FOR QuantityConsumed
GO
ALTER TABLE Inventory.Tmp_Consumption ADD CONSTRAINT
	DF_Inventory_Consumption_Status DEFAULT ((0)) FOR Status
GO
ALTER TABLE Inventory.Tmp_Consumption ADD CONSTRAINT
	DF_Inventory_Consumption_Source DEFAULT ((0)) FOR Source
GO
SET IDENTITY_INSERT Inventory.Tmp_Consumption ON
GO
IF EXISTS(SELECT * FROM Inventory.Consumption)
	 EXEC('INSERT INTO Inventory.Tmp_Consumption (ConsumptionId, StockId, ConsumedOn, ConsumedBy, CreatedBy, CreatedOn, DeletedOn, DeletedBy, DeletionReason, QuantityConsumed, Status, PatientId, ClinicalRecordId, Source, PaymentClass)
		SELECT ConsumptionId, StockId, ConsumedOn, ConsumedBy, CreatedBy, CreatedOn, DeletedOn, DeletedBy, DeletionReason, QuantityConsumed, Status, PatientId, ClinicalRecordId, Source, PaymentClass FROM Inventory.Consumption WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_Consumption OFF
GO
ALTER TABLE Inventory.ProductStockRequest
	DROP CONSTRAINT FK_Inventory_ProductStockRequest_Inventory_Consumption
GO
DROP TABLE Inventory.Consumption
GO
EXECUTE sp_rename N'Inventory.Tmp_Consumption', N'Consumption', 'OBJECT' 
GO
ALTER TABLE Inventory.Consumption ADD CONSTRAINT
	PK_Inventory_Consumption PRIMARY KEY CLUSTERED 
	(
	ConsumptionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.Consumption ADD CONSTRAINT
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
ALTER TABLE Inventory.ProductStockRequest ADD CONSTRAINT
	FK_Inventory_ProductStockRequest_Inventory_Consumption FOREIGN KEY
	(
	ConsumptionId
	) REFERENCES Inventory.Consumption
	(
	ConsumptionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
