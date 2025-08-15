With InvLines as (
SELECT 
    sh.No_ AS INo,
	sh.[Posting Date] as InvDate,
    sl.[Line no_] AS LNo,
    sl.No_ AS ArticleNo,
    COALESCE(sha.[Shipping No_], '') AS ShipNo,
    COALESCE(ile1.[Lot No_], ile2.[Lot No_]) AS LotNo
FROM 
    [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] sh
    INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl 
        ON sh.No_ = sl.[Document No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972] sha 
        ON sh.No_ = sha.[Posting No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile1 
        ON sha.[Shipping No_] = ile1.[Document No_]
        AND sl.[Line no_] = ile1.[Document Line No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile2 
        ON ile2.[Document No_] = sh.No_
        AND ile2.[Document Line No_] = sl.[Line no_]
        AND ile2.[Item No_] = sl.No_
WHERE 
    sl.Type = 2
    AND sh.[Posting Date] > '2024-01-01'
    AND sl.[Gen_ Prod_ Posting Group] IN ('GOODS', 'PRODUCT', 'RAWMAT')
    AND sl.[Shipment No_] = ''
	--and COALESCE(ile1.[Lot No_], ile2.[Lot No_]) is null
--ORDER BY 
--    sh.No_, 
--	sl.[Line no_]
)
select 
	InvLines.*,
	[Item Charge No_], 
	[Cost per Unit], 
	* 
from 
	InvLines
	left outer join TRANSPACNAV21.dbo.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve on 
		ve.[Item No_] = InvLines.ArticleNo
		and ve.[Posting Date] <= InvLines.InvDate
where
	[Item Ledger Entry No_] in (select [Entry No_] from [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] where [Lot No_] like InvLines.LotNo + '%' )
	--and [item charge No_] <> '' 
	and [item charge No_] <> 'FXADJ'
order by [Posting Date] desc