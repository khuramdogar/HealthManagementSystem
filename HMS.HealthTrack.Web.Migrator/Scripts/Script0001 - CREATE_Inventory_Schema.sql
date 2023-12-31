/****** Object:  Schema [Inventory]    Script Date: 01/20/2015 14:33:48 ******/
CREATE SCHEMA [Inventory] AUTHORIZATION [dbo]
GO
/****** Object:  Table [Inventory].[PriceType]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[PriceType](
	[PriceTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
 CONSTRAINT [PK_Inventory_PriceType] PRIMARY KEY CLUSTERED 
(
	[PriceTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[StockSet]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[StockSet](
	[StockSetId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](50) NULL,
 CONSTRAINT [PK_Inventory_StockSet] PRIMARY KEY CLUSTERED 
(
	[StockSetId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Category]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Category](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NULL,
	[CategoryName] [varchar](250) NOT NULL,
	[Deleted] [bit] NOT NULL,
	[DeletedBy] [varchar](50) NULL,
	[DeletedOn] [datetime] NULL,
	[LastModifiedDate] [datetime] NULL,
	[LastModifiedUser] [varchar](50) NULL,
	[CreationDate] [datetime] NOT NULL,
	[UserCreated] [varchar](50) NULL,
 CONSTRAINT [PK_Inventory_Category] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Address]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Address](
	[AddressId] [int] IDENTITY(1,1) NOT NULL,
	[Address1] [varchar](50) NULL,
	[Address2] [varchar](50) NULL,
	[Suburb] [varchar](50) NULL,
	[State] [varchar](10) NULL,
	[PostCode] [varchar](10) NULL,
	[Country] [varchar](50) NULL,
	[Department] [varchar](50) NULL,
 CONSTRAINT [PK_Inventory_Address] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[CostCenter]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[CostCenter](
	[CostCenterId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedUser] [varchar](50) NULL,
	[LastModifiedDate] [datetime] NULL,
	[DeletedBy] [varchar](50) NULL,
	[DeletedOn] [datetime] NULL,
 CONSTRAINT [PK_Inventory_CostCenter] PRIMARY KEY CLUSTERED 
(
	[CostCenterId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Oracle_PO_Receipts]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Oracle_PO_Receipts](
	[PurchaseOrderReceiptId] [int] IDENTITY(1,1) NOT NULL,
	[InventoryOrderId] [bigint] NOT NULL,
	[InventoryOrderItemId] [bigint] NOT NULL,
	[OracleRequisitionNumber] [bigint] NOT NULL,
	[OracleRequisitionLineNumber] [bigint] NOT NULL,
	[OracleRequisitionDistributionNumber] [bigint] NOT NULL,
	[OracleDistribution] [varchar](max) NOT NULL,
	[OracleVendorCode] [bigint] NOT NULL,
	[OracleSupplierProductCode] [varchar](max) NOT NULL,
	[OracleHospitalProductCode] [varchar](max) NOT NULL,
	[OraclePricePerUnit] [float] NOT NULL,
	[OracleUnitOfMeasure] [varchar](25) NOT NULL,
	[OracleOrgId] [varchar](3) NOT NULL,
 CONSTRAINT [PK_Inventory_Oracle_PO_Receipts] PRIMARY KEY CLUSTERED 
(
	[PurchaseOrderReceiptId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Oracle_PO_Errors]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Oracle_PO_Errors](
	[PurchaseOrderErrorId] [int] IDENTITY(1,1) NOT NULL,
	[InventoryOrderId] [bigint] NOT NULL,
	[InventoryOrderItemId] [bigint] NOT NULL,
	[ItemDescription] [varchar](max) NOT NULL,
	[RequestId] [bigint] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[ProcessFlag] [varchar](10) NOT NULL,
	[InterfaceSourceCode] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Inventory_Oracle_PO_Errors] PRIMARY KEY CLUSTERED 
(
	[PurchaseOrderErrorId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Property]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Inventory].[Property](
	[PropertyId] [int] IDENTITY(1,1) NOT NULL,
	[PropertyName] [nvarchar](50) NOT NULL,
	[PropertyValue] [nvarchar](200) NULL,
	[Description] [nvarchar](300) NULL,
	[PropertyType] [int] NOT NULL,
 CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED 
(
	[PropertyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Inventory].[Location]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Location](
	[LocationId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](50) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedUser] [varchar](50) NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](50) NULL,
	[AddressId] [int] NULL,
	[IsStockLocation] [bit] NOT NULL,
	[LogoImage] [varbinary](max) NULL,
 CONSTRAINT [PK_Inventory_Location] PRIMARY KEY CLUSTERED 
(
	[LocationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Order]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Order](
	[InventoryOrderId] [int] IDENTITY(1,1) NOT NULL,
	[DateCreated] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[StatusId] [int] NOT NULL,
	[PartialShipping] [bit] NOT NULL,
	[Name] [varchar](200) NULL,
	[Notes] [varchar](max) NULL,
	[CostCenterId] [int] NULL,
	[DeliveryLocationId] [int] NULL,
	[NeedBy] [datetime] NULL,
	[IsUrgent] [bit] NOT NULL,
 CONSTRAINT [PK_Inventory_Order] PRIMARY KEY CLUSTERED 
(
	[InventoryOrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Product]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Product](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[SPC] [varchar](50) NULL,
	[LPC] [varchar](50) NULL,
	[UPN] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[ClinicalGroup] [varchar](50) NULL,
	[ClinicalSubGroup] [varchar](50) NULL,
	[GLC] [varchar](50) NULL,
	[Manufacturer] [varchar](50) NULL,
	[UseExpired] [bit] NOT NULL,
	[UseSterile] [bit] NOT NULL,
	[MaxUses] [int] NOT NULL,
	[Notes] [varchar](max) NULL,
	[SpecialRequirements] [varchar](max) NULL,
	[PrimarySupplier] [int] NULL,
	[SecondarySupplier] [int] NULL,
	[RequiresSerialNumber] [bit] NOT NULL,
	[RequiresBatchNumber] [bit] NOT NULL,
	[IsConsignment] [bit] NOT NULL,
	[Prosthetic] [bit] NOT NULL,
	[PriceModelId] [int] NULL,
	[RebateCode] [varchar](50) NULL,
	[MinBenefit] [money] NULL,
	[MaxBenefit] [money] NULL,
	[BuyPrice] [money] NULL,
	[BuyCurrency] [varchar](50) NULL,
	[BuyCurrencyRate] [nvarchar](50) NULL,
	[SellPrice] [money] NULL,
	[MarkUp] [decimal](18, 2) NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](50) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [varchar](50) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](50) NULL,
	[MinimumOrder] [int] NOT NULL,
	[RecommendedOrder] [int] NOT NULL,
	[OrderMultiple] [int] NOT NULL,
	[ReorderThreshold] [int] NULL,
	[TargetStockLevel] [int] NULL,
	[OrderableUnits] [int] NOT NULL,
 CONSTRAINT [PK_Inventory_Product] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[StockSetItem]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Inventory].[StockSetItem](
	[ProductId] [int] NOT NULL,
	[StockSetId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_Inventory_StockSetItem] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC,
	[StockSetId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Inventory].[Stock]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Stock](
	[StockId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[RecievedOn] [datetime] NOT NULL,
	[BoughtPrice] [money] NULL,
	[BatchNumber] [varchar](50) NULL,
	[ExpiresOn] [datetime] NULL,
	[SerialNumber] [varchar](150) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [varchar](100) NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](100) NULL,
	[DeletionReason] [varchar](500) NULL,
	[StockStatus] [int] NOT NULL,

	[Owner] [int] NULL,
	[StoredAt] [int] NULL,
	[PriceModelOnReceipt] [int] NULL,
	[TaxRateOnReceipt] [money] NULL,
	[SellPrice] [money] NULL,
	[ReceivedQuantity] [int] NOT NULL,
	[InventoryOrderId] [int] NULL,
 CONSTRAINT [PK_Inventory_Stock] PRIMARY KEY CLUSTERED 
(
	[StockId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[ProductPrice]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[ProductPrice](
	[PriceId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[BuyPrice] [money] NULL,
	[BuyCurrency] [varchar](50) NULL,
	[BuyCurrencyRate] [nvarchar](50) NULL,
	[SellPrice] [money] NULL,
	[PriceTypeId] [int] NOT NULL,
 CONSTRAINT [PK_Inventory_Product_Price] PRIMARY KEY CLUSTERED 
(
	[PriceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Product_Category]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Inventory].[Product_Category](
	[InventoryCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Inv_ID] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
 CONSTRAINT [PK_Inventory_Master_Category] PRIMARY KEY CLUSTERED 
(
	[InventoryCategoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Inventory].[OrderItem]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Inventory].[OrderItem](
	[OrderItemId] [int] IDENTITY(1,1) NOT NULL,
	[InventoryOrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NULL,
	[IncludeRebateCode] [bit] NOT NULL,
	[InBulk] [bit] NOT NULL,
 CONSTRAINT [PK_Inventory_Order_Item] PRIMARY KEY CLUSTERED 
(
	[OrderItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [OrderItem_Product_Order_Unique] UNIQUE NONCLUSTERED 
(
	[InventoryOrderId] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Inventory].[ExternalProductMapping]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[ExternalProductMapping](
	[ProductMappingId] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[ExternalProductId] [int] NOT NULL,
	[InventoryProductId] [int] NOT NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](50) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [varchar](50) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_Inventory_ExternalProductMapping] PRIMARY KEY CLUSTERED 
(
	[ProductMappingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[Consumption]    Script Date: 01/20/2015 14:33:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[Consumption](
	[ConsumptionId] [int] IDENTITY(1,1) NOT NULL,
	[StockId] [int] NOT NULL,
	[ConsumedOn] [datetime] NULL,
	[ConsumedBy] [varchar](100) NULL,
	[CreatedBy] [varchar](100) NULL,
	[CreatedOn] [datetime] NULL,
	[DeletedOn] [datetime] NULL,
	[DeletedBy] [varchar](100) NULL,
	[DeletionReason] [varchar](500) NULL,
	[QuantityConsumed] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[PatientId] [int] NULL,
	[ClinicalRecordId] [bigint] NULL,
	[Source] [int] NOT NULL,
	[PaymentClass] [tinyint] NULL,
 CONSTRAINT [PK_Inventory_Consumption] PRIMARY KEY CLUSTERED 
(
	[ConsumptionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Inventory].[ProductStockRequest]    Script Date: 01/20/2015 14:33:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Inventory].[ProductStockRequest](
	[StockRequestId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](50) NULL,
	[RequestStatus] [int] NOT NULL,
	[InventoryOrderId] [int] NULL,
	[Fulfilled] [bit] NOT NULL,
	[ConsumptionId] [int] NULL,
	[RequestLocationId] [int] NULL,
	[IsUrgent] [bit] NOT NULL,
 CONSTRAINT [PK_Inventory_ProductStockRequest] PRIMARY KEY CLUSTERED 
(
	[StockRequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Default [DF_Inventory_Stock_Quantity]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock] ADD  CONSTRAINT [DF_Inventory_Stock_Quantity]  DEFAULT ((1)) FOR [Quantity]
GO
/****** Object:  Default [DF_Inventory_Stock_CreatedOn]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock] ADD  CONSTRAINT [DF_Inventory_Stock_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_Stock_StockStatus]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock] ADD  CONSTRAINT [DF_Inventory_Stock_StockStatus]  DEFAULT ((0)) FOR [StockStatus]
GO
/****** Object:  Default [DF_Inventory_Stock_ReceivedQuantity]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock] ADD  CONSTRAINT [DF_Inventory_Stock_ReceivedQuantity]  DEFAULT ((1)) FOR [ReceivedQuantity]
GO
/****** Object:  Default [DF_Property_PropertyType]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Property] ADD  CONSTRAINT [DF_Property_PropertyType]  DEFAULT ((0)) FOR [PropertyType]
GO
/****** Object:  Default [DF_Inventory_ProductStockRequest_Quantity]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest] ADD  CONSTRAINT [DF_Inventory_ProductStockRequest_Quantity]  DEFAULT ((1)) FOR [Quantity]
GO
/****** Object:  Default [DF_Inventory_ProductStockRequest_CreatedOn]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest] ADD  CONSTRAINT [DF_Inventory_ProductStockRequest_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_ProductStockRequest_RequestStatus]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest] ADD  CONSTRAINT [DF_Inventory_ProductStockRequest_RequestStatus]  DEFAULT ((0)) FOR [RequestStatus]
GO
/****** Object:  Default [DF_Inventory_ProductStockRequest_Fulfilled]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest] ADD  CONSTRAINT [DF_Inventory_ProductStockRequest_Fulfilled]  DEFAULT ((0)) FOR [Fulfilled]
GO
/****** Object:  Default [DF_Inventory_ProductStockRequest_IsUrgent]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest] ADD  CONSTRAINT [DF_Inventory_ProductStockRequest_IsUrgent]  DEFAULT ((0)) FOR [IsUrgent]
GO
/****** Object:  Default [DF_Inventory_Product_UseExpired]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_UseExpired]  DEFAULT ((0)) FOR [UseExpired]
GO
/****** Object:  Default [DF_Inventory_Product_UseSterile]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_UseSterile]  DEFAULT ((0)) FOR [UseSterile]
GO
/****** Object:  Default [DF_Inventory_Product_MaxUses]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_MaxUses]  DEFAULT ((1)) FOR [MaxUses]
GO
/****** Object:  Default [DF_Inventory_Product_RequiresSerialNumber]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_RequiresSerialNumber]  DEFAULT ((0)) FOR [RequiresSerialNumber]
GO
/****** Object:  Default [DF_Inventory_Product_RequiresBatchNumber]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_RequiresBatchNumber]  DEFAULT ((0)) FOR [RequiresBatchNumber]
GO
/****** Object:  Default [DF_Inventory_Product_IsConsignment]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_IsConsignment]  DEFAULT ((0)) FOR [IsConsignment]
GO
/****** Object:  Default [DF_Inventory_Product_Prosthetic]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_Prosthetic]  DEFAULT ((0)) FOR [Prosthetic]
GO
/****** Object:  Default [DF_Inventory_Product_CreatedOn]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_Product_MinimumReorder]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_MinimumReorder]  DEFAULT ((1)) FOR [MinimumOrder]
GO
/****** Object:  Default [DF_Inventory_Product_RecommendedReorder]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_RecommendedReorder]  DEFAULT ((1)) FOR [RecommendedOrder]
GO
/****** Object:  Default [DF_Inventory_Product_OrderMultiple]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_OrderMultiple]  DEFAULT ((1)) FOR [OrderMultiple]
GO
/****** Object:  Default [DF_Inventory_Product_Units]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product] ADD  CONSTRAINT [DF_Inventory_Product_Units]  DEFAULT ((1)) FOR [OrderableUnits]
GO
/****** Object:  Default [DF_Inventory_Order_Item_]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[OrderItem] ADD  CONSTRAINT [DF_Inventory_Order_Item_]  DEFAULT ((0)) FOR [IncludeRebateCode]
GO
/****** Object:  Default [DF_Inventory_Order_Item_InBulk]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[OrderItem] ADD  CONSTRAINT [DF_Inventory_Order_Item_InBulk]  DEFAULT ((0)) FOR [InBulk]
GO
/****** Object:  Default [DF_Inventory_Order_StatusId]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Order] ADD  CONSTRAINT [DF_Inventory_Order_StatusId]  DEFAULT ((1)) FOR [StatusId]
GO
/****** Object:  Default [DF_Inventory_Order_PartialShipping]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Order] ADD  CONSTRAINT [DF_Inventory_Order_PartialShipping]  DEFAULT ((0)) FOR [PartialShipping]
GO
/****** Object:  Default [DF_Inventory_Order_IsUrgent]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Order] ADD  CONSTRAINT [DF_Inventory_Order_IsUrgent]  DEFAULT ((0)) FOR [IsUrgent]
GO
/****** Object:  Default [DF_Inventory_Location_CreatedOn]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Location] ADD  CONSTRAINT [DF_Inventory_Location_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_Location_IsStockLocation]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Location] ADD  CONSTRAINT [DF_Inventory_Location_IsStockLocation]  DEFAULT ((0)) FOR [IsStockLocation]
GO
/****** Object:  Default [DF_Inventory_ExternalProductMapping_CreatedOn]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[ExternalProductMapping] ADD  CONSTRAINT [DF_Inventory_ExternalProductMapping_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_CostCenter_CreatedOn]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[CostCenter] ADD  CONSTRAINT [DF_Inventory_CostCenter_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
/****** Object:  Default [DF_Inventory_Consumption_QuantityConsumed]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Consumption] ADD  CONSTRAINT [DF_Inventory_Consumption_QuantityConsumed]  DEFAULT ((1)) FOR [QuantityConsumed]
GO
/****** Object:  Default [DF_Inventory_Consumption_Status]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Consumption] ADD  CONSTRAINT [DF_Inventory_Consumption_Status]  DEFAULT ((0)) FOR [Status]
GO
/****** Object:  Default [DF_Inventory_Consumption_Source]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Consumption] ADD  CONSTRAINT [DF_Inventory_Consumption_Source]  DEFAULT ((0)) FOR [Source]
GO
/****** Object:  Default [DF_Inventory_Category_Deleted]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Category] ADD  CONSTRAINT [DF_Inventory_Category_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
/****** Object:  Default [DF_Inventory_Category_CreationDate]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Category] ADD  CONSTRAINT [DF_Inventory_Category_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
/****** Object:  ForeignKey [FK_Inventory_StockSetItem_Inventory_Product]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[StockSetItem]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_StockSetItem_Inventory_Product] FOREIGN KEY([ProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[StockSetItem] CHECK CONSTRAINT [FK_Inventory_StockSetItem_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_StockSetItem_Inventory_StockSet]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[StockSetItem]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_StockSetItem_Inventory_StockSet] FOREIGN KEY([StockSetId])
REFERENCES [Inventory].[StockSet] ([StockSetId])
GO
ALTER TABLE [Inventory].[StockSetItem] CHECK CONSTRAINT [FK_Inventory_StockSetItem_Inventory_StockSet]
GO
/****** Object:  ForeignKey [FK_Inventory_Stock_Inventory_Order]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Stock_Inventory_Order] FOREIGN KEY([InventoryOrderId])
REFERENCES [Inventory].[Order] ([InventoryOrderId])
GO
ALTER TABLE [Inventory].[Stock] CHECK CONSTRAINT [FK_Inventory_Stock_Inventory_Order]
GO
/****** Object:  ForeignKey [FK_Inventory_Stock_Inventory_Product]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Stock_Inventory_Product] FOREIGN KEY([ProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[Stock] CHECK CONSTRAINT [FK_Inventory_Stock_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Stock_Location]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Stock]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Stock_Location] FOREIGN KEY([StoredAt])
REFERENCES [Inventory].[Location] ([LocationId])
GO
ALTER TABLE [Inventory].[Stock] CHECK CONSTRAINT [FK_Inventory_Stock_Location]
GO
/****** Object:  ForeignKey [FK_Inventory_ProductStockRequest_Inventory_Consumption]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Consumption] FOREIGN KEY([ConsumptionId])
REFERENCES [Inventory].[Consumption] ([ConsumptionId])
GO
ALTER TABLE [Inventory].[ProductStockRequest] CHECK CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Consumption]
GO
/****** Object:  ForeignKey [FK_Inventory_ProductStockRequest_Inventory_Order]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Order] FOREIGN KEY([InventoryOrderId])
REFERENCES [Inventory].[Order] ([InventoryOrderId])
GO
ALTER TABLE [Inventory].[ProductStockRequest] CHECK CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Order]
GO
/****** Object:  ForeignKey [FK_Inventory_ProductStockRequest_Inventory_Product]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductStockRequest]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Product] FOREIGN KEY([ProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[ProductStockRequest] CHECK CONSTRAINT [FK_Inventory_ProductStockRequest_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Product_Price_Inventory_PriceType]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductPrice]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Product_Price_Inventory_PriceType] FOREIGN KEY([PriceTypeId])
REFERENCES [Inventory].[PriceType] ([PriceTypeId])
GO
ALTER TABLE [Inventory].[ProductPrice] CHECK CONSTRAINT [FK_Inventory_Product_Price_Inventory_PriceType]
GO
/****** Object:  ForeignKey [FK_Inventory_Product_Price_Inventory_Product]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[ProductPrice]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Product_Price_Inventory_Product] FOREIGN KEY([ProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[ProductPrice] CHECK CONSTRAINT [FK_Inventory_Product_Price_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Master_Category_Inventory_Category]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Product_Category]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Master_Category_Inventory_Category] FOREIGN KEY([CategoryId])
REFERENCES [Inventory].[Category] ([CategoryId])
GO
ALTER TABLE [Inventory].[Product_Category] CHECK CONSTRAINT [FK_Inventory_Master_Category_Inventory_Category]
GO
/****** Object:  ForeignKey [FK_Inventory_Master_Category_Inventory_Product]    Script Date: 01/20/2015 14:33:49 ******/
ALTER TABLE [Inventory].[Product_Category]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Master_Category_Inventory_Product] FOREIGN KEY([Inv_ID])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[Product_Category] CHECK CONSTRAINT [FK_Inventory_Master_Category_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Product_Company]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Product_Company] FOREIGN KEY([PrimarySupplier])
REFERENCES [dbo].[Company] ([company_ID])
GO
ALTER TABLE [Inventory].[Product] CHECK CONSTRAINT [FK_Inventory_Product_Company]
GO
/****** Object:  ForeignKey [FK_Inventory_Product_Company1]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Product_Company1] FOREIGN KEY([SecondarySupplier])
REFERENCES [dbo].[Company] ([company_ID])
GO
ALTER TABLE [Inventory].[Product] CHECK CONSTRAINT [FK_Inventory_Product_Company1]
GO
/****** Object:  ForeignKey [FK_Inventory_Order_Item_Inventory_Order]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Order_Item_Inventory_Order] FOREIGN KEY([InventoryOrderId])
REFERENCES [Inventory].[Order] ([InventoryOrderId])
GO
ALTER TABLE [Inventory].[OrderItem] CHECK CONSTRAINT [FK_Inventory_Order_Item_Inventory_Order]
GO
/****** Object:  ForeignKey [FK_Inventory_Order_Item_Inventory_Product]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Order_Item_Inventory_Product] FOREIGN KEY([ProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[OrderItem] CHECK CONSTRAINT [FK_Inventory_Order_Item_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Order_Inventory_CostCenter]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Order_Inventory_CostCenter] FOREIGN KEY([CostCenterId])
REFERENCES [Inventory].[CostCenter] ([CostCenterId])
GO
ALTER TABLE [Inventory].[Order] CHECK CONSTRAINT [FK_Inventory_Order_Inventory_CostCenter]
GO
/****** Object:  ForeignKey [FK_Inventory_Order_Inventory_Location]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Order_Inventory_Location] FOREIGN KEY([DeliveryLocationId])
REFERENCES [Inventory].[Location] ([LocationId])
GO
ALTER TABLE [Inventory].[Order] CHECK CONSTRAINT [FK_Inventory_Order_Inventory_Location]
GO
/****** Object:  ForeignKey [FK_Inventory_Location_Inventory_Address]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Location]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Location_Inventory_Address] FOREIGN KEY([AddressId])
REFERENCES [Inventory].[Address] ([AddressId])
GO
ALTER TABLE [Inventory].[Location] CHECK CONSTRAINT [FK_Inventory_Location_Inventory_Address]
GO
/****** Object:  ForeignKey [FK_Inventory_ExternalProductMapping_Inventory_Product]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[ExternalProductMapping]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_ExternalProductMapping_Inventory_Product] FOREIGN KEY([InventoryProductId])
REFERENCES [Inventory].[Product] ([ProductId])
GO
ALTER TABLE [Inventory].[ExternalProductMapping] CHECK CONSTRAINT [FK_Inventory_ExternalProductMapping_Inventory_Product]
GO
/****** Object:  ForeignKey [FK_Inventory_Consumption_Inventory_Stock]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Consumption]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Consumption_Inventory_Stock] FOREIGN KEY([StockId])
REFERENCES [Inventory].[Stock] ([StockId])
GO
ALTER TABLE [Inventory].[Consumption] CHECK CONSTRAINT [FK_Inventory_Consumption_Inventory_Stock]
GO
/****** Object:  ForeignKey [FK_Inventory_Category_Inventory_Category]    Script Date: 01/20/2015 14:33:50 ******/
ALTER TABLE [Inventory].[Category]  WITH CHECK ADD  CONSTRAINT [FK_Inventory_Category_Inventory_Category] FOREIGN KEY([ParentId])
REFERENCES [Inventory].[Category] ([CategoryId])
GO
ALTER TABLE [Inventory].[Category] CHECK CONSTRAINT [FK_Inventory_Category_Inventory_Category]
GO
