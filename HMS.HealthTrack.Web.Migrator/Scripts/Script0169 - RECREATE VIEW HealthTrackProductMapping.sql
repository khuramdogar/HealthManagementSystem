IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'HealthTrackProductMapping' AND  TABLE_SCHEMA = 'Inventory')
	DROP VIEW Inventory.HealthTrackProductMapping
GO

CREATE VIEW Inventory.HealthTrackProductMapping 
AS
SELECT *
FROM Inventory.ExternalProductMapping epm
LEFT JOIN Inventory_Master im on epm.ExternalProductId = im.Inv_ID
WHERE epm.DeletedOn IS NULL AND im.deleted = 0
