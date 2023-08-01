/****** Object:  View [Inventory].[HealthTrackConsumption]    Script Date: 31/08/2015 3:14:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [Inventory].[HealthTrackConsumption]
AS
SELECT        u.invUsed_ID AS UsedId, m.Inv_ID AS ProductId, m.Inv_SPC AS SPC, m.Inv_LPC AS LPC, m.Inv_UPN AS ScanCode, m.Inv_Description AS Name,u.LOTNO as LotNumber, u.invUsed_SerialNo AS SerialNumber, u.invUsed_Qty AS Quantity, 
                         u.invUsed_Location AS Location,
                             (SELECT        TOP (1) pm.RemotePatient_ID
                               FROM            dbo.HL7_PatientMapping AS pm INNER JOIN
                                                         dbo.Location AS loc ON u.invUsed_Location = loc.location_ID
                               WHERE        (pm.LocalPatient_ID = u.patient_ID) AND (pm.Feed_ID =
                                                             (SELECT        TOP (1) DEPT_FeedID
                                                               FROM            dbo.Department
                                                               WHERE        (DEPT_ID = loc.Department)))) AS PatientMRN, u.container_ID AS ContainerId, u.patient_ID AS PatientId, m.Inv_Group AS ClinicalGroup, m.Inv_SubGroup AS ClinicalSubGroup, 
                         m.Manufacturer, m.Inv_GL AS GL, m.Billing_Code AS RebateCode, m.Description_Additional AS Description, m.Inv_BuyPrice AS Price, m.Inv_BuyCurrency AS BuyCurrency, 
                         m.Inv_BuyCurrencyRate AS BuyCurrencyRate, u.deleted, u.deletionDate, u.deletionUser, u.dateLastModified, u.userLastModified, u.dateCreated, u.userCreated, u.StockStatus, u.StatusMessage
FROM            dbo.Inventory_Used AS u INNER JOIN
                         dbo.Inventory_Master AS m ON u.invItem_ID = m.Inv_ID
GO


