UPDATE Inventory.DashboardNotification
SET Description = 'Items are being consumed that do not have their stock handling levels set. These items will not be available for automatic re-ordering'
WHERE DashboardNotificationId = 'UncontrolledConsumptions'