/*
   Friday, 9 December 20164:28:38 PM
   User: sa
   Server: bolton
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
ALTER TABLE Inventory.Submission ADD
	AdditionalDetails varchar(2000) NULL
GO
ALTER TABLE Inventory.Submission SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
