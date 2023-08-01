Create view [inventory].[ClinicalConsumption] AS
 
SELECT        Inventory.Consumption.ConsumptionId, dbo.MR_Container.container_ID, Inventory.Stock.SerialNumber, Inventory.Product.Description, Inventory.Product.ProductId, Inventory.Product.SPC, 
                         Inventory.Product.Manufacturer, dbo.MR_Container.patient_ID, dbo.MR_Container.testDate, dbo.Patient.firstName, dbo.Patient.surname, dbo.Patient.patientType, dbo.Patient.healthFundID, 
                         dbo.Staff_Used.StaffName, dbo.Staff_Used.AdditionalInfo
FROM            dbo.Patient INNER JOIN
                         dbo.MR_Container ON dbo.Patient.patient_ID = dbo.MR_Container.patient_ID INNER JOIN
                         Inventory.Consumption ON dbo.MR_Container.container_ID = Inventory.Consumption.ClinicalRecordId INNER JOIN
                         Inventory.Stock ON Inventory.Consumption.StockId = Inventory.Stock.StockId AND Inventory.Consumption.StockId = Inventory.Stock.StockId INNER JOIN
                         Inventory.Product ON Inventory.Stock.ProductId = Inventory.Product.ProductId AND Inventory.Stock.ProductId = Inventory.Product.ProductId INNER JOIN
                         dbo.Staff_Used ON dbo.MR_Container.container_ID = dbo.Staff_Used.Container_ID
