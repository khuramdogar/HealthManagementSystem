/*
   Wednesday, 26 August 20151:21:08 PM
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
ALTER TABLE Inventory.ChargeAccount
	DROP CONSTRAINT FK_ChargeAccount_CostCenter
GO
ALTER TABLE Inventory.CostCenter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'Inventory.ChargeAccount.AccountCode', N'Tmp_GLC', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.ChargeAccount.Tmp_GLC', N'GLC', 'COLUMN' 
GO
ALTER TABLE Inventory.ChargeAccount
	DROP COLUMN EntityCode, CostCenterId
GO
ALTER TABLE Inventory.ChargeAccount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
