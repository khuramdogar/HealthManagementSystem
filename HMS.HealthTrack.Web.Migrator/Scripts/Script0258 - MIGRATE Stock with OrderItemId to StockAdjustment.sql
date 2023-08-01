BEGIN TRANSACTION


ALTER TABLE Inventory.StockAdjustment
ADD StockId bigint NULL
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
    Status,
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

SELECT s.ReceivedOn AS AdjustedOn, 
	s.ReceivedBy AS AdjustedBy,
	s.CreatedBy,
	s.CreatedOn,
	s.DeletedOn,
	s.DeletedBy,
	s.DeletionReason,
	s.ReceivedQuantity AS Quantity,
	0 AS [Status],
	NULL AS PatientId,
	NULL AS ClinicalRecordId,
	4 AS Source, -- Order
	s.LastModifiedBy,
	s.LastModifiedOn,
	NULL AS StockTakeItemId,
	NULL AS StockTakeAdjustmentReasonId,
	NULL AS GeneralLedgerId,
	NULL AS Note,
	1 AS IsPositive,
	s.OrderItemId,
	s.StockId
FROM Inventory.Stock s
WHERE s.OrderItemId IS NOT NULL

INSERT INTO Inventory.StockAdjustmentStock
(
    StockAdjustmentId,
    StockId
)
SELECT sa.StockAdjustmentId, sa.StockId
FROM Inventory.StockAdjustment sa
WHERE sa.StockId IS NOT NULL
GO

ALTER TABLE Inventory.StockAdjustment
DROP COLUMN StockId
GO

COMMIT
