/*
   Tuesday, 21 July 201511:27:56 AM
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
CREATE TABLE Inventory.DashboardNotification
   (
   DashboardNotificationId nvarchar(50) NOT NULL,
   Title nvarchar(100) NULL,
   Description nvarchar(350) NULL,
   Icon nvarchar(200) NULL,
   Priority int NOT NULL,
   ShowWhenZero bit NOT NULL
   )  ON [PRIMARY]
GO
ALTER TABLE Inventory.DashboardNotification ADD CONSTRAINT
   DF_DashboardNotification_ShowWhenZero DEFAULT 0 FOR ShowWhenZero
GO
ALTER TABLE Inventory.DashboardNotification ADD CONSTRAINT
   PK_DashboardNotification PRIMARY KEY CLUSTERED 
   (
   DashboardNotificationId
   ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.DashboardNotification SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


Begin Transaction

Insert into Inventory.DashboardNotification (DashboardNotificationId,Title,Description,Icon,Priority,ShowWhenZero)
values ('UncontrolledConsumptions','Consumptions not under stock control','Items are being consumed without an initial stock take being done. These items will not be available for automatic re-ordering','Scale And Box Warning.png',50,0),
 ('OrderItems','Order items for approval','Items are ready to be ordered, please view and approve them','Delivery Truck 1 Check.png',10,1),
 ('NonStockTakeConsumptions','Consumptions without a stock take','Products that have not yet had a stock take are being consumed. Perform a stock take if you wish them to be re-stocked','Scale And Box Stop.png',40,0),
 ('ConsumptionNotificationProcessingErrors','Consumption errors','Errors were found when attempting to process items that were consumed.','Barcode Error.png',20,0),
 ('UnmappedProducts','Unidentified products','Unidentified products have been consumed and require attention. Please save them as new products or map them to existing products','Barcode Help.png',30,0),
 ('UnclassifiedProducts','Unclassified products','There are products in the unclassified category. Please set their primary category to control their behaviour in the system','Scale And Box Help.png',100,0),
 ('ProductsWithoutStockControl','Products not under stock control','There are products that are missing target stock levels or re-order thresholds. These must be set for the product to be under stock control','Scale And Box Progress.png',90,0),
 ('MissingPaymentClass','Missing payment class','Items have been consumed for appoinments that are missing a payment class and will not be re-ordered. Please add a payment class to the appointment','Barcode Warning.png',70,0)
commit