/*
   Tuesday, 27 October 201510:00:15 PM
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
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.Billed', N'Tmp_Reported', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ConsumptionNotificationManagement.Tmp_Reported', N'Reported', 'COLUMN' 
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement ADD
	ReportedOn datetime NULL,
	ReportedBy varchar(50) NULL
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
