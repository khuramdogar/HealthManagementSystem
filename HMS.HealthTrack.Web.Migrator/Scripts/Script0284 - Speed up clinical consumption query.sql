CREATE INDEX MedicareRebateCodeIndex
ON Inventory.MedicareProducts (Code)

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [Inventory].[ClinicalConsumption]
AS
SELECT        inventory_used.invUsed_ID AS UsedId, mrn.RemotePatient_ID,
 Inventory_Used.invUsed_SerialNo as SerialNumber, m.Inv_Description as Name, 
                         m.Inv_ID AS ProductId, m.Inv_SPC as SPC,  m.Manufacturer, p.RebateCode, 
						 container.container_ID,container.patient_ID, container.testDate, 
                         dbo.Patient.firstName, Patient.surname, dbo.Patient.patientType, dbo.Patient.healthFundID, 
						 Staff_Used.StaffName, Staff_Used.AdditionalInfo
                         ,Inventory.MedicareProducts.MinBenefit
FROM            dbo.Patient 
INNER JOIN dbo.MR_Container as container ON dbo.Patient.patient_ID = container.patient_ID 
INNER JOIN Inventory_Used on Inventory_used.container_ID = container.container_ID
INNER JOIN dbo.Inventory_Master AS m ON Inventory_Used.invItem_ID = m.Inv_ID
LEFT OUTER JOIN Inventory.ExternalProductMapping AS epm ON m.Inv_ID = epm.ExternalProductId AND epm.DeletedOn IS NULL 
Left outer join Inventory.Product AS p ON epm.InventoryProductId = p.ProductId						 
INNER JOIN dbo.Staff_Used ON container.container_ID = dbo.Staff_Used.Container_ID
LEFT OUTER JOIN Inventory.MedicareProducts ON p.RebateCode = Inventory.MedicareProducts.Code
LEFT OUTER JOIN Inventory.PatientContainerMRN AS mrn ON container.container_ID = mrn.container_ID
WHERE (epm.DeletedOn IS NULL AND dbo.Staff_Used.AdditionalInfo = 'R')
GO
