CREATE VIEW Inventory.ExternalMappedProducts AS
	SELECT     im.Inv_ID AS ExternalProductId, epm.InventoryProductId, p.ReportConsumption
	FROM         dbo.Inventory_Master AS im
	INNER JOIN Inventory.ExternalProductMapping epm
		ON im.Inv_ID = epm.ExternalProductId
	INNER JOIN Inventory.Product p on epm.InventoryProductId = p.ProductId