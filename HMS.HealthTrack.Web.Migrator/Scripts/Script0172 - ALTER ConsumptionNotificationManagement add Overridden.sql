/*
   Tuesday, 1 December 201510:30:35 AM
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
ALTER TABLE Inventory.ConsumptionNotificationManagement ADD
	Overridden bit NOT NULL CONSTRAINT DF_ConsumptionNotificationManagement_Overridden DEFAULT 0,
	OverriddenOn datetime NULL,
	OverriddenBy varchar(50) NULL
GO
ALTER TABLE Inventory.ConsumptionNotificationManagement SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
