DROP VIEW Inventory.NegativeStock
GO

CREATE VIEW Inventory.NegativeStock AS 
WITH AvailableStock (ProductId, StoredAt) AS (
	SELECT s.ProductId, StoredAt
	FROM Inventory.Stock s 
	INNER JOIN Inventory.Product p on s.ProductId = s.ProductId
	WHERE s.DeletedOn IS NULL AND p.DeletedOn IS NULL
		AND StockStatus = 0 AND s.ProductId = 9043
	GROUP BY s.ProductId, StoredAt
	
), MostRecentNonNegativeConsumption (ProductId, StoredAt, MostRecent) AS (
		SELECT s.ProductId, s.StoredAt, MAX(sd.StockId) AS MostRecent -- most recently consumed non-negative stock
		FROM Inventory.StockDeduction sd
		INNER JOIN Inventory.Stock s on s.StockId = sd.StockId
		INNER JOIN Inventory.Product p on s.ProductId = p.ProductId
		WHERE s.IsNegative = 0 AND p.DeletedOn IS NULL AND sd.DeletedOn IS NULL AND s.ProductId = 9043
		GROUP BY s.ProductId, s.StoredAt
), UnavailableStock (StockId, ProductId, StoredAt, ReceivedQuantity, IsNegative) AS (
	SELECT s.StockId, s.ProductId, s.StoredAt, ReceivedQuantity, IsNegative
	FROM Inventory.Stock s
	INNER JOIN Inventory.Product p on s.ProductId = p.ProductId
	LEFT JOIN AvailableStock avs on s.ProductId = avs.ProductId AND s.StoredAt = avs.StoredAt
	WHERE avs.ProductId IS NULL AND s.DeletedOn IS NULL AND p.DeletedOn IS NULL 
)


SELECT us.ProductId, us.StoredAt, SUM(us.ReceivedQuantity) AS NegativeQuantity
FROM Inventory.StockDeduction sd
INNER JOIN UnavailableStock us on sd.StockId = us.StockId
LEFT JOIN MostRecentNonNegativeConsumption c on us.ProductId = c.ProductId and us.StoredAt = c.StoredAt
WHERE us.IsNegative = 1 AND (c.MostRecent IS NULL OR sd.StockId > c.MostRecent) AND sd.DeletedOn IS NULL
GROUP BY us.ProductId, us.StoredAt