IF NOT EXISTS (SELECT 1 FROM Inventory.DashboardNotification WHERE DashboardNotificationId = 'NegativeStock')
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
              ('NegativeStock'
              ,'Negative stock'
              ,'There are products with stock levels that have fallen below zero. Please correct these by receiving new stock or performing a stock take.'
              ,'Shipping Box 1 Error.png'
              ,50
              ,0
              ,0
              ,'Stock')
   GO


