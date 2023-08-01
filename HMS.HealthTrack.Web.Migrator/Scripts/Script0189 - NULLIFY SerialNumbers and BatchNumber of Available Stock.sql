BEGIN TRANSACTION

UPDATE Inventory.Stock
SET SerialNumber = NULL, BatchNumber = NULL
FROM Inventory.Stock
WHERE StockStatus = 0 AND (
SerialNumber IS NOT NULL OR BatchNumber IS NOT NULL)
	
COMMIT