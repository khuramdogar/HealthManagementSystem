/*
   Friday, 22 June 201811:48:09 AM
   User: sa
   Server: LANNISTER\SQLSERVER2016
   Database: HealthTrack_Web_DevNeil
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
CREATE TABLE Inventory.ConsumptionReversal
	(
	ConsumptionReversalId bigint NOT NULL IDENTITY (1, 1),
	ConsumptionReference int NOT NULL,
	SubmittedBy varchar(200) NULL,
	SubmittedOn datetime NULL,
	Processed bit NOT NULL,
	ProcessedOn datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.ConsumptionReversal ADD CONSTRAINT
	DF_Table_1_Processed DEFAULT 0 FOR Processed
GO
ALTER TABLE Inventory.ConsumptionReversal ADD CONSTRAINT
	PK_Table_1_1 PRIMARY KEY CLUSTERED 
	(
	ConsumptionReversalId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_ConsumptionReversal_ConRef ON Inventory.ConsumptionReversal
	(
	ConsumptionReference
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.ConsumptionReversal SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
