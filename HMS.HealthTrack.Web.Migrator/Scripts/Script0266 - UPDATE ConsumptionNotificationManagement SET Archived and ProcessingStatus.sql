BEGIN TRAN
UPDATE Inventory.ConsumptionNotificationManagement 
SET Inventory.ConsumptionNotificationManagement.ArchivedBy = Inventory.ConsumptionNotificationManagement.LastModifiedBy, Inventory.ConsumptionNotificationManagement.ArchivedOn = Inventory.ConsumptionNotificationManagement.LastModifiedOn,
Inventory.ConsumptionNotificationManagement.ProcessingStatus = 1,
Inventory.ConsumptionNotificationManagement.ProcessingStatusMessage = 'Processed: ' 
	+ CONVERT(varchar   (100),Inventory.ConsumptionNotificationManagement.LastModifiedOn, 113)
WHERE Inventory.ConsumptionNotificationManagement.ProcessingStatus = 4

COMMIT


