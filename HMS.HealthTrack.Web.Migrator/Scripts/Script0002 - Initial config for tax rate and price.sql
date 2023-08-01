
IF (NOT EXISTS(SELECT * FROM [Inventory].[Property] WHERE [PropertyName] = 'TaxRate')) 
BEGIN 
   INSERT INTO [Inventory].[Property]
           ([PropertyName]
           ,[PropertyValue]
           ,[Description]
           ,[PropertyType])
     VALUES
           ('TaxRate'
           ,'10'
           ,'Current rate of GST'
           ,0)
END 
ELSE 
BEGIN 
    UPDATE [Inventory].[Property]  
    SET [PropertyValue] = '10'
    WHERE [PropertyName] = 'TaxRate'
END 


GO

IF NOT EXISTS (SELECT * FROM Inventory.PriceType WHERE Name = 'Public')
INSERT INTO [Inventory].[PriceType]
           ([Name])
     VALUES
           ('Public')
GO

IF NOT EXISTS (SELECT * FROM Inventory.PriceType WHERE Name = 'Private')
INSERT INTO [Inventory].[PriceType]
           ([Name])
     VALUES
           ('Private')
GO


