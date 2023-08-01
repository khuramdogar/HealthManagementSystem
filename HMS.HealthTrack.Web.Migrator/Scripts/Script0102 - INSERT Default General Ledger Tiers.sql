INSERT INTO [Inventory].[GeneralLedgerTier]
           ([Tier]
           ,[Name]
           ,[TierType]
           ,[SettingId]
           ,[Mandatory]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[LastModifiedBy]
           ,[LastModifiedOn]
           ,[DeletedBy]
           ,[DeletedOn])
     VALUES
           (1
           ,'Entity'
           ,0
           ,1
           ,1
           ,'System'
           ,GETDATE()
           ,'System'
           ,GETDATE()
           ,NULL
           ,NULL)
GO


INSERT INTO [Inventory].[GeneralLedgerTier]
           ([Tier]
           ,[Name]
           ,[TierType]
           ,[SettingId]
           ,[Mandatory]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[LastModifiedBy]
           ,[LastModifiedOn]
           ,[DeletedBy]
           ,[DeletedOn])
     VALUES
           (2
           ,'Cost Center'
           ,0
           ,1
           ,1
           ,'System'
           ,GETDATE()
           ,'System'
           ,GETDATE()
           ,NULL
           ,NULL)
GO


INSERT INTO [Inventory].[GeneralLedgerTier]
           ([Tier]
           ,[Name]
           ,[TierType]
           ,[SettingId]
           ,[Mandatory]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[LastModifiedBy]
           ,[LastModifiedOn]
           ,[DeletedBy]
           ,[DeletedOn])
     VALUES
           (3
           ,'Account'
           ,1
           ,2
           ,1
           ,'System'
           ,GETDATE()
           ,'System'
           ,GETDATE()
           ,NULL
           ,NULL)
GO

INSERT INTO [Inventory].[GeneralLedgerTier]
           ([Tier]
           ,[Name]
           ,[TierType]
           ,[SettingId]
           ,[Mandatory]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[LastModifiedBy]
           ,[LastModifiedOn]
           ,[DeletedBy]
           ,[DeletedOn])
     VALUES
           (4
           ,'Sub Account'
           ,1
           ,2
           ,0
           ,'System'
           ,GETDATE()
           ,'System'
           ,GETDATE()
           ,NULL
           ,NULL)
GO

INSERT INTO [Inventory].[GeneralLedgerTier]
           ([Tier]
           ,[Name]
           ,[TierType]
           ,[SettingId]
           ,[Mandatory]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[LastModifiedBy]
           ,[LastModifiedOn]
           ,[DeletedBy]
           ,[DeletedOn])
     VALUES
           (5
           ,'Product'
           ,1
           ,2
           ,0
           ,'System'
           ,GETDATE()
           ,'System'
           ,GETDATE()
           ,NULL
           ,NULL)
GO

