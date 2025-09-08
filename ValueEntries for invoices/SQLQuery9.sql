SELECT
    sh.No_ AS INo,
    sh.[Posting Date] AS InvDate,
    sl.[Line no_] AS InvLineNo,
    sl.No_ AS ArticleNo,
    sl.Quantity AS InvQuantity,
    COALESCE(sl.[Shipment No_], sha.[Shipping No_], '') AS ShipNo,
    lot.LotNoBase AS LotNo,
    ve_top.[Document Type],
    ve_top.[Item Charge No_],
    ve_top.[Valued Quantity] AS Quantity,
    COALESCE(sum_ve.TotalCostForILE, 0) AS TotalCostForILE  -- <<< added
    --ve_top.*
FROM [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] sh
JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl 
  ON sh.No_ = sl.[Document No_]
LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Sales Header Archive$437dbf0e-84ff-417a-965d-ed2bb9650972] sha 
  ON sh.No_ = sha.[Posting No_]
--LEFT JOIN TRANSPACNAV21.dbo.[CDF$Sales Shipment Header$437dbf0e-84ff-417a-965d-ed2bb9650972] ssh 
--  ON ssh.No_ = sl.[Shipment No_]
JOIN TRANSPACNAV21.dbo.[CDF$Sales Shipment Line$437dbf0e-84ff-417a-965d-ed2bb9650972] ssline 
  ON ssline.[Document No_] = sl.[Shipment No_]
 AND ssline.[Line No_]     = sl.[Shipment Line No_]
LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile0
  ON ile0.[Document No_]     = sl.[Shipment No_]
 AND ile0.[Item No_]         = sl.[No_]
 AND ile0.[Document Line No_] = sl.[Shipment Line No_] 
LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile1 
  ON sha.[Shipping No_] = ile1.[Document No_]
 AND sl.[Line no_]      = ile1.[Document Line No_]
LEFT JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile2 
  ON ile2.[Document No_]     = sh.No_
 AND ile2.[Document Line No_] = sl.[Line no_]
 AND ile2.[Item No_]         = sl.No_

CROSS APPLY (
  SELECT COALESCE(ile0.[Lot No_], ile1.[Lot No_], ile2.[Lot No_]) AS LotNoBase
) AS lot

CROSS APPLY (
  SELECT TOP (1) ve.*
  FROM [TRANSPACNAV21].[dbo].[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve
  JOIN [TRANSPACNAV21].[dbo].[CDF$Item Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ile
    ON ile.[Entry No_] = ve.[Item Ledger Entry No_]
  WHERE ile.[Lot No_] LIKE lot.LotNoBase + '%'
    AND ve.[Item No_] = sl.No_
    AND ve.[Posting Date] <= sh.[Posting Date]
    AND ve.[Item Charge No_] NOT IN ('FXADJ','CONTRACTED WORK')
    AND ve.[Document Type] = 14 
	AND ve.[Source Code] = 'ASSEMBLY'
    AND ve.[Cost Amount (Actual)] <> 0.0
  ORDER BY ve.[Posting Date] DESC, ve.[Entry No_] DESC
) AS ve_top

OUTER APPLY (
  SELECT SUM(ve2.[Cost Amount (Actual)]) AS TotalCostForILE
  FROM [TRANSPACNAV21].[dbo].[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve2
  WHERE ve2.[Item Ledger Entry No_] = ve_top.[Item Ledger Entry No_]
) AS sum_ve

WHERE
  sh.No_ = 'CDF-BE-24-0018' AND 
  sl.Type = 2
  AND sh.[Posting Date] > '2024-01-01'
  AND sl.[Gen_ Prod_ Posting Group] IN ('PRODUCT','RAWMAT')
  AND lot.LotNoBase <> '1'
ORDER BY sh.[No_], sl.[Line No_], ve_top.[Item Charge No_];
