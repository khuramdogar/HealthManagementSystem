Create view Inventory.ConsumptionRequiringPaymentClass as
SELECT        c.ConsumptionId, c.StockId, c.ConsumedOn, c.ConsumedBy, c.DeletedOn, c.QuantityConsumed, c.Status, p.ProductId, p.SPC, p.Description, p.Manufacturer, p.PrimarySupplier, p.SecondarySupplier, 
                         p.RequiresBatchNumber, p.IsConsignment, p.MinimumOrder, p.RecommendedOrder, p.OrderMultiple, p.ReorderThreshold, p.TargetStockLevel, p.OrderableUnits, pat.patient_ID, pat.firstName, pat.surname, 
                         pat.dob, pat.mrn, pat.patientType, pat.homePhone, pat.workPhone, pat.mobile, pat.email, pat.pStatus, pat.medicare, pat.medicareRefNo, pat.MediExpYYYY, pat.MediExpMM, pat.medicareVerifyDate, pat.vetAffairs,
                          pat.healthFundID, pat.fundNumber, pat.fundNumberUPI, pat.fundExpYYYY, pat.fundExpMM, pat.fundPlan, pat.feeTable_ID, pat.billingType, pat.HospitalBillingClass, pat.pensionNumber, pat.MRN_Feed, 
                         b.dateTimeStart, b.booking_ID, b.dateTimeStart AS BookingStartedOn, b.dateTimeEnd, b.duration, b.type, b.location_ID, b.room_ID, b.PaymentClass, b.PaymentClassUserUpdated, b.PaymentClassDateUpdated, 
                         p.RequiresSerialNumber, c.ClinicalRecordId, pat.title
FROM            Inventory.Consumption AS c INNER JOIN
                         dbo.MR_Container AS cr ON cr.container_ID = c.ClinicalRecordId INNER JOIN
                         dbo.Booking AS b ON cr.booking_ID = b.booking_ID INNER JOIN
                         dbo.Patient AS pat ON cr.patient_ID = pat.patient_ID INNER JOIN
                         Inventory.Stock AS s ON c.StockId = s.StockId INNER JOIN
                         Inventory.Product AS p ON s.ProductId = p.ProductId
WHERE        (p.ProductId IN
                             (SELECT        pc.Inv_ID
                               FROM            Inventory.Product_Category AS pc INNER JOIN
                                                         Inventory.Category AS c ON pc.CategoryId = c.CategoryId
                               WHERE        (c.RequiresPaymentClass = 1)))