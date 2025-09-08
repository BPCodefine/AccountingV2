SELECT 
    sh.No_ AS INo,
    sh.[Posting Date] AS InvDate,
    sl.[Line no_] AS InvLineNo,
    sl.No_ AS ArticleNo,
	sl.Quantity as InvQuantity,
    COALESCE(sl.[Shipment No_], sha.[Shipping No_], '') AS ShipNo,
    COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) AS LotNo,
	ve.[Document Type],
	ve.[Item Charge No_],  
	ve.[Valued Quantity] AS Quantity,
	ve.[Cost Amount (Actual)] AS Cost,
	ve.[Cost per Unit] AS CostPerUnit,
	ve.*
FROM 
    [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] sh
    INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl 
        ON sh.No_ = sl.[Document No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972] sha 
        ON sh.No_ = sha.[Posting No_]
	LEFT JOIN TRANSPACNAV21.dbo.[CDF$Sales Shipment Header$437dbf0e-84ff-417a-965d-ed2bb9650972] ssh 
		ON ssh.No_ = sl.[Shipment No_]
	INNER JOIN TRANSPACNAV21.dbo.[CDF$Sales Shipment Line$437dbf0e-84ff-417a-965d-ed2bb9650972] ssline 
		ON ssh.No_ = ssline.[Document No_]
	LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile0
		ON sl.[Shipment No_] = ile0.[Document No_]
		AND sl.[No_] = ile0.[Item No_]
		AND ssline.[Line No_] = ile0.[Document Line No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile1 
        ON sha.[Shipping No_] = ile1.[Document No_]
        AND sl.[Line no_] = ile1.[Document Line No_]
    LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile2 
        ON ile2.[Document No_] = sh.No_
        AND ile2.[Document Line No_] = sl.[Line no_]
        AND ile2.[Item No_] = sl.No_
	INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ileWLotNo  
		ON ileWLotNo.[Lot No_] LIKE COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) + '%'
	INNER JOIN [TRANSPACNAV21].[dbo].[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve 
		ON ve.[Item Ledger Entry No_] = ileWLotNo.[Entry No_]
		AND ve.[Item No_] = sl.No_
		AND ve.[Posting Date] <= sh.[Posting Date]
WHERE 
	sh.No_ = 'CDF-DE-24-0212' AND
    sl.Type = 2
    AND sh.[Posting Date] > '2024-01-01'
    AND sl.[Gen_ Prod_ Posting Group] IN ('GOODS', 'PRODUCT', 'RAWMAT')
	AND COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) <> '1'
	AND ve.[Item Charge No_] <> 'FXADJ'
	AND ve.[Item Charge No_] <> 'CONTRACTED WORK'
	--AND ve.[Item Ledger Entry Type] = 0
	AND (ve.[Document Type] = 6 OR (ve.[Document Type] = 14 AND ve.[Source Code] = 'ASSEMBLY'))
	AND ve.[Cost Amount (Actual)] <> 0.0
ORDER BY 
    sh.[No_],
	sl.[Line No_],
	ve.[Item Charge No_]