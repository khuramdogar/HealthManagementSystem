/****** Object:  View [Inventory].[ClinicalConsumption]    Script Date: 28/02/2017 4:18:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [Inventory].[ClinicalConsumption]
AS
SELECT        Inventory.HealthTrackConsumption.UsedId, mrn.RemotePatient_ID, dbo.MR_Container.container_ID, Inventory.HealthTrackConsumption.SerialNumber, Inventory.HealthTrackConsumption.Name, 
                         Inventory.HealthTrackConsumption.ProductId, Inventory.HealthTrackConsumption.SPC, Inventory.HealthTrackConsumption.Manufacturer, dbo.MR_Container.patient_ID, dbo.MR_Container.testDate, 
                         dbo.Patient.firstName, dbo.Patient.surname, dbo.Patient.patientType, dbo.Patient.healthFundID, dbo.Staff_Used.StaffName, dbo.Staff_Used.AdditionalInfo, Inventory.HealthTrackConsumption.RebateCode, 
                         Inventory.MedicareProducts.MinBenefit
FROM            dbo.Patient INNER JOIN
                         dbo.MR_Container ON dbo.Patient.patient_ID = dbo.MR_Container.patient_ID INNER JOIN
                         Inventory.HealthTrackConsumption ON dbo.MR_Container.container_ID = Inventory.HealthTrackConsumption.ContainerId INNER JOIN
                         dbo.Staff_Used ON dbo.MR_Container.container_ID = dbo.Staff_Used.Container_ID AND dbo.Staff_Used.AdditionalInfo = 'R' LEFT OUTER JOIN
                         Inventory.MedicareProducts ON Inventory.HealthTrackConsumption.RebateCode = Inventory.MedicareProducts.Code LEFT OUTER JOIN
                         Inventory.PatientContainerMRN AS mrn ON dbo.MR_Container.container_ID = mrn.container_ID

GO