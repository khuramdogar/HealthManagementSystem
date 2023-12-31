/*
   Wednesday, 9 November 20161:40:33 PM
   User: sa
   Server: bolton
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
ALTER TABLE Inventory.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.OrderChannel
	(
	OrderChannelId int NOT NULL IDENTITY (1, 1),
	Name varchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.OrderChannel ADD CONSTRAINT
	PK_OrderChannel PRIMARY KEY CLUSTERED 
	(
	OrderChannelId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderChannel SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.OrderChannelSupplier
	(
	OrderChannelId int NOT NULL,
	SupplierId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.OrderChannelSupplier ADD CONSTRAINT
	PK_OrderChannelSupplier PRIMARY KEY CLUSTERED 
	(
	OrderChannelId,
	SupplierId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderChannelSupplier ADD CONSTRAINT
	FK_OrderChannelSupplier_OrderChannel FOREIGN KEY
	(
	OrderChannelId
	) REFERENCES Inventory.OrderChannel
	(
	OrderChannelId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelSupplier ADD CONSTRAINT
	FK_OrderChannelSupplier_Supplier FOREIGN KEY
	(
	SupplierId
	) REFERENCES Inventory.Supplier
	(
	company_ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelSupplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Location SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.OrderChannelLocation
	(
	OrderChannelId int NOT NULL,
	LocationId int NOT NULL,
	Reference varchar(100) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.OrderChannelLocation ADD CONSTRAINT
	PK_OrderChannelLocation PRIMARY KEY CLUSTERED 
	(
	OrderChannelId,
	LocationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderChannelLocation ADD CONSTRAINT
	FK_OrderChannelLocation_Location FOREIGN KEY
	(
	LocationId
	) REFERENCES Inventory.Location
	(
	LocationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelLocation ADD CONSTRAINT
	FK_OrderChannelLocation_OrderChannel FOREIGN KEY
	(
	OrderChannelId
	) REFERENCES Inventory.OrderChannel
	(
	OrderChannelId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelLocation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.OrderChannelProduct
	(
	OrderChannelProductId int NOT NULL IDENTITY (1, 1),
	OrderChannelId int NOT NULL,
	ProductId int NOT NULL,
	Reference varchar(100) NOT NULL,
	AutomaticOrder bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.OrderChannelProduct ADD CONSTRAINT
	DF_OrderChannelProduct_AutomaticOrder DEFAULT 0 FOR AutomaticOrder
GO
ALTER TABLE Inventory.OrderChannelProduct ADD CONSTRAINT
	PK_OrderChannelProduct PRIMARY KEY CLUSTERED 
	(
	OrderChannelProductId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.OrderChannelProduct ADD CONSTRAINT
	FK_OrderChannelProduct_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelProduct ADD CONSTRAINT
	FK_OrderChannelProduct_OrderChannel FOREIGN KEY
	(
	OrderChannelId
	) REFERENCES Inventory.OrderChannel
	(
	OrderChannelId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.OrderChannelProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Submission
	(
	SubmissionId int NOT NULL IDENTITY (1, 1),
	OrderId int NOT NULL,
	OrderChannelId int NOT NULL,
	SubmissionReference varchar(100) NULL,
	SubmissionStatus int NOT NULL,
	TriggeredBy varchar(100) NULL,
	TimeStamp datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Submission ADD CONSTRAINT
	DF_Submission_SubmissionStatus DEFAULT 0 FOR SubmissionStatus
GO
ALTER TABLE Inventory.Submission ADD CONSTRAINT
	DF_Submission_TimeStamp DEFAULT GETDATE() FOR TimeStamp
GO
ALTER TABLE Inventory.Submission ADD CONSTRAINT
	PK_Submission PRIMARY KEY CLUSTERED 
	(
	SubmissionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE Inventory.Submission ADD CONSTRAINT
	FK_Submission_OrderChannel FOREIGN KEY
	(
	OrderChannelId
	) REFERENCES Inventory.OrderChannel
	(
	OrderChannelId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Submission ADD CONSTRAINT
	FK_Submission_Order FOREIGN KEY
	(
	OrderId
	) REFERENCES Inventory.[Order]
	(
	InventoryOrderId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.Submission SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
