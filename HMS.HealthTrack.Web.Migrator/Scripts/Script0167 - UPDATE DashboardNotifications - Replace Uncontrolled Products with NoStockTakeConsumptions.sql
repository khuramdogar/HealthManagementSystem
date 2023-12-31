  update [Inventory].[DashboardNotification] set Disabled = 1 where DashboardNotificationId = 'ProductsWithoutStockControl'
  begin
  IF NOT EXISTS ( SELECT * FROM [Inventory].[DashboardNotification] 
                   WHERE DashboardNotificationId = 'ConsumptionsMissingStockTakes')
               Begin
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
           ('ConsumptionsMissingStockTakes'
           ,'Consumptions missing stock take'
           ,'Products that have not yet had a stock take have been consumed. Please perform a stock take for the consumed products'
           ,'Barcode Stop.png'
           ,'25'
           ,0
           ,0
           ,'Consumption processing');
end
end