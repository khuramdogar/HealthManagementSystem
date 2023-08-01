BEGIN TRANSACTION

ALTER TABLE [Inventory].[ProductImportData] DROP CONSTRAINT [DF_ProductImportSet_ImportedOn]
GO

  ALTER TABLE Inventory.ProductImportData
  ALTER COLUMN ImportedOn DateTime NULL
  GO

COMMIT

UPDATE Inventory.ProductImportData
SET ImportedOn = NULL
WHERE Status != 2
GO
