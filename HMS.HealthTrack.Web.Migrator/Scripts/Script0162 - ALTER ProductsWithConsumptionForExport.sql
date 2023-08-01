IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'ProductsWithConsumptionForExport' AND TABLE_SCHEMA = 'Inventory')
	DROP VIEW Inventory.ProductsWithConsumptionForExport
GO

CREATE VIEW Inventory.ProductsWithConsumptionForExport
AS

WITH InventoryUsedCTE (invItemId, qty, mostRecent) 
AS (
SELECT invItem_ID, SUM(invUsed_Qty), MAX(dateCreated)
FROM Inventory_Used
GROUP BY invItem_ID
)

SELECT p.ProductId, c.companyName AS PrimarySupplierName, used.mostRecent AS MostRecentConsumption, used.qty AS ConsumedQuantity
FROM Inventory.Product p 
LEFT JOIN Inventory.Supplier s on p.PrimarySupplier = s.company_ID
LEFT JOIN Company c on s.company_ID = c.company_ID
LEFT JOIN Inventory.ExternalProductMapping epm on p.ProductId = epm.InventoryProductId
LEFT JOIN InventoryUsedCTE used on epm.ExternalProductId = used.invItemId
GROUP BY p.ProductId, c.companyName, used.mostRecent, used.qty