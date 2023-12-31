/*
   Wednesday, 20 June 20184:14:53 PM
   User: sa
   Server: LANNISTER\SQLSERVER2016
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
CREATE TABLE Inventory.Consumption
	(
	ConsumptionId bigint NOT NULL IDENTITY (1, 1),
	ConsumptionReference int NOT NULL,
	ProductId int NULL,
	SPC nvarchar(50) NULL,
	UPC nvarchar(150) NULL,
	ProductName nvarchar(500) NULL,
	Quantity int NOT NULL,
	ConsumedOn datetime NOT NULL,
	SerialNumber varchar(200) NULL,
	BatchNumber varchar(200) NULL,
	LocationId int NOT NULL,
	LocationName varchar(200) NULL,
	Consumer varchar(200) NULL,
	ApplicationId varchar(200) NOT NULL,
	Description varchar(1000) NULL,
	RebateCode varchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Consumption ADD CONSTRAINT
	PK_Consumption_ConsumptionId PRIMARY KEY CLUSTERED 
	(
	ConsumptionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_Consumption_ConsumptionReference ON Inventory.Consumption
	(
	ConsumptionReference
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Consumption_SPC ON Inventory.Consumption
	(
	SPC
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Consumption_ProductId ON Inventory.Consumption
	(
	ProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.Consumption SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
