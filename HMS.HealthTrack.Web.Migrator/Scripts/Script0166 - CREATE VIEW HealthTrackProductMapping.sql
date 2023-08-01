CREATE VIEW Inventory.HealthTrackProductMapping 
AS
SELECT *
FROM Inventory.ExternalProductMapping epm
LEFT JOIN Inventory_Master im on epm.ExternalProductId = im.Inv_ID
WHERE epm.DeletedOn IS NULL AND im.deleted = 0