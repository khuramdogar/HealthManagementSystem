BEGIN TRANSACTION
ALTER TABLE Inventory.Product ADD CONSTRAINT
   DF_Product_ReportConsumption DEFAULT 0 FOR ReportConsumption
GO

UPDATE Inventory.Product SET ReportConsumption = 0 WHERE ReportConsumption IS NULL
ALTER TABLE Inventory.Product ALTER COLUMN ReportConsumption bit NOT NULL

COMMIT