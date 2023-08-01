UPDATE Inventory.Product
SET InError = 1
WHERE (SPC IN (
		SELECT SPC 
		FROM Inventory.Product
		WHERE DeletedOn IS NULL
			AND SPC IS NOT NULL AND SPC != ''
		GROUP BY SPC 
		HAVING COUNT(*) > 1) OR 
	UPN IN (
		SELECT UPN
		FROM Inventory.Product
		WHERE DeletedOn IS NOT NULL
			AND UPN IS NOT NULL AND UPN != ''
		GROUP BY UPN 
		HAVING COUNT(*) > 1))
	AND DeletedOn IS NULL 


UPDATE Inventory.Product
SET InError = 1
WHERE DeletedOn IS NULL AND TargetStockLevel IS NOT NULL AND (ReorderThreshold - TargetStockLevel > 0)

UPDATE Inventory.Product
SET InError = 1
WHERE DeletedOn IS NULL AND (UPN IS NULL OR UPN = '') AND (SPC IS NULL OR SPC = '')