BEGIN TRANSACTION

UPDATE Inventory.DashboardNotification
SET Inventory.DashboardNotification.Description = 'There are products that have been created by the system from consumptions. Please update them with their correct details and stock handling information so they can be managed correctly.'
WHERE Inventory.DashboardNotification.DashboardNotificationId = 'PendingConsumedProducts'

DELETE FROM Inventory.DashboardNotification
WHERE Inventory.DashboardNotification.DashboardNotificationId IN ('UncontrolledConsumptions', 'ProductsWithoutStockControl')

SELECT *
FROM Inventory.DashboardNotification dn

COMMIT