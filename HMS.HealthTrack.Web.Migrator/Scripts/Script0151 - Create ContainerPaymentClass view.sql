CREATE view [Inventory].[ContainerPaymentClass] as
select c.container_ID, lc.ItemValue 
from MR_Container c
join booking b on b.booking_ID = c.booking_ID
join List_Core lc on lc.ItemID = b.PaymentClass
where b.PaymentClass is null or (ListGroup = 'bookingform' and ListName = 'paymentclass')
GO


