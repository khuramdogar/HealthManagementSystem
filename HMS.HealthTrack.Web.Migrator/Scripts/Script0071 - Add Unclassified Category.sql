UPDATE Inventory.Category
SET Deleted = 0
WHERE CategoryName = 'New Product'

UPDATE Inventory.Category
SET LastModifiedUser = 'System'
WHERE CategoryName = 'New Product'

UPDATE Inventory.Category
SET LastModifiedDate = GETDATE()
WHERE CategoryName = 'New Product'

UPDATE Inventory.Category
SET CategoryName = 'Unclassified'
WHERE CategoryName = 'New Product'

GO


IF NOT EXISTS (SELECT 1 FROM Inventory.Category WHERE CategoryName = 'Unclassified')
	INSERT INTO [Inventory].[Category]
			   ([ParentId]
			   ,[CategoryName]
			   ,[LastModifiedDate]
			   ,[LastModifiedUser]
			   ,[CreationDate]
			   ,[UserCreated]
			   ,[CategoryType]
			   )
		 VALUES
			   (NULL
			   ,'Unclassified'
			   ,GETDATE()
			   ,'Daniel'
			   ,GETDATE()
			   ,'Daniel'
			   ,1)
	GO