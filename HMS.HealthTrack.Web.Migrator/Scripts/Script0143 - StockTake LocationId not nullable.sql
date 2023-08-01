BEGIN TRANSACTION 

UPDATE Inventory.StockTake
SET LocationId = (
   SELECT TOP 1 LocationId
   FROM Inventory.Location
   WHERE DeletedOn IS NULL
)
WHERE LocationId is null

COMMIT

BEGIN TRANSACTION
ALTER TABLE Inventory.StockTake
ALTER COLUMN LocationId int NOT NULL 

COMMIT