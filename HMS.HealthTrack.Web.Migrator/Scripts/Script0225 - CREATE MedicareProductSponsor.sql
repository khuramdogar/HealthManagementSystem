/*
   Friday, April 1, 20163:53:52 PM
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
CREATE TABLE Inventory.MedicareProductSponsor
	(
	Code varchar(2) NOT NULL,
	company_ID int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.MedicareProductSponsor ADD CONSTRAINT
	PK_MedicareProductSponsor PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.MedicareProductSponsor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
