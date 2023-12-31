/*
   Friday, 17 April 20154:13:32 PM
   User: 
   Server: devdaniel
   Database: HMS_Net_v2_Inventory
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
EXECUTE sp_rename N'Inventory.Stock.RecievedOn', N'Tmp_ReceivedOn', 'COLUMN' 
GO
EXECUTE sp_rename N'Inventory.Stock.Tmp_ReceivedOn', N'ReceivedOn', 'COLUMN' 
GO
ALTER TABLE Inventory.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
