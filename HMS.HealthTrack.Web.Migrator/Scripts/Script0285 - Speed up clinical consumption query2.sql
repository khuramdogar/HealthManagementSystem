/****** Object:  View [Inventory].[ClinicalConsumption]    Script Date: 16/06/2017 12:26:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [Inventory].[ClinicalConsumption]
AS
SELECT        htc.UsedId, htc.PatientMRN as RemotePatient_ID, dbo.MR_Container.container_ID, 
						htc.Name, htc.ProductId, htc.SPC, htc.Manufacturer,htc.RebateCode
						,dbo.MR_Container.patient_ID, dbo.MR_Container.testDate, 
                         dbo.Patient.firstName, dbo.Patient.surname, dbo.Staff_Used.StaffName,  
                         Inventory.MedicareProducts.MinBenefit
FROM            dbo.Patient INNER JOIN
                         dbo.MR_Container ON dbo.Patient.patient_ID = dbo.MR_Container.patient_ID INNER JOIN
                         Inventory.HealthTrackConsumption as htc ON dbo.MR_Container.container_ID = htc.ContainerId INNER JOIN
                         dbo.Staff_Used ON dbo.MR_Container.container_ID = dbo.Staff_Used.Container_ID AND dbo.Staff_Used.AdditionalInfo = 'R' LEFT OUTER JOIN
                         Inventory.MedicareProducts ON htc.RebateCode = Inventory.MedicareProducts.Code
GO


