BEGIN TRANSACTION

INSERT INTO [Inventory].[Category]
           ([ParentId]
           ,[CategoryName]
           ,[Deleted]
           ,[DeletedBy]
           ,[DeletedOn]
           ,[LastModifiedDate]
           ,[LastModifiedUser]
           ,[CreationDate]
           ,[UserCreated]
           ,[CategoryType])
     VALUES
           (NULL
           ,'New Product'
           ,0
           ,NULL
           ,NULL
           ,GETDATE()
           ,'Daniel'
           ,GETDATE()
           ,'Daniel'
           ,1)
GO
commit