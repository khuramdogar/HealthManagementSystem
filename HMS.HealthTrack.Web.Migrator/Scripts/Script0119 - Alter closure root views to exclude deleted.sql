/****** Object:  View [Inventory].[CategoryClosureRoots]    Script Date: 10/09/2015 1:34:05 PM ******/
DROP VIEW [Inventory].[CategoryClosureRoots]
GO

/****** Object:  View [Inventory].[CategoryClosureRoots]    Script Date: 10/09/2015 1:34:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [Inventory].[CategoryClosureRoots] AS
SELECT cat.CategoryId
FROM Inventory.Category cat
LEFT OUTER JOIN (
	SELECT c.ChildId, c.parentId, c.depth
	FROM Inventory.CategoryClosure c
	RIGHT OUTER JOIN Inventory.CategoryClosure p
		on c.ParentId <> p.ChildId 
			AND c.ChildId = p.ChildId
) lo on cat.CategoryId = lo.ChildId
WHERE lo.ChildId IS NULL AND cat.DeletedOn IS NULL
GO

/****** Object:  View [Inventory].[GeneralLedgerClosureRoots]    Script Date: 10/09/2015 1:34:17 PM ******/
DROP VIEW [Inventory].[GeneralLedgerClosureRoots]
GO

/****** Object:  View [Inventory].[GeneralLedgerClosureRoots]    Script Date: 10/09/2015 1:34:17 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [Inventory].[GeneralLedgerClosureRoots] AS
SELECT gl.LedgerId
FROM Inventory.GeneralLedger gl
LEFT OUTER JOIN (
	SELECT c.ChildId, c.parentId, c.depth
	FROM Inventory.GeneralLedgerClosure c
	RIGHT OUTER JOIN Inventory.GeneralLedgerClosure p
		on c.ParentId <> p.ChildId 
			AND c.ChildId = p.ChildId
) lo on gl.LedgerId = lo.ChildId
WHERE lo.ChildId IS NULL AND gl.DeletedOn IS NULL
GO



