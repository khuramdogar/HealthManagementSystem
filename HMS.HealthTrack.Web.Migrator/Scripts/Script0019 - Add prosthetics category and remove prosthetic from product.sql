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
           ,'Prosthetics'
           ,0
           ,NULL
           ,NULL
           ,GETDATE()
           ,'Daniel'
           ,GETDATE()
           ,'Daniel'
           ,1)
GO

DECLARE @ProstheticsId int
SET @ProstheticsId = (SELECT CategoryId FROM Inventory.Category WHERE CategoryName = 'Prosthetics' AND CategoryType = 1)

INSERT INTO [Inventory].[Product_Category]
           ([Inv_ID]
           ,[CategoryId])
SELECT ProductId, @ProstheticsId
FROM Inventory.Product
WHERE Prosthetic = 1
           
GO
ALTER TABLE Inventory.Product
	DROP CONSTRAINT DF_Inventory_Product_Prosthetic
GO
ALTER TABLE Inventory.Product
	DROP COLUMN Prosthetic
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO