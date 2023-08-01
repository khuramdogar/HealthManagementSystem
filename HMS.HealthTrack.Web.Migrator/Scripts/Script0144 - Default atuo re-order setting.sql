Begin transaction
UPDATE Inventory.Product SET ReorderSetting=0 WHERE ReorderSetting IS NULL
ALTER TABLE Inventory.Product ALTER COLUMN ReorderSetting INTEGER NOT NULL

ALTER TABLE Inventory.Product ADD CONSTRAINT
   DF_Product_ReorderSetting DEFAULT 0 FOR ReorderSetting
GO
commit