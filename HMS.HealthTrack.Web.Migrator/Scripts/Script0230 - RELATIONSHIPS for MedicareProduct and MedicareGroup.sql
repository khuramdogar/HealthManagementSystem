
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
ALTER TABLE Inventory.MedicareCategory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareProducts
	DROP CONSTRAINT FK_MedicareProducts_MedicareGroup1
GO
ALTER TABLE Inventory.MedicareGroup ADD
	MedicareCategoryId int NULL
GO
ALTER TABLE Inventory.MedicareGroup ADD CONSTRAINT
	FK_MedicareGroup_MedicareCategory FOREIGN KEY
	(
	MedicareCategoryId
	) REFERENCES Inventory.MedicareCategory
	(
	MedicareCategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareProducts SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


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
ALTER TABLE Inventory.MedicareProducts
	DROP CONSTRAINT FK_MedicareProducts_MedicareGroup
GO
ALTER TABLE Inventory.MedicareGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareProducts
	DROP COLUMN GroupId, SubGroupId
GO
ALTER TABLE Inventory.MedicareProducts SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


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
ALTER TABLE Inventory.MedicareSubGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_MedicareGroup FOREIGN KEY
	(
	ProductGroup
	) REFERENCES Inventory.MedicareGroup
	(
	MedicareGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_MedicareSubGroup FOREIGN KEY
	(
	ProductSubGroup
	) REFERENCES Inventory.MedicareSubGroup
	(
	MedicareSubGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

/*
   Monday, April 4, 201611:08:23 AM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
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
ALTER TABLE Inventory.MedicareProductSponsor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.MedicareProducts ADD CONSTRAINT
	FK_MedicareProducts_MedicareProductSponsor FOREIGN KEY
	(
	Sponsor
	) REFERENCES Inventory.MedicareProductSponsor
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareProducts SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
