UPDATE Inventory.Product
SET ReorderThreshold = TargetStockLevel - 1
WHERE ReorderSetting = 2 AND
	ReorderThreshold IS NOT NULL AND 
	TargetStockLevel IS NOT NULL