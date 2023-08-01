/****** Object:  Table [Inventory].[Manufacturer]    Script Date: 04/14/2015 11:18:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [Inventory].[Manufacturer](
	[company_ID] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](100) NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [varchar](100) NULL,
	[MedicareCode] [varchar](2) NULL
	
 CONSTRAINT [PK_Manufacturer] PRIMARY KEY CLUSTERED 
(
	[company_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [Inventory].[Manufacturer] WITH CHECK ADD  CONSTRAINT [FK_Manufacturer_Company] FOREIGN KEY([company_ID])
REFERENCES [dbo].[Company] ([company_ID])
GO

ALTER TABLE [Inventory].[Manufacturer] CHECK CONSTRAINT [FK_Manufacturer_Company]
GO

ALTER TABLE [Inventory].[Manufacturer] ADD  CONSTRAINT [DF_Manufacturer_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO


