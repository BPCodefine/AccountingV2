with InvLines AS (
select 
	sh.No_ as INo,
	sl.[Line no_] as LNo
from 
	[TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] as sh
	inner join [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl on sh.No_ = sl.[Document No_]
where 
	sl.Type = 2
	and sh.[Posting Date] > '2024-01-01'
	and sl.[Gen_ Prod_ Posting Group] in ('GOODS', 'PRODUCT', 'RAWMAT')
	and [Shipment No_] = ''
	),
SSHNo as (
select 
	INo, 
	LNo,
	[Shipping No_] as ShipNo
from 
	InvLines 
	left outer join TRANSPACNAV21.dbo.[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972] sha on InvLines.INo = sha.[Posting No_])

select
	INo, 
	LNo,
	ShipNo,
	[Lot No_], ile.* 
from 
	SSHNo
	left outer join TRANSPACNAV21.dbo.[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile on 
		SSHNo.ShipNo = ile.[Document No_]
		and SSHNo.LNo = ile.[Document Line No_]
where
	SSHNo.ShipNo is not null
	and [Item No_] not like 'FREIGHT%'
	and [Lot No_] <> '1'
order by 
	INo, LNo