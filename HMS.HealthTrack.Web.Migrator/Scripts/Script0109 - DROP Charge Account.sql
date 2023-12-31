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
ALTER TABLE Inventory.[Order]
	DROP CONSTRAINT FK_Order_ChargeAccount
GO
ALTER TABLE Inventory.ChargeAccount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


DELETE FROM Inventory.ChargeAccount
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Inventory].[FK_ChargeAccount_ChargeAccount]') AND parent_object_id = OBJECT_ID(N'[Inventory].[ChargeAccount]'))
ALTER TABLE [Inventory].[ChargeAccount] DROP CONSTRAINT [FK_ChargeAccount_ChargeAccount]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ChargeAccount_CreatedOn]') AND type = 'D')
BEGIN
ALTER TABLE [Inventory].[ChargeAccount] DROP CONSTRAINT [DF_ChargeAccount_CreatedOn]
END

GO

/****** Object:  Table [Inventory].[ChargeAccount]    Script Date: 08/28/2015 15:32:39 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Inventory].[ChargeAccount]') AND type in (N'U'))
DROP TABLE [Inventory].[ChargeAccount]
GO


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
ALTER TABLE Inventory.[Order]
	DROP COLUMN ChargeAccountId
GO
ALTER TABLE Inventory.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

