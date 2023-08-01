BEGIN TRANSACTION
  
  delete from Inventory.GeneralLedgerClosure
  where exists (
	select 1 
	from inventory.generalledger
	where LedgerType = 2 and LedgerId = ChildId)
	

INSERT INTO Inventory.GeneralLedgerClosure
	(ParentId,
	ChildId,
	Depth)
SELECT LedgerId, LedgerId, 0
FROM Inventory.GeneralLedger
where not exists (
	select 1
	from inventory.GeneralLedgerClosure
	where ParentId = LedgerId)
COMMIT
	
