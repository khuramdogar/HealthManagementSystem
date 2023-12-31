INSERT INTO [Inventory].[StockSetting]
           ([SettingId]
           ,[Name]
           ,[Description]
           ,[Enabled])
     VALUES
           ('RPD'
           ,'Requires patient details'
           ,'Identifies stock deductions that require patient details'
           ,1)
GO


UPDATE Inventory.ProductSetting
SET SettingId = 'RPD'
WHERE SettingId = 'RCD'
GO

UPDATE Inventory.CategorySetting
SET SettingId = 'RPD'
WHERE SettingId = 'RCD'

GO
DELETE FROM [Inventory].[StockSetting]
WHERE SettingId = 'RCD' AND Name = 'Requires consumption details'