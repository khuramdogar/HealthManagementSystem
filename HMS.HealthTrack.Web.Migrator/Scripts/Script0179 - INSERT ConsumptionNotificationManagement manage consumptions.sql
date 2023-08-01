-- update script to create ConsumptionNotificationManagement items for each Consumption that has gone through the processor

INSERT INTO [Inventory].[ConsumptionNotificationManagement]
           ([invUsed_ID],
		   [ProcessingStatus],
		   [ProcessingStatusMessage]		   )
     SELECT
           invUsed_ID,
		   StockStatus,
		   StatusMessage
		   FROM [Inventory].[UnmanagedConsumptions]
GO


