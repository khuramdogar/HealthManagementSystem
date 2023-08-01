BEGIN TRANSACTION

UPDATE Inventory.ConsumptionNotificationManagement
SET LastModifiedBy = iu.userLastModified, LastModifiedOn = iu.dateLastModified
FROM Inventory.ConsumptionNotificationManagement cnm
INNER JOIN Inventory_Used iu on cnm.invUsed_ID = iu.invUsed_ID
WHERE cnm.LastModifiedBy IS NULL

COMMIT
