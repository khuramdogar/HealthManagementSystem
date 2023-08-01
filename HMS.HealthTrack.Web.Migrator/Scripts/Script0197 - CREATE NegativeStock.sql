CREATE VIEW Inventory.NegativeStock AS 
WITH AvailableStock (ProductId, StoredAt) AS (
	SELECT ProductId, StoredAt
	FROM Inventory.Stock
	WHERE DeletedOn IS NULL
		AND StockStatus = 0
	GROUP BY ProductId, StoredAt
), MostRecentNonNegativeConsumption (ProductId, StoredAt, MostRecent) AS (
		SELECT s.ProductId, s.StoredAt, MAX(sd.ConsumedOn) AS MostRecent -- most recently consumed non-negative stock
		FROM Inventory.StockDeduction sd
		INNER JOIN Inventory.Stock s on s.StockId = sd.StockId
		WHERE s.IsNegative = 0
		GROUP BY s.ProductId, s.StoredAt
), UnavailableStock (StockId, ProductId, StoredAt, ReceivedQuantity, IsNegative) AS (
	SELECT s.StockId, s.ProductId, s.StoredAt, ReceivedQuantity, IsNegative
	FROM Inventory.Stock s
	LEFT JOIN AvailableStock avs on s.ProductId = avs.ProductId AND s.StoredAt = avs.StoredAt
	WHERE avs.ProductId IS NULL AND s.DeletedOn IS NULL
)

SELECT us.ProductId, us.StoredAt, SUM(us.ReceivedQuantity) AS NegativeQuantity
FROM Inventory.StockDeduction sd
INNER JOIN UnavailableStock us on sd.StockId = us.StockId
LEFT JOIN MostRecentNonNegativeConsumption c on us.ProductId = c.ProductId and us.StoredAt = c.StoredAt
WHERE us.IsNegative = 1 AND (c.MostRecent IS NULL OR sd.ConsumedOn > c.MostRecent)
GROUP BY us.ProductId, us.StoredAt