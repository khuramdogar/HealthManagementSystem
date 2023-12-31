/*
   Thursday, May 26, 201612:35:49 PM
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
ALTER TABLE Inventory.ConsumptionNotificationManagement SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.OrderItemSource ADD
	invUsed_ID int NULL
GO
ALTER TABLE Inventory.OrderItemSource ADD CONSTRAINT
	FK_OrderItemSource_ConsumptionNotificationManagement FOREIGN KEY
	(
	invUsed_ID
	) REFERENCES Inventory.ConsumptionNotificationManagement
	(
	invUsed_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderItemSource SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
