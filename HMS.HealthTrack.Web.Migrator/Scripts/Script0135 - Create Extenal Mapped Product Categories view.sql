create VIEW [Inventory].[ExternalMappedCategories]
AS

select im.inv_ID as ExternalProductId, ISNULL(c.CategoryName, 'Unidentified') as InternalCategory from Inventory_Master im
left outer join 
(SELECT        m.ExternalProductId, c.CategoryName
FROM            Inventory.ExternalProductMapping AS m INNER JOIN
                         Inventory.ProductCategory AS pc ON pc.ProductId = m.InventoryProductId INNER JOIN
                         Inventory.Category AS c ON c.CategoryId = pc.CategoryId) c on c.ExternalProductId = im.Inv_ID
GO


