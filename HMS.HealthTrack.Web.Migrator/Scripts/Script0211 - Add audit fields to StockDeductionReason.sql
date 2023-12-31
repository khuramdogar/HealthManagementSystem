/*
   Friday, 29 January 20163:01:45 PM
   User: 
   Server: DEV-NEIL\SQL2012
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
ALTER TABLE Inventory.StockDeductionReason ADD
	CreatedOn datetime NOT NULL CONSTRAINT DF_StockDeductionReason_CreatedOn DEFAULT (getdate()),
	CreatedBy varchar(50) NULL,
	LastModifiedOn datetime NULL,
	LastModifiedUser varchar(50) NULL,
	DeletedOn datetime NULL,
	DeletedBy varchar(50) NULL
GO

update Inventory.StockDeductionReason set deletedon = GetDate() where deleted = 1

ALTER TABLE Inventory.StockDeductionReason 
   drop column deleted
GO

ALTER TABLE Inventory.StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
