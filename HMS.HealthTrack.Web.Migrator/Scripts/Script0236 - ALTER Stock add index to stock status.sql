/****** Object:  Index [IX_Inventory_Stock_StockStatus]    Script Date: 21-Apr-16 9:54:12 AM ******/
CREATE NONCLUSTERED INDEX [IX_Inventory_Stock_StockStatus] ON [Inventory].[Stock]
(
	[StockStatus] ASC
)
INCLUDE ( 	[Quantity]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


