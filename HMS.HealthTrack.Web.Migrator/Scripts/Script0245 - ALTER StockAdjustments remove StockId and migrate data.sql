BEGIN TRAN

INSERT INTO Inventory.StockAdjustmentStock
(
    StockAdjustmentId,
    StockId
)
SELECT sa.StockAdjustmentId, sa.StockId
FROM Inventory.StockAdjustment sa
GO

ALTER TABLE Inventory.StockAdjustment
	DROP CONSTRAINT FK_Inventory_Consumption_Inventory_Stock
GO

ALTER TABLE Inventory.StockAdjustment
	DROP COLUMN StockId
GO

COMMIT	


