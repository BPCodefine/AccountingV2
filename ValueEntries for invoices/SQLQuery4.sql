-- No Sales Shipment No
select [Shipping No_] from TRANSPACNAV21.dbo.[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972]
where [Posting No_] = 'CDF-CH-24-0055'

select 
[Shipping No_] from TRANSPACNAV21.dbo.[CDF$Sales Header$437dbf0e-84ff-417a-965d-ed2bb9650972]
where [Posting No_] = 'CDF-CH-24-0055'

select [Lot No_], * from TRANSPACNAV21.dbo.[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where [Document No_] = 'CDF-CH-24-0055' 
and [Document Line No_] = '10000'
--and [Item No_] = '3484478-XXX'

select [Item Charge No_], [Cost per Unit], * 
from TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] like '2236176 - L65875%' )
	--and [item charge No_] <> '' 
	and [item charge No_] <> 'FXADJ'
	and [Item No_] = '4657289-137'
	and [Posting Date] <= '2024-03-06' -- Invoice date 
	and [Item Ledger Entry Type] = 0
	and [Cost Amount (Actual)] <> 0.0
order by [Posting Date] desc

select [Item Charge No_], Sum([Cost per Unit]) 
from TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] like '2236176 - L65875%' )
	--and [item charge No_] <> '' 
	and [item charge No_] <> 'FXADJ'
	and [Item No_] = '4657289-137'
	and [Posting Date] <= '2024-03-06' -- Invoice date
	and [Item Ledger Entry Type] = 0
	and [Cost Amount (Actual)] <> 0.0
group by [Item Charge No_]

select [Item Charge No_], [Document No_], Sum([Valued Quantity]) as Qty, Sum([Cost Amount (Actual)]) as Amount
from TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] = '2247287 C12/30' )
	and [item charge No_] <> ''
	and [Item No_] = '308654-144'
group by [item charge No_], [Document No_]
having Sum([Valued Quantity]) > 0
order by [item charge No_]