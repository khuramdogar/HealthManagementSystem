/*
   Thursday, 3 December 201511:47:39 AM
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
CREATE TABLE Inventory.StockDeductionReason
   (
   StockDeductionReasonId int NOT NULL IDENTITY (1, 1),
   Name varchar(50) NOT NULL,
   Description varchar(400) NULL,
   Deleted bit NOT NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.StockDeductionReason ADD CONSTRAINT
   PK_StockDeductionReason PRIMARY KEY CLUSTERED 
   (
   StockDeductionReasonId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
