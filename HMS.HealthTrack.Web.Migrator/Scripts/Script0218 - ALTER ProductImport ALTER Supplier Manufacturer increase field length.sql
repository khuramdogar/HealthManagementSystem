BEGIN TRANSACTION

ALTER TABLE Inventory.ProductImport
ALTER COLUMN Manufacturer VARCHAR(100) NULL


ALTER TABLE Inventory.ProductImport
ALTER COLUMN Supplier VARCHAR(100) NULL
COMMIT