Create view Inventory.ConsumptionBooking as
SELECT        Inventory.Consumption.ConsumptionId, dbo.MR_Container.container_ID, Inventory.Stock.SerialNumber, Inventory.Product.Description, Inventory.Product.ProductId, Inventory.Product.SPC, 
                         Inventory.Product.Manufacturer, dbo.MR_Container.patient_ID, dbo.MR_Container.testDate, dbo.Booking.PaymentClass
FROM            dbo.MR_Container INNER JOIN
                         Inventory.Consumption ON dbo.MR_Container.container_ID = Inventory.Consumption.ClinicalRecordId INNER JOIN
                         Inventory.Stock ON Inventory.Consumption.StockId = Inventory.Stock.StockId AND Inventory.Consumption.StockId = Inventory.Stock.StockId INNER JOIN
                         Inventory.Product ON Inventory.Stock.ProductId = Inventory.Product.ProductId AND Inventory.Stock.ProductId = Inventory.Product.ProductId INNER JOIN
                         dbo.Booking ON dbo.MR_Container.booking_ID = dbo.Booking.booking_ID