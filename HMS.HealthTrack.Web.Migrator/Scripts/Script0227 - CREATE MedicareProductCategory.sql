/*
   Monday, April 4, 20169:43:05 AM
   User: 
   Server: DEVDANIEL\SQLSERVER2014
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
CREATE TABLE Inventory.MedicareCategory
	(
	MedicareCategoryId int NOT NULL,
	Name nvarchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.MedicareCategory ADD CONSTRAINT
	PK_MedicareCategory PRIMARY KEY CLUSTERED 
	(
	MedicareCategoryId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.MedicareCategory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
