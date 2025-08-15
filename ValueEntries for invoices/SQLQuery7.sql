WITH InvLines AS (
    SELECT 
        sh.No_ AS INo,
        sh.[Posting Date] AS InvDate,
        sl.[Line no_] AS LNo,
        sl.No_ AS ArticleNo,
		sl.Quantity,
        COALESCE(sl.[Shipment No_], sha.[Shipping No_], '') AS ShipNo,
        COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) AS LotNo
    FROM 
        [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] sh
        INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl 
            ON sh.No_ = sl.[Document No_]
        LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972] sha 
            ON sh.No_ = sha.[Posting No_]
		LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile0
			ON sl.[Shipment No_] = ile0.[Document No_]
			AND sl.[No_] = ile0.[Item No_]
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
		AND COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) <> '1'
)

SELECT 
    InvLines.INo,
	InvLines.LNo,
	InvLines.LotNo,
	InvLines.Quantity,
    ve.[Item Charge No_], 
	ve.*
FROM 
    InvLines
    INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile 
        ON ile.[Lot No_] LIKE InvLines.LotNo + '%'
    INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve 
        ON ve.[Item Ledger Entry No_] = ile.[Entry No_]
        AND ve.[Item No_] = InvLines.ArticleNo
        AND ve.[Posting Date] <= InvLines.InvDate
		AND ve.[Item Charge No_] <> 'FXADJ'
		AND ve.[Item Charge No_] <> 'CONTRACTED WORK'
		--AND ve.[Item Ledger Entry Type] = 0
		--AND ve.[Cost Amount (Actual)] <> 0.0
		and InvLines.INo = 'CDF-CH-24-0055'
		and InvLines.LNo = '10000'
ORDER BY 
    InvLines.INo, 
	InvLines.LNo,
	ve.[Item Charge No_]