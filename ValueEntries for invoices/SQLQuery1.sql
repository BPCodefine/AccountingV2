select 
	sh.No_ as InvoiceNo,
	sl.[Line no_],
	sl.No_ as ArticleNo,
	sl.Quantity,
	--sh.[Sell-to Customer No_] as SellToID,
	sh.[Sell-to Customer Name] as  SellToName,
	--sh.[Bill-to Customer No_] as BillToID,
	sh.[Bill-to Name] as BillToName,
	sh.[Posting Date] as InvDate,
	sl.Description as Article,
	sl.Quantity,
	CASE WHEN sh.[Currency Code] = '' THEN 'EUR' ELSE sh.[Currency Code] END as Cur,
	sl.Amount,
	--(select Sum(sl.Amount) from [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl where sh.No_ = sl.[Document No_]) as InvAmount,
	--sh.[Invoice Discount Value] as Discount,
	--sl.[Line Amount],
	--sl.[Shortcut Dimension 2 Code] as ArticleType,
	sl.[Item Category Code],
	sl.[Shipment No_],
	sl.[Order No_],
	sl.[Gen_ Prod_ Posting Group]
from 
	[TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] as sh
	inner join [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl on sh.No_ = sl.[Document No_]
where 
	sl.Type = 2
	and sh.[Posting Date] > '2024-01-01'
	and sl.[Gen_ Prod_ Posting Group] in ('GOODS', 'PRODUCT', 'RAWMAT')
	--and sh.No_ = 'CDF-CH-24-0055'
order by 
	sh.No_,
	sl.[Line No_]