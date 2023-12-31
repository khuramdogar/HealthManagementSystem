/*
   Tuesday, 14 April 20153:06:21 PM
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
ALTER TABLE Inventory.[MedicareGroup] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Manufacturer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.MedicareProducts
	(
	MedicareProductId int NOT NULL IDENTITY (1, 1),
	Code varchar(5) NOT NULL,
	Name varchar(500) NULL,
	Description varchar(MAX) NULL,
	Size varchar(MAX) NULL,
	MinBenefit money NULL,
	MaxBenefit money NULL,
	Sponsor varchar(2) NULL,
	ProductGroup int NULL,
	ProductSubGroup int NULL,
	Suffix varchar(110) NULL,
	Note varchar(255) NULL,
	Change varchar(255) NULL,
	DeleteComment varchar(255) NULL,
	SupplierId int NULL,
	ManufacturerId int NULL,
	GroupId int NULL,
	SubGroupId int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	PK_MedicareProducts PRIMARY KEY CLUSTERED 
	(
	MedicareProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_Supplier FOREIGN KEY
	(
	SupplierId
	) REFERENCES Inventory.Supplier
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_Manufacturer FOREIGN KEY
	(
	ManufacturerId
	) REFERENCES Inventory.Manufacturer
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_MedicareGroup FOREIGN KEY
	(
	GroupId
	) REFERENCES Inventory.[MedicareGroup]
	(
	MedicareGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_MedicareGroup1 FOREIGN KEY
	(
	SubGroupId
	) REFERENCES Inventory.[MedicareGroup]
	(
	MedicareGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	UQ_MedicareProducts_RebateCode UNIQUE NONCLUSTERED 
	(
	MedicareProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE Inventory.MedicareProducts SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
