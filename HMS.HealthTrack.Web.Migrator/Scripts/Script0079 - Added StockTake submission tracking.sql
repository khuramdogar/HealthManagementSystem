/*
   Monday, 13 July 201511:46:50 AM
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
ALTER TABLE Inventory.StockTake ADD
	SubmittedBy varchar(50) NULL,
	SubmittedOn DateTime NULL
GO
ALTER TABLE Inventory.StockTake SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
