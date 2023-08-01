BEGIN TRANSACTION
INSERT INTO [Inventory].[CategoryClosure]
           ([ParentId]
           ,[ChildId]
           ,[Depth])
SELECT cat.CategoryId, cat.CategoryId, 0 as Depth
FROM Inventory.Category cat
WHERE cat.ParentId IS NULL
UNION
SELECT cat.ParentId, cat.ParentId, 0 as Depth
FROM Inventory.Category cat
WHERE cat.ParentId IS NOT NULL
UNION
SELECT cat.ParentId, cat.CategoryId, 1 as Depth
FROM Inventory.Category cat
WHERE cat.ParentId IS NOT NULL

COMMIT

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Category
	DROP CONSTRAINT FK_Inventory_Category_Inventory_Category
GO
ALTER TABLE Inventory.Category
	DROP CONSTRAINT Unique_CategoryName_ParentId
GO
ALTER TABLE Inventory.Category
	DROP COLUMN ParentId
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Category ADD
	Disinherit bit NOT NULL CONSTRAINT DF_Category_Disinherit DEFAULT (0)
GO
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
