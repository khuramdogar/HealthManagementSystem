DECLARE @newProductCategory int

SET @newProductCategory = (SELECT CategoryId FROM Inventory.Category WHERE CategoryName = 'New Product')

UPDATE Inventory.Product
SET PrimaryCategoryId = @newProductCategory
WHERE PrimaryCategoryId IS NULL
GO

/*
   Tuesday, 5 May 20154:02:04 PM
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
ALTER TABLE Inventory.Product
	DROP CONSTRAINT FK_Inventory_Product_Company
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT FK_Inventory_Product_Company1
GO
ALTER TABLE dbo.Company SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT FK_Product_Category
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_UseExpired
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_UseSterile
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_MaxUses
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_IsConsignment
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_CreatedOn
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_MinimumReorder
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_RecommendedReorder
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_OrderMultiple
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Product_UsePrimaryCategorySettings
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Product_UsePrimaryCategoryGLC
GO
CREATE TABLE Inventory.Tmp_Product
	(
	ProductId int NOT NULL IDENTITY (1, 1),
	SPC varchar(50) NULL,
	LPC varchar(50) NULL,
	UPN varchar(50) NULL,
	Description varchar(500) NULL,
	ClinicalGroup varchar(50) NULL,
	ClinicalSubGroup varchar(50) NULL,
	GLC varchar(50) NULL,
	Manufacturer varchar(50) NULL,
	UseExpired bit NOT NULL,
	UseSterile bit NOT NULL,
	MaxUses int NOT NULL,
	Notes varchar(MAX) NULL,
	SpecialRequirements varchar(MAX) NULL,
	PrimarySupplier int NULL,
	SecondarySupplier int NULL,
	IsConsignment bit NOT NULL,
	PriceModelId int NULL,
	RebateCode varchar(50) NULL,
	BuyPrice money NULL,
	BuyCurrency varchar(50) NULL,
	BuyCurrencyRate nvarchar(50) NULL,
	SellPrice money NULL,
	MarkUp decimal(18, 2) NULL,
	DeletedOn datetime NULL,
	DeletedBy varchar(50) NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy varchar(50) NULL,
	CreatedOn datetime NOT NULL,
	CreatedBy varchar(50) NULL,
	MinimumOrder int NOT NULL,
	RecommendedOrder int NOT NULL,
	OrderMultiple int NOT NULL,
	ReorderThreshold int NULL,
	TargetStockLevel int NULL,
	PrimaryCategoryId int NOT NULL,
	UsePrimaryCategorySettings bit NOT NULL,
	UsePrimaryCategoryGLC bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_Product SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_UseExpired DEFAULT ((0)) FOR UseExpired
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_UseSterile DEFAULT ((0)) FOR UseSterile
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_MaxUses DEFAULT ((1)) FOR MaxUses
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_IsConsignment DEFAULT ((0)) FOR IsConsignment
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_CreatedOn DEFAULT (getdate()) FOR CreatedOn
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_MinimumReorder DEFAULT ((1)) FOR MinimumOrder
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_RecommendedReorder DEFAULT ((1)) FOR RecommendedOrder
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Inventory_Product_OrderMultiple DEFAULT ((1)) FOR OrderMultiple
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Product_UsePrimaryCategorySettings DEFAULT ((0)) FOR UsePrimaryCategorySettings
GO
ALTER TABLE Inventory.Tmp_Product ADD CONSTRAINT
	DF_Product_UsePrimaryCategoryGLC DEFAULT ((0)) FOR UsePrimaryCategoryGLC
GO
SET IDENTITY_INSERT Inventory.Tmp_Product ON
GO
IF EXISTS(SELECT * FROM Inventory.Product)
	 EXEC('INSERT INTO Inventory.Tmp_Product (ProductId, SPC, LPC, UPN, Description, ClinicalGroup, ClinicalSubGroup, GLC, Manufacturer, UseExpired, UseSterile, MaxUses, Notes, SpecialRequirements, PrimarySupplier, SecondarySupplier, IsConsignment, PriceModelId, RebateCode, BuyPrice, BuyCurrency, BuyCurrencyRate, SellPrice, MarkUp, DeletedOn, DeletedBy, LastModifiedOn, LastModifiedBy, CreatedOn, CreatedBy, MinimumOrder, RecommendedOrder, OrderMultiple, ReorderThreshold, TargetStockLevel, PrimaryCategoryId, UsePrimaryCategorySettings, UsePrimaryCategoryGLC)
		SELECT ProductId, SPC, LPC, UPN, Description, ClinicalGroup, ClinicalSubGroup, GLC, Manufacturer, UseExpired, UseSterile, MaxUses, Notes, SpecialRequirements, PrimarySupplier, SecondarySupplier, IsConsignment, PriceModelId, RebateCode, BuyPrice, BuyCurrency, BuyCurrencyRate, SellPrice, MarkUp, DeletedOn, DeletedBy, LastModifiedOn, LastModifiedBy, CreatedOn, CreatedBy, MinimumOrder, RecommendedOrder, OrderMultiple, ReorderThreshold, TargetStockLevel, PrimaryCategoryId, UsePrimaryCategorySettings, UsePrimaryCategoryGLC FROM Inventory.Product WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_Product OFF
GO
ALTER TABLE Inventory.StockSetItem
	DROP CONSTRAINT FK_Inventory_StockSetItem_Inventory_Product
GO
ALTER TABLE Inventory.ProductSetting
	DROP CONSTRAINT FK_ProductSetting_Product
GO
ALTER TABLE Inventory.Stock
	DROP CONSTRAINT FK_Inventory_Stock_Inventory_Product
GO
ALTER TABLE Inventory.ProductStockRequest
	DROP CONSTRAINT FK_Inventory_ProductStockRequest_Inventory_Product
GO
ALTER TABLE Inventory.ProductPrice
	DROP CONSTRAINT FK_Inventory_Product_Price_Inventory_Product
GO
ALTER TABLE Inventory.Product_Category
	DROP CONSTRAINT FK_Inventory_Master_Category_Inventory_Product
GO
ALTER TABLE Inventory.ExternalProductMapping
	DROP CONSTRAINT FK_Inventory_ExternalProductMapping_Inventory_Product
GO
ALTER TABLE Inventory.OrderItem
	DROP CONSTRAINT FK_Inventory_Order_Item_Inventory_Product
GO
DROP TABLE Inventory.Product
GO
EXECUTE sp_rename N'Inventory.Tmp_Product', N'Product', 'OBJECT' 
GO
ALTER TABLE Inventory.Product ADD CONSTRAINT
	PK_Inventory_Product PRIMARY KEY CLUSTERED 
	(
	ProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.Product ADD CONSTRAINT
	FK_Product_Category FOREIGN KEY
	(
	PrimaryCategoryId
	) REFERENCES Inventory.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Product ADD CONSTRAINT
	FK_Inventory_Product_Company FOREIGN KEY
	(
	PrimarySupplier
	) REFERENCES dbo.Company
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Product ADD CONSTRAINT
	FK_Inventory_Product_Company1 FOREIGN KEY
	(
	SecondarySupplier
	) REFERENCES dbo.Company
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItem ADD CONSTRAINT
	FK_Inventory_Order_Item_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ExternalProductMapping ADD CONSTRAINT
	FK_Inventory_ExternalProductMapping_Inventory_Product FOREIGN KEY
	(
	InventoryProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ExternalProductMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product_Category ADD CONSTRAINT
	FK_Inventory_Master_Category_Inventory_Product FOREIGN KEY
	(
	Inv_ID
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Product_Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductPrice ADD CONSTRAINT
	FK_Inventory_Product_Price_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductPrice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductStockRequest ADD CONSTRAINT
	FK_Inventory_ProductStockRequest_Inventory_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductStockRequest SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.ProductSetting ADD CONSTRAINT
	FK_ProductSetting_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.ProductSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE Inventory.StockSetItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
