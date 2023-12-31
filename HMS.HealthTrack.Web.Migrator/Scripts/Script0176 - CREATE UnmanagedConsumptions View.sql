CREATE VIEW Inventory.UnmanagedConsumptions
AS
SELECT [invUsed_ID]
      ,[container_ID]
      ,[cversion]
      ,[patient_ID]
      ,[invItem_ID]
      ,[invUsed_SPC]
      ,[invUsed_LPC]
      ,[invUsed_SellPrice]
      ,[invUsed_GL]
      ,[invUsed_SerialNo]
      ,[invUsed_Qty]
      ,[invUsed_Units]
      ,[invUsed_Location]
      ,[deleted]
      ,[deletionDate]
      ,[deletionUser]
      ,[dateLastModified]
      ,[userLastModified]
      ,[dateCreated]
      ,[userCreated]
      ,[admissionStage_ID]
      ,[invDescription]
      ,[invNotUsed]
      ,[StockStatus]
      ,[StatusMessage]
      ,[LOTNO]
      ,[ExtStudy_ID]
  FROM [dbo].[Inventory_Used]
  WHERE deleted = 0 AND NOT EXISTS (
	SELECT 1
	FROM Inventory.ConsumptionNotificationManagement
	WHERE Inventory_Used.invUsed_ID = ConsumptionNotificationManagement.invUsed_ID
	)
