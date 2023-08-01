ALTER view [Inventory].[PatientContainerMRN] AS

SELECT lookup.RemotePatient_ID, c.container_ID,c.patient_ID
FROM
MR_Container c
INNER JOIN dbo.Location l ON c.Location_ID=l.location_ID
INNER JOIN dbo.Department d ON l.Department=d.DEPT_ID
CROSS APPLY (
SELECT TOP 1 * 
FROM (
SELECT TOP 1 'A' AS FoundMap, Feed_ID, RemotePatient_ID, LocalPatient_ID, Merged
FROM HL7_PatientMapping pm
WHERE ( pm.Feed_ID = d.DEPT_FeedID) and pm.LocalPatient_ID = c.patient_ID
ORDER BY pm.Merged
UNION SELECT TOP 1 'B' AS FoundMap, NULL AS Feed_ID, CAST(NULL as varchar(64)) as RemotePatient_ID, c.patient_ID, 1 AS Merged
) pm
ORDER BY FoundMap, Merged
) lookup

WHERE c.Location_ID NOT IN (1)
GO