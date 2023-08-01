
UPDATE Inventory.Product
SET Description = im.inv_Description
FROM Inventory.Product p
INNER JOIN Inventory.ExternalProductMapping epm on p.ProductId = epm.InventoryProductId
INNER JOIN Inventory_Master im on epm.ExternalProductId = im.Inv_ID
WHERE p.[Description] is null