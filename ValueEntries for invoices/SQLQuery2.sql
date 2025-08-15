select [Lot No_], * from TRANSPACNAV21.dbo.[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where [Document No_] = 'SSH00035973'
and [Item No_] = '1812358-144'

--select * from TRANSPACNAV21.dbo.[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
--where [Lot No_] = '2232252 - L65679'
--and [Item No_] = '4664980-051'

select [item charge No_], * 
from TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] = 'L64071'
and [Item No_] = '3930057-051')
	and [item charge No_] <> ''
order by [Posting Date] desc

select [item charge No_], [Document No_], Sum([Valued Quantity]) as Qty, Sum([Cost Amount (Actual)]) as Amount
from TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] = 'L64071'
and [Item No_] = '3930057-051')
	and [item charge No_] <> ''
group by [item charge No_], [Document No_]
having Sum([Valued Quantity]) > 0
order by [item charge No_]