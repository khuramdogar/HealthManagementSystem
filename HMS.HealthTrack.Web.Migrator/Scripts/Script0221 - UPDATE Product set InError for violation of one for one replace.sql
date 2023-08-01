UPDATE Inventory.Product
SET InError = 1
WHERE AutoReorderSetting = 2 AND (TargetStockLevel - ReorderThreshold != 1)