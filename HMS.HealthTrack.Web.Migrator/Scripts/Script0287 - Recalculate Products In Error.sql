UPDATE Inventory.Product
Set InError = 0 
WHERE DeletedOn IS NULL

/* 
** Validate Product Script 
**
** Reflects the logic in ValidateProduct method of ProductRepository
** as at 2017-06-27.
*/

BEGIN TRANSACTION

/* Calculate Unique SPC */
UPDATE Inventory.Product
SET InError = 1 
WHERE SPC IN (
	SELECT SPC
	FROM inventory.Product
	WHERE DeletedOn IS NULL
	GROUP BY SPC
	HAVING COUNT(*) > 1
)

/* Calculate Unique Scan Code */
UPDATE Inventory.Product
SET InError = 1
FROM Inventory.Product p
INNER JOIN Inventory.ScanCode sc on p.ProductId = sc.ProductId
WHERE Value IN 
(SELECT Value
FROM Inventory.Product p 
INNER JOIN Inventory.ScanCode sc on p.ProductId = sc.ProductId
WHERE p.DeletedOn IS NULL AND sc.Value NOT IN (
	SELECT Value
	FROM Inventory.Product p 
	INNER JOIN Inventory.ScanCode sc on p.ProductId = sc.ProductId
	WHERE p.DeletedOn IS NULL
	GROUP BY Value, p.ProductId
	HAVING COUNT(*) > 1	
)
GROUP BY Value
HAVING COUNT(*) > 1
)

/* Calculate Valid Stock Handling */

-- Manage Stock and One for One Replace
UPDATE Inventory.Product
SET InError = 1
from inventory.Product
where DeletedOn is null 
	and ManageStock = 1 and product.AutoReorderSetting = 2 -- one for one replace

-- Manage Stock and Specify Levels and ReorderThreshold above Target
UPDATE Inventory.Product
SET InError = 1
from inventory.Product
where DeletedOn is null 
	and ManageStock = 1 and product.AutoReorderSetting = 0  -- specify levels
	AND TargetStockLevel < ReorderThreshold

-- Unmanaged Stock and Specify Levels setting
UPDATE Inventory.Product
SET InError = 1
from inventory.Product
where DeletedOn is null 
	and ManageStock = 0 and product.AutoReorderSetting = 0  -- specify levels and not manage stock
	

/* Calculate missing SPC and ScanCode */
UPDATE Inventory.Product
SET InError = 1
FROM Inventory.Product p
LEFT JOIN Inventory.ScanCode sc on p.ProductId = sc.ProductId
WHERE DeletedOn IS NULL AND 
		(
			(p.SPC IS NULL OR p.SPC = '') AND -- No SPC
			(sc.ProductId IS NULL) -- No Scan Codes
		)

COMMIT