BEGIN TRANSACTION

CREATE TABLE Inventory.ScanCode
	(
	ScanCodeId int IDENTITY(1,1) NOT NULL,
	Value varchar(50) NOT NULL,
	ProductId int NOT NULL
CONSTRAINT [PK_Inventory_ScanCode] PRIMARY KEY CLUSTERED 
(
	ScanCodeId ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 

ALTER TABLE Inventory.ScanCode ADD CONSTRAINT
	FK_ScanCode_Product FOREIGN KEY
	(
	ProductId
	) REFERENCES Inventory.Product
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
insert into inventory.ScanCode (ProductId,Value)
select productid,UPN from Inventory.Product where UPN is not null

select * from Inventory.ScanCode
alter table inventory.product
drop column UPN
select * from inventory.product

commit
