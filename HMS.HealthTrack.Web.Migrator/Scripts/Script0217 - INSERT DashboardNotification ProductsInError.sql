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
           ('ProductsInError'
           ,'Products in error'
           ,'There are products that are currently in a state of error. Please edit them to correct their invalid data.'
           ,'Scale And Box Error.png'
           ,20
           ,0
           ,0
           ,'Product issues')
GO


