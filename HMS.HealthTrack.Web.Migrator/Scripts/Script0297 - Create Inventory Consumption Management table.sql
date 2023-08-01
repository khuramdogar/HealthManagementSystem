CREATE TABLE [Inventory].[ConsumptionManagement](
	[ConsumptionId] [bigint] NOT NULL,
	[Invoiced] [bit] NOT NULL,
	[OrderItemId] [int] NULL,
	[Reported] [bit] NOT NULL,
	[ReportedOn] [datetime] NULL,
	[ReportedBy] [varchar](50) NULL,
	[ProcessingStatus] [int] NOT NULL,
	[ProcessingStatusMessage] [varchar](500) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [varchar](50) NULL,
	[ArchivedBy] [varchar](50) NULL,
	[ArchivedOn] [datetime] NULL,
 CONSTRAINT [PK_ConsumptionManagement] PRIMARY KEY CLUSTERED 
(
	[ConsumptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Inventory].[ConsumptionManagement] ADD  CONSTRAINT [DF_ConsumptionManagement_Invoiced]  DEFAULT ((0)) FOR [Invoiced]
GO

ALTER TABLE [Inventory].[ConsumptionManagement] ADD  CONSTRAINT [DF_ConsumptionManagement_Billed]  DEFAULT ((0)) FOR [Reported]
GO

ALTER TABLE [Inventory].[ConsumptionManagement] ADD  CONSTRAINT [DF_ConsumptionManagement_ProcessingStatus]  DEFAULT ((0)) FOR [ProcessingStatus]
GO

ALTER TABLE [Inventory].[ConsumptionManagement]  WITH CHECK ADD  CONSTRAINT [FK_ConsumptionManagement_Consumption] FOREIGN KEY([ConsumptionId])
REFERENCES [Inventory].[Consumption] ([ConsumptionId])
GO

ALTER TABLE [Inventory].[ConsumptionManagement] CHECK CONSTRAINT [FK_ConsumptionManagement_Consumption]
GO

ALTER TABLE [Inventory].[ConsumptionManagement]  WITH CHECK ADD  CONSTRAINT [FK_ConsumptionManagement_Order] FOREIGN KEY([OrderItemId])
REFERENCES [Inventory].[OrderItem] ([OrderItemId])
GO

ALTER TABLE [Inventory].[ConsumptionManagement] CHECK CONSTRAINT [FK_ConsumptionManagement_Order]
GO