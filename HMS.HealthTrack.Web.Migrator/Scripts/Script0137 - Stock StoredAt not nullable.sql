BEGIN TRANSACTION

UPDATE Inventory.Stock
SET StoredAt = (
					SELECT PropertyValue
					FROM Inventory.Property
					WHERE PropertyName = 'DefaultStockLocationId')
WHERE StoredAt IS NULL
COMMIT


BEGIN TRANSACTION

ALTER TABLE Inventory.Stock
ALTER COLUMN StoredAt int NOT NULL

COMMIT
