
IF (NOT EXISTS(SELECT * FROM [Inventory].[Property] WHERE [PropertyName] = 'StockTakesEnabled')) 
BEGIN 
   INSERT INTO [Inventory].[Property]
           ([PropertyName]
           ,[PropertyValue]
           ,[Description]
           ,[PropertyType])
     VALUES
           ('StockTakesEnabled'
           ,'0'
           ,'Allow users to perform stock takes'
           ,0)
END 
ELSE 
BEGIN 
    UPDATE [Inventory].[Property]  
    SET [PropertyValue] = '0'
    WHERE [PropertyName] = 'StockTakesEnabled'
END 


GO