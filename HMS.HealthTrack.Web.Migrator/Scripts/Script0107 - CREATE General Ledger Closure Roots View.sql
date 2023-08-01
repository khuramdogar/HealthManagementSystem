CREATE VIEW Inventory.GeneralLedgerClosureRoots AS
SELECT gl.LedgerId
FROM Inventory.GeneralLedger gl
LEFT OUTER JOIN (
	SELECT c.ChildId, c.parentId, c.depth
	FROM Inventory.GeneralLedgerClosure c
	RIGHT OUTER JOIN Inventory.GeneralLedgerClosure p
		on c.ParentId <> p.ChildId 
			AND c.ChildId = p.ChildId
) lo on gl.LedgerId = lo.ChildId
WHERE lo.ChildId IS NULL
