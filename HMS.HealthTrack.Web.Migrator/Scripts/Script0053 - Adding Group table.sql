/*
   Tuesday, 14 April 201512:50:44 PM
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
CREATE TABLE Inventory.[MedicareGroup]
	(
	MedicareGroupId int NOT NULL IDENTITY (1, 1),
	ParentId int NULL,
	Name varchar(200) NOT NULL,
	CreatedBy varchar(50) NULL,
	CreatedOn datetime NOT NULL,
	LastModifiedBy varchar(50) NULL,
	LastModifiedOn datetime NULL,
	DeletedBy varchar(50) NULL,
	DeletedOn datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.[MedicareGroup] ADD CONSTRAINT
	DF_MedicareGroup_CreatedOn DEFAULT getdate() FOR CreatedOn
GO
ALTER TABLE Inventory.[MedicareGroup] ADD CONSTRAINT
	PK_MedicareGroup PRIMARY KEY CLUSTERED 
	(
	MedicareGroupId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.[MedicareGroup] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
