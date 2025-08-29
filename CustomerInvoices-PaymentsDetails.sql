WITH DetailedSums AS (
    SELECT 
        [Cust_ Ledger Entry No_] AS EntryNo,
        SUM(CASE WHEN [Ledger Entry Amount] = 1 THEN Amount ELSE 0 END) AS OrigAmount,
        SUM(Amount) AS RemainingAmount
    FROM 
        transpacnav21.dbo.[cdf$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
    GROUP BY 
        [Cust_ Ledger Entry No_]
)

SELECT
    cust.[Name] AS CustomerName,
    cle.[Document Type] AS DocType,
    cle.[Document No_] AS DocNo,
    cle.[Posting Date] AS PostingDate,
    cle.[Description],
    CAST(cle.[Due Date] AS DATE) AS DueDate,
    CASE cle.[Currency Code]
        WHEN '' THEN '{DefCur}'
        ELSE cle.[Currency Code]
    END AS Cur,
    ds.OrigAmount,
    ds.RemainingAmount,
    dcle.[Document No_] AS AppliedDocNo,
    dcle.Amount AS AppliedAmount
FROM
    transpacnav21.dbo.[cdf$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust
INNER JOIN 
    transpacnav21.dbo.[cdf$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle 
    ON cust.[No_] = cle.[Customer No_]
LEFT JOIN 
    transpacnav21.dbo.[cdf$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] dcle 
    ON cle.[Entry No_] = dcle.[Cust_ Ledger Entry No_]
    AND dcle.[Entry Type] = 2 -- Application
    AND dcle.Unapplied = 0
LEFT JOIN 
    DetailedSums ds ON cle.[Entry No_] = ds.EntryNo
WHERE
    cle.[Document Type] IN (0, 2, 3, 6)
ORDER BY
    cle.[Document No_]