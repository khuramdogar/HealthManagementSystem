/****** Object:  View [Inventory].[ConsumptionRequiringPaymentClass]    Script Date: 7/04/2015 5:05:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER view [Inventory].[ConsumptionRequiringPaymentClass] as
SELECT        c.ConsumptionId, c.StockId, c.ConsumedOn, c.ConsumedBy, c.DeletedOn, c.QuantityConsumed, c.Status, p.ProductId, p.SPC, p.Description, p.Manufacturer, p.PrimarySupplier, p.SecondarySupplier, 
                         p.IsConsignment, p.MinimumOrder, p.RecommendedOrder, p.OrderMultiple, p.ReorderThreshold, p.TargetStockLevel, p.OrderableUnits, pat.patient_ID, pat.firstName, pat.surname, pat.dob, pat.mrn, 
                         pat.patientType, pat.homePhone, pat.workPhone, pat.mobile, pat.email, pat.pStatus, pat.medicare, pat.medicareRefNo, pat.MediExpYYYY, pat.MediExpMM, pat.medicareVerifyDate, pat.vetAffairs, 
                         pat.healthFundID, pat.fundNumber, pat.fundNumberUPI, pat.fundExpYYYY, pat.fundExpMM, pat.fundPlan, pat.feeTable_ID, pat.billingType, pat.HospitalBillingClass, pat.pensionNumber, pat.MRN_Feed, 
                         b.dateTimeStart, b.booking_ID, b.dateTimeStart AS BookingStartedOn, b.dateTimeEnd, b.duration, b.type, b.location_ID, b.room_ID, b.PaymentClass, b.PaymentClassUserUpdated, b.PaymentClassDateUpdated, 
                         c.ClinicalRecordId, pat.title
FROM            Inventory.Consumption AS c INNER JOIN
                         dbo.MR_Container AS cr ON cr.container_ID = c.ClinicalRecordId INNER JOIN
                         dbo.Booking AS b ON cr.booking_ID = b.booking_ID INNER JOIN
                         dbo.Patient AS pat ON cr.patient_ID = pat.patient_ID INNER JOIN
                         Inventory.Stock AS s ON c.StockId = s.StockId INNER JOIN
                         Inventory.Product AS p ON s.ProductId = p.ProductId
WHERE        (p.ProductId IN
                             (SELECT ps.ProductId FROM Inventory.ProductSetting ps
                               WHERE (ps.SettingId = 'RPC')))
GO


