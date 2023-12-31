/*
   Wednesday, 26 August 20157:16:12 PM
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
CREATE TABLE Inventory.GlcSetting
	(
	SettingId int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NOT NULL,
	FillCharacter char(1) NOT NULL,
	Length int NOT NULL,
	Format int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.GlcSetting ADD CONSTRAINT
	PK_GlcSetting PRIMARY KEY CLUSTERED 
	(
	SettingId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.GlcSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
