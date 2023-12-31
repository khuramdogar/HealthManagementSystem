/* Increase the size of the stock deduction reason description */
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
CREATE TABLE Inventory.Tmp_StockDeductionReason
	(
	StockDeductionReasonId int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NOT NULL,
	Description varchar(1000) NULL,
	Deleted bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockDeductionReason SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_StockDeductionReason ON
GO
IF EXISTS(SELECT * FROM Inventory.StockDeductionReason)
	 EXEC('INSERT INTO Inventory.Tmp_StockDeductionReason (StockDeductionReasonId, Name, Description, Deleted)
		SELECT StockDeductionReasonId, Name, Description, Deleted FROM Inventory.StockDeductionReason WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_StockDeductionReason OFF
GO
ALTER TABLE Inventory.StockDeduction
	DROP CONSTRAINT FK_StockDeduction_StockDeductionReason
GO
DROP TABLE Inventory.StockDeductionReason
GO
EXECUTE sp_rename N'Inventory.Tmp_StockDeductionReason', N'StockDeductionReason', 'OBJECT' 
GO
ALTER TABLE Inventory.StockDeductionReason ADD CONSTRAINT
	PK_StockDeductionReason PRIMARY KEY CLUSTERED 
	(
	StockDeductionReasonId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockDeduction ADD CONSTRAINT
	FK_StockDeduction_StockDeductionReason FOREIGN KEY
	(
	Reason
	) REFERENCES Inventory.StockDeductionReason
	(
	StockDeductionReasonId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockDeduction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

/* Insert the reasons */

BEGIN TRANSACTION
Insert into Inventory.StockDeductionReason (Name,Description,deleted)
values 
('Loss of stock','Stock that cannot be located',0),
('Theft of stock','Stock that is known to have been stolen',0),
('Stock unserviceable due to damage','Stock that has become damaged whilst being held within a warehouse, or in transit between locations',0),
('Stock expiry','Stock that is either within one month of the expiry date or if it has already reached the expiry date recorded on the item or its packaging as determined by the manufacturer',0),
('Obsolescence of stock','Items can become obsolete for various reasons such as issues with supply, suppliers or distributors',0),
('Stock is redundant','In cases when stock will no longer be used, this is simply a discontinuation of use (redundancy) and not stock obsolescence',0)
COMMIT
