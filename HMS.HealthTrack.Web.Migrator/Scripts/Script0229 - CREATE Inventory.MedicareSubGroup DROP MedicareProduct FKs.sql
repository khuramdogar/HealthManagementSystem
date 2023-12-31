/*
   Monday, April 4, 201610:44:38 AM
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
ALTER TABLE Inventory.MedicareGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.MedicareSubGroup
	(
	MedicareSubGroupId int NOT NULL,
	MedicareGroupId int NOT NULL,
	Name varchar(200) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.MedicareSubGroup ADD CONSTRAINT
	PK_MedicareSubGroup PRIMARY KEY CLUSTERED 
	(
	MedicareSubGroupId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.MedicareSubGroup ADD CONSTRAINT
	FK_MedicareSubGroup_MedicareGroup FOREIGN KEY
	(
	MedicareGroupId
	) REFERENCES Inventory.MedicareGroup
	(
	MedicareGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.MedicareSubGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
