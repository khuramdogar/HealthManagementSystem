--Archives any ignored consumptions that are not already archived
begin transaction
update [Inventory].[ConsumptionNotificationManagement]
set ArchivedBy = 'SRX17439'
where ProcessingStatus = 6 and ArchivedOn is null

update [Inventory].[ConsumptionNotificationManagement]
set ArchivedOn = GETDATE()
where ArchivedBy = 'SRX17439'
and ArchivedOn is null

commit