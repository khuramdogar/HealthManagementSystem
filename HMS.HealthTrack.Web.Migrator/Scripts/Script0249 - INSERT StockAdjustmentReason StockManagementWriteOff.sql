
IF NOT EXISTS (SELECT 1 FROM INVENTORY.STOCKADJUSTMENTREASON WHERE NAME = 'Stock management write off')
INSERT INTO inventory.StockAdjustmentReason
(
    --StockAdjustmentReasonId - this column value is auto-generated
    Name,
    Description,
    Disabled,
    CreatedOn,
    CreatedBy,
    LastModifiedOn,
    LastModifiedUser,
    DeletedOn,
    DeletedBy
)
VALUES
(
    -- StockAdjustmentReasonId - int
    'Stock management write off', -- Name - varchar
    'Stock that has been written off as a result of no longer being managed by the system.', -- Description - varchar
    0, -- Disabled - bit
    '2016-05-13 10:31:51', -- CreatedOn - datetime
    'Daniel', -- CreatedBy - varchar
    '2016-05-13 10:31:51', -- LastModifiedOn - datetime
    'Daniel', -- LastModifiedUser - varchar
    NULL, -- DeletedOn - datetime
    NULL -- DeletedBy - varchar
)
GO