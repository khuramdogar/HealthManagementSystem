/*
   Wednesday, 18 March 201511:55:48 AM
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
CREATE TABLE Inventory.StockSetting
   (
   SettingId varchar(20) NOT NULL,
   Name varchar(50) NULL,
   Description varchar(250) NULL,
   Enabled bit NOT NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockSetting ADD CONSTRAINT
   DF_StockSetting_Enabled DEFAULT 0 FOR Enabled
GO
ALTER TABLE Inventory.StockSetting ADD CONSTRAINT
   PK_StockSetting PRIMARY KEY CLUSTERED 
   (
   SettingId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.CategorySetting
   (
   SettingId varchar(20) NOT NULL,
   CategoryId int NOT NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.CategorySetting ADD CONSTRAINT
   PK_CategorySetting PRIMARY KEY CLUSTERED 
   (
   SettingId,
   CategoryId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.CategorySetting ADD CONSTRAINT
   FK_CategorySetting_StockSetting FOREIGN KEY
   (
   SettingId
   ) REFERENCES Inventory.StockSetting
   (
   SettingId
   ) ON UPDATE  NO ACTION 
    ON DELETE  NO ACTION 
   
GO
ALTER TABLE Inventory.CategorySetting ADD CONSTRAINT
   FK_CategorySetting_Category FOREIGN KEY
   (
   CategoryId
   ) REFERENCES Inventory.Category
   (
   CategoryId
   ) ON UPDATE  NO ACTION 
    ON DELETE  NO ACTION 
   
GO
ALTER TABLE Inventory.CategorySetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.ProductSetting
   (
   SettingId varchar(20) NOT NULL,
   ProductId int NOT NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.ProductSetting ADD CONSTRAINT
   PK_ProductSetting PRIMARY KEY CLUSTERED 
   (
   SettingId,
   ProductId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.ProductSetting ADD CONSTRAINT
   FK_ProductSetting_StockSetting FOREIGN KEY
   (
   SettingId
   ) REFERENCES Inventory.StockSetting
   (
   SettingId
   ) ON UPDATE  NO ACTION 
    ON DELETE  NO ACTION 
   
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


Insert into Inventory.StockSetting (SettingId,Name,Description,Enabled)
values ('RSN','Requires serial number','Item cannot be consumed without a serial number being entered',1),
       ('RBN','Requires batch number','Item cannot be consumed without a batch number being entered',1),
       ('RPC','Requires payment class','Item cannot be ordered from consumption without a payment class for the booking',1),
       ('RCD','Requires consumption details','Items will show patient/consumption details on orders',1)