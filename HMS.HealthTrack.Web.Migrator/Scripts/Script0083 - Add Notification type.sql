/*
   Tuesday, 21 July 20154:12:17 PM
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
ALTER TABLE Inventory.DashboardNotification ADD
	Disabled bit NOT NULL CONSTRAINT DF_DashboardNotification_Enabled DEFAULT 0,
	Area varchar(100) NULL
GO
ALTER TABLE Inventory.DashboardNotification SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'Inventory.DashboardNotification', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'Inventory.DashboardNotification', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'Inventory.DashboardNotification', 'Object', 'CONTROL') as Contr_Per 

update Inventory.DashboardNotification set Area = 'Automatic ordering' where [DashboardNotificationId] = 'MissingPaymentClass'
update Inventory.DashboardNotification set Area = 'Automatic ordering' where [DashboardNotificationId] = 'OrderItems'
update Inventory.DashboardNotification set Area = 'Automatic ordering' where [DashboardNotificationId] = 'ProductsWithoutStockControl'
update Inventory.DashboardNotification set Area = 'Automatic ordering' where [DashboardNotificationId] = 'UnclassifiedProducts'
update Inventory.DashboardNotification set Area = 'Consumption processing' where [DashboardNotificationId] = 'ConsumptionNotificationProcessingErrors'
update Inventory.DashboardNotification set Area = 'Consumption processing' where [DashboardNotificationId] = 'NonStockTakeConsumptions'
update Inventory.DashboardNotification set Area = 'Consumption processing' where [DashboardNotificationId] = 'UncontrolledConsumptions'
update Inventory.DashboardNotification set Area = 'Consumption processing' where [DashboardNotificationId] = 'UnmappedProducts'

