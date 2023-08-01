/****** Object:  View [Inventory].[ConsumptionRequiringPaymentClass]    Script Date: 03-May-16 3:34:07 PM ******/
DROP VIEW [Inventory].[ConsumptionRequiringPaymentClass]
GO

/****** Object:  View [Inventory].[ConsumptionRequiringPaymentClass]    Script Date: 03-May-16 3:34:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE view [Inventory].[DeductionRequiringPaymentClass] as
SELECT        sa.StockAdjustmentId, sa.StockId, sa.AdjustedOn, sa.AdjustedBy, sa.DeletedOn, sa.Quantity, sa.Status	, p.ProductId, p.SPC, p.Description, p.Manufacturer, p.PrimarySupplier, p.SecondarySupplier, 
                         p.IsConsignment, p.MinimumOrder, p.OrderMultiple, p.ReorderThreshold, p.TargetStockLevel, pat.patient_ID, pat.firstName, pat.surname, pat.dob, pat.mrn, pat.patientType, pat.homePhone, pat.workPhone, 
                         pat.mobile, pat.email, pat.pStatus, pat.medicare, pat.medicareRefNo, pat.MediExpYYYY, pat.MediExpMM, pat.medicareVerifyDate, pat.vetAffairs, pat.healthFundID, pat.fundNumber, pat.fundNumberUPI, 
                         pat.fundExpYYYY, pat.fundExpMM, pat.fundPlan, pat.feeTable_ID, pat.billingType, pat.HospitalBillingClass, pat.pensionNumber, pat.MRN_Feed, b.dateTimeStart, b.booking_ID, 
                         b.dateTimeStart AS BookingStartedOn, b.dateTimeEnd, b.duration, b.type, b.location_ID, b.room_ID, b.PaymentClass, b.PaymentClassUserUpdated, b.PaymentClassDateUpdated, sa.ClinicalRecordId, pat.title
FROM            Inventory.StockAdjustment AS sa INNER JOIN
                         dbo.MR_Container AS cr ON cr.container_ID = ClinicalRecordId INNER JOIN
                         dbo.Booking AS b ON cr.booking_ID = b.booking_ID INNER JOIN
                         dbo.Patient AS pat ON cr.patient_ID = pat.patient_ID INNER JOIN
                         Inventory.Stock AS s ON sa.StockId = s.StockId INNER JOIN
                         Inventory.Product AS p ON s.ProductId = p.ProductId
WHERE        sa.IsPositive = 0 AND (p.ProductId IN
                             (SELECT        ProductId
                               FROM            Inventory.ProductSetting AS ps
                               WHERE        (SettingId = 'RPC')))
GO




/****** Object:  View [Inventory].[NegativeStock]    Script Date: 03-May-16 3:41:20 PM ******/
DROP VIEW [Inventory].[NegativeStock]
GO

/****** Object:  View [Inventory].[NegativeStock]    Script Date: 03-May-16 3:41:20 PM ******/
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
		SELECT s.ProductId, s.StoredAt, MAX(sa.StockId) AS MostRecent, MAX(sa.AdjustedOn) AS MostRecentDate -- most recently consumed non-negative stock
		FROM Inventory.StockAdjustment sa
		INNER JOIN Inventory.Stock s on s.StockId = sa.StockId
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
INNER JOIN UnavailableStock us on sa.StockId = us.StockId
LEFT JOIN MostRecentNonNegativeConsumption c on us.ProductId = c.ProductId and us.StoredAt = c.StoredAt
LEFT JOIN MostRecentStockTake st on us.ProductId = st.ProductId AND us.StoredAt = st.LocationId
WHERE us.IsNegative = 1 AND (c.MostRecent IS NULL OR sa.StockId > c.MostRecent) AND (st.StockTakeDate IS NULL OR st.StockTakeDate < c.MostRecentDate) AND sa.DeletedOn IS NULL AND sa.IsPositive = 0
GROUP BY us.ProductId, us.StoredAt
GO


