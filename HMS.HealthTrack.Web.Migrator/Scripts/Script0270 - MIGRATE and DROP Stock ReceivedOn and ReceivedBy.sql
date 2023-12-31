/* Migrate the data first. */

BEGIN TRANSACTION

ALTER TABLE Inventory.StockAdjustment ADD
	StockId bigint NULL
GO

INSERT INTO Inventory.StockAdjustment 
(
AdjustedOn,
AdjustedBy,
CreatedBy,
CreatedOn,
DeletedOn,
DeletedBy,
DeletionReason,
Quantity,
PatientId,
ClinicalRecordId,
Source,
LastModifiedBy,
LastModifiedOn,
StockTakeItemId,
StockAdjustmentReasonId,
GeneralLedgerId,
Note,
IsPositive,
OrderItemId,
StockId
)
SELECT 
s.ReceivedOn AS AdjustedOn,
s.ReceivedBy AS AdjustedBy,
s.CreatedBy,
s.CreatedOn,
s.DeletedOn, 
s.DeletedBy,
s.DeletionReason,
s.ReceivedQuantity, 
NULL,
NULL,
2 AS Source, -- Web
s.LastModifiedBy,
s.LastModifiedOn,
NULL,
NULL,
NULL,
NULL,
1 AS IsPositive,
NULL,
StockId
FROM Inventory.Stock s
WHERE s.StockId NOT IN (
   SELECT s.StockId
   FROM Inventory.StockAdjustment
   INNER JOIN Inventory.StockAdjustmentStock sas ON sas.StockAdjustmentId = Inventory.StockAdjustment.StockAdjustmentId
   INNER JOIN Inventory.Stock s ON s.StockId = sas.StockId
   WHERE IsPositive = 1 AND OrderItemId IS NOT NULL
)
GO

INSERT INTO Inventory.StockAdjustmentStock
(StockAdjustmentId, StockId)
SELECT sa.StockAdjustmentId, sa.StockId
FROM Inventory.StockAdjustment sa
WHERE sa.StockId IS NOT NULL


ALTER TABLE Inventory.StockAdjustment
DROP COLUMN StockId

COMMIT

/*
   Tuesday, May 31, 201612:33:07 PM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
   Database: HealthTrack_Web
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Stock
	DROP COLUMN ReceivedOn, ReceivedBy
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
