/*
   Monday, 31 August 201511:44:21 AM
   User: 
   Server: devdaniel
   Database: HealthTrack_Web
   Application: 
*/

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
ALTER TABLE Inventory.Category SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.CategoryClosure
	(
	ParentId int NOT NULL,
	ChildId int NOT NULL,
	Depth int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.CategoryClosure ADD CONSTRAINT
	PK_CategoryClosure PRIMARY KEY CLUSTERED 
	(
	ParentId,
	ChildId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.CategoryClosure ADD CONSTRAINT
	FK_CategoryClosure_ParentCategory FOREIGN KEY
	(
	ParentId
	) REFERENCES Inventory.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.CategoryClosure ADD CONSTRAINT
	FK_CategoryClosure_ChildCategory FOREIGN KEY
	(
	ChildId
	) REFERENCES Inventory.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.CategoryClosure SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
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
WHERE lo.ChildId IS NULL
GO


COMMIT

