/*
   Thursday, January 14, 201610:36:57 AM
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
ALTER TABLE Inventory.PriceType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.PaymentClassMapping
	(
	PaymentClass varchar(200) NOT NULL,
	PriceTypeId int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.PaymentClassMapping ADD CONSTRAINT
	PK_PaymentClassMapping PRIMARY KEY CLUSTERED 
	(
	PaymentClass
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.PaymentClassMapping ADD CONSTRAINT
	FK_PaymentClassMapping_PriceType FOREIGN KEY
	(
	PriceTypeId
	) REFERENCES Inventory.PriceType
	(
	PriceTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.PaymentClassMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


/*
   Thursday, January 14, 201610:48:31 AM
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
ALTER TABLE Inventory.PaymentClassMapping ADD
	CreatedBy varchar(50) NULL,
	CreatedOn datetime NOT NULL CONSTRAINT DF_PaymentClassMapping_CreatedOn DEFAULT GETDATE(),
	ModifiedBy varchar(50) NULL,
	ModifiedOn datetime NULL,
	DeletedBy varchar(50) NULL,
	DeletedOn datetime NULL
GO
ALTER TABLE Inventory.PaymentClassMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
