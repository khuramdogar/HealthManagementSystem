IF NOT EXISTS (SELECT 1 FROM Inventory.DashboardNotification WHERE DashboardNotificationId = 'UnmappedPaymentClasses')
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
			   ('UnmappedPaymentClasses'
			   ,'Unmapped payment classes'
			   ,'There are payment classes which are not linked to a price category. Please create the appropriate mappings.'
			   ,'Savings Account.png'
			   ,50
			   ,0
			   ,0
			   ,'Admin')
	GO

