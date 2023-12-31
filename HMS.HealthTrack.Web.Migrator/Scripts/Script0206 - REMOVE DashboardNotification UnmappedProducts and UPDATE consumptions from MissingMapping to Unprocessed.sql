DELETE FROM Inventory.DashboardNotification
WHERE DashboardNotificationId = 'UnmappedProducts'


BEGIN TRANSACTION 
UPDATE Inventory.ConsumptionNotificationManagement
SET ProcessingStatus = 0, ProcessingStatusMessage = ''
WHERE ProcessingStatus = 3

COMMIT


INSERT INTO [Inventory].[DashboardNotification]
           ([DashboardNotificationId]
           ,[Title]
           ,[Description]
           ,[Icon]
           ,[Priority]
           ,[ShowWhenZero]
           ,[Disabled]
           ,[Area])
     VALUES
           ('PendingConsumedProducts'
           ,'Pending consumed products'
           ,'There are products that have been created by the system and have been consumed. Please update them with their correct information.'
           ,'Scale And Box Configuration.png'
           ,50
           ,0
           ,0
           ,'Product issues')
GO


