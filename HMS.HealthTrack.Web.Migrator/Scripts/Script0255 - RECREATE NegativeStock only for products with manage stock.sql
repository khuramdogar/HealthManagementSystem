/****** Object:  View [Inventory].[NegativeStock]    Script Date: 17-May-16 12:39:08 PM ******/
DROP VIEW [Inventory].[NegativeStock]
GO

/****** Object:  View [Inventory].[NegativeStock]    Script Date: 17-May-16 12:39:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [Inventory].[NegativeStock] AS 
WITH AvailableStock (ProductId, StoredAt) AS (
	SELECT s.ProductId, StoredAt
	FROM Inventory.Stock s 
	INNER JOIN Inventory.Product p on s.ProductId = s.ProductId
	WHERE s.DeletedOn IS NULL AND p.DeletedOn IS NULL
		AND StockStatus = 0 
	GROUP BY s.ProductId, StoredAt
	
), MostRecentNonNegativeConsumption (ProductId, StoredAt, MostRecent, MostRecentDate) AS (
		SELECT s.ProductId, s.StoredAt, MAX(sas.StockId) AS MostRecent, MAX(sa.AdjustedOn) AS MostRecentDate -- most recently consumed non-negative stock
		FROM Inventory.StockAdjustment sa
		INNER JOIN Inventory.StockAdjustmentStock sas ON sa.StockAdjustmentId = sas.StockAdjustmentId
		INNER JOIN Inventory.Stock s on s.StockId = sas.StockId
		INNER JOIN Inventory.Product p on s.ProductId = p.ProductId
		WHERE s.IsNegative = 0 AND p.DeletedOn IS NULL AND sa.DeletedOn IS NULL AND sa.IsPositive = 0
		GROUP BY s.ProductId, s.StoredAt
), UnavailableStock (StockId, ProductId, StoredAt, ReceivedQuantity, IsNegative) AS (
	SELECT s.StockId, s.ProductId, s.StoredAt, ReceivedQuantity, IsNegative
	FROM Inventory.Stock s
	INNER JOIN Inventory.Product p on s.ProductId = p.ProductId
	LEFT JOIN AvailableStock avs on s.ProductId = avs.ProductId AND s.StoredAt = avs.StoredAt
	WHERE avs.ProductId IS NULL AND s.DeletedOn IS NULL AND p.DeletedOn IS NULL 
), MostRecentStockTake (ProductId, StockTakeDate, LocationId) AS (
	SELECT p.ProductId, MAX(st.StockTakeDate), st.LocationId
	FROM Inventory.Product p
	LEFT JOIN Inventory.StockTakeItem sti on p.ProductId = sti.ProductId
	INNER JOIN Inventory.StockTake st on sti.StockTakeId = st.StockTakeId
	WHERE 
		p.DeletedOn IS NULL AND
		st.[Status] = 3 AND st.DeletedOn IS NULL AND st.[Source] = 0 AND -- STOCK TAKE FILTERING
		sti.[Status] = 2 AND sti.DeletedOn IS NULL AND sti.ProcessedOn IS NOT NULL -- STOCK TAKE ITEM FILTERING
GROUP BY p.ProductId, st.LocationId)


SELECT us.ProductId, us.StoredAt, SUM(us.ReceivedQuantity) AS NegativeQuantity
FROM Inventory.StockAdjustment sa
INNER JOIN Inventory.StockAdjustmentStock sas ON sa.StockAdjustmentId = sas.StockAdjustmentId
INNER JOIN Inventory.Stock s ON sas.StockId = s.StockId
INNER JOIN UnavailableStock us on s.StockId = us.StockId
INNER JOIN Inventory.Product p ON us.ProductId = p.ProductId
LEFT JOIN MostRecentNonNegativeConsumption c on us.ProductId = c.ProductId and us.StoredAt = c.StoredAt
LEFT JOIN MostRecentStockTake st on us.ProductId = st.ProductId AND us.StoredAt = st.LocationId
WHERE us.IsNegative = 1 AND (c.MostRecent IS NULL OR s.StockId > c.MostRecent) AND (st.StockTakeDate IS NULL OR st.StockTakeDate < c.MostRecentDate) AND sa.DeletedOn IS NULL AND sa.IsPositive = 0 AND p.ManageStock = 1
GROUP BY us.ProductId, us.StoredAt
GO