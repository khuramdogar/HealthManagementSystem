UPDATE Inventory.DashboardNotification
SET Area = 'Product issues'
WHERE DashboardNotificationId IN ('NonStockTakeConsumptions', 'UnclassifiedProducts', 'ProductsWithoutStockControl') 

UPDATE Inventory.DashboardNotification
SET Title = 'Products requiring stock management'
WHERE DashboardNotificationId = 'ProductsWithoutStockControl'

UPDATE Inventory.DashboardNotification
SET Title = 'Products requiring a stock take'
WHERE DashboardNotificationId = 'NonStockTakeConsumptions'

UPDATE Inventory.DashboardNotification
SET DashboardNotificationId = 'MissingStockTakes', 
   Title = 'Missing stock takes',
   [Description] = 'There are products which are yet to have a stock take. Please perform a stock take if you wish them to be re-stocked.'
WHERE DashboardNotificationId = 'NonStockTakeConsumptions'