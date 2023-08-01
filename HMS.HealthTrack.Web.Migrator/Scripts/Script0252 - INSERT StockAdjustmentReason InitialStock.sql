IF NOT EXISTS(SELECT 1 FROM Inventory.StockAdjustmentReason sar WHERE NAME = 'Initial stock')
INSERT INTO [Inventory].[StockAdjustmentReason]
           ([Name]
           ,[Description]
           ,[Disabled]
           ,[CreatedOn]
           ,[CreatedBy]
           ,[LastModifiedOn]
           ,[LastModifiedUser]
           ,[DeletedOn]
           ,[DeletedBy]
           ,[IsSystemReason])
     VALUES
           ('Initial stock'
           ,'The inital stock for available for the product'
           ,0
           ,GETDATE()
           ,'Daniel'
           ,GETDATE()
           ,'Daniel'
           ,NULL
           ,NULL
           ,1)
GO


