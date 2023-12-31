/*
   Wednesday, 25 March 201510:36:46 AM
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
ALTER TABLE Inventory.CostCenter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.ChargeAccount
	(
	AccountId int NOT NULL IDENTITY (1, 1),
	AccountName varchar(50) NULL,
	EntityCode varchar(10) NULL,
	CostCenterId int NULL,
	AccountCode varchar(10) NULL,
	ParentId int NULL,
   CreatedBy varchar(50) NOT NULL,
   CreatedOn datetime NOT NULL CONSTRAINT DF_ChargeAccount_CreatedOn DEFAULT (GETDATE()), 
   LastModifiedBy varchar(50) NULL,
   LastModifiedOn datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.ChargeAccount ADD CONSTRAINT
	PK_ChargeAccount PRIMARY KEY CLUSTERED 
	(
	AccountId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE Inventory.ChargeAccount ADD CONSTRAINT
	FK_ChargeAccount_CostCenter FOREIGN KEY
	(
	CostCenterId
	) REFERENCES Inventory.CostCenter
	(
	CostCenterId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE Inventory.ChargeAccount ADD CONSTRAINT
	FK_ChargeAccount_ChargeAccount FOREIGN KEY
	(
	ParentId
	) REFERENCES Inventory.ChargeAccount
	(
	AccountId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
GO
ALTER TABLE Inventory.ChargeAccount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
