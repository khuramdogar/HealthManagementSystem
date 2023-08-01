INSERT INTO [Inventory].[StockSetting]
           ([SettingId]
           ,[Name]
           ,[Description]
           ,[Enabled])
     VALUES
           ('UNO'
           ,'Unorderable'
           ,'Item cannot be ordered and will not show in lists of items to order.'
           ,1)
GO


DECLARE @newProductId int
SET @newProductId = (SELECT CategoryId FROM Inventory.Category WHERE CategoryName = 'New Product')


INSERT INTO [Inventory].[CategorySetting]
           ([SettingId]
           ,[CategoryId])
     VALUES
           ('UNO'
           ,@newProductId)
GO



