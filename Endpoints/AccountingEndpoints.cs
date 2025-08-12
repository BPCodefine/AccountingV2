using AccountingV2.Entities;
using Dapper;
using System.Data;
using System.Text;

namespace AccountingV2.Endpoints
{
    struct DateInterval
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class LedgerQuery
    {
        public DateTime ToDate { get; set; }
        public string? ExtraAccList { get; set; }
    }
    public class CustInvQuery
    {
        public string Company { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class VendorInvQuery
    {
        public string Company { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? OnlyOpen { get; set; } = false;
    }
    public class HulkenQuery
    {
        public required string FromDate { get; set; }
        public required string ToDate { get; set; }
        public bool showUSSales { get; set; }
        public required string ConvertTo { get; set; }
    }
    public static class AccountingEndpoints
    {
        public static string GetDefaultCurrency(string company, string comp, IDbConnection c)
        {
            string sql = $"select [LCY Code] from {comp}.[{company}$General Ledger Setup$437dbf0e-84ff-417a-965d-ed2bb9650972]";
            string? currency = c.ExecuteScalar<string>(sql);

            if (currency == null)
                throw new InvalidOperationException($"Default currency for {company} could not be retrieved.");

            return currency;
        }

        public static void MapAccountingEndpoints(this IEndpointRouteBuilder app)
        {
            const string sqlPurchInvoicesAndExpenses = @"
with InvCTE as(
select
	pih.[No_] as InvoiceNo,
    pih.[Buy-from Vendor No_] as BuyFromID,
    pih.[Buy-from Vendor Name] as BuyFromName,
    pih.[Pay-to Vendor No_] as PayToID,
    pih.[Pay-to Name] as PayToName,
    pih.[Your Reference] as YourRefOrderID,
    pih.[Posting Date] as PostingDate,
    pil.[Line No_] as InvLineNo,
    pil.[No_] as ArticleNo,
    pil.Description as ArticleName,
    pil.Quantity as InvoicedQty
from
    {DBName}.[CDF$Purch_ Inv_ Header$437dbf0e-84ff-417a-965d-ed2bb9650972] pih
	inner join {DBName}.[CDF$Purch_ Inv_ Line$437dbf0e-84ff-417a-965d-ed2bb9650972] pil on pih.[No_] = pil.[Document No_]
where
    pil.Type = 2
),
-- Step 1: Aggregate charges and pivot them
ChargeSummary as (
    select
        subquery.[Document No_] as InvoiceNo,
        subquery.[Document Line No_] as DocLineNo,
        ve.[Item No_] as ItemNo,
        ve.[Valued Quantity] as ValuedQuantity,
        ve.[Item Charge No_] as ChargeName,
        SUM(ve.[Cost Amount (Actual)]) as TotalCostAmount
    from 
        {DBName}.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ve
    inner join 
        {DBName}.[CDF$Value Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] subquery 
        on ve.[Item Ledger Entry No_] = subquery.[Item Ledger Entry No_]
    where 
        subquery.[Source Code] <> 'INVTADJMT'
        and ve.[Cost Amount (Actual)] <> 0
    group by 
        subquery.[Document No_], 
        subquery.[Document Line No_], 
        ve.[Item No_],
        ve.[Valued Quantity],
        ve.[Item Charge No_]
),
ChargePivot as (
    select
        InvoiceNo,
        DocLineNo,
        ItemNo,
        ValuedQuantity,
        ISNULL([ ], 0) as PUACost,
        ISNULL(SEAFREIGHT, 0) as SeafreightCost,
        ISNULL(DUTY, 0) as DutyCost,
        ISNULL(STORAGE, 0) as StorageCost,
        ISNULL(OUTFREIGHT, 0) as OutfreightCost,
        ISNULL(INLANDFREIGHT, 0) as InlandFreightCost,
        ISNULL(SMALLCOST, 0) as SmallCost,
        ISNULL(UVYARNCOST, 0) as UVYarnCost,
        ISNULL(QUALDISCOUNT, 0) as QualDiscountCost
    from
        ChargeSummary
    pivot
    (
        sum(TotalCostAmount)
        for ChargeName in (
            [ ], 
            SEAFREIGHT, 
            DUTY, 
            STORAGE, 
            OUTFREIGHT, 
            INLANDFREIGHT, 
            SMALLCOST, 
            UVYARNCOST, 
            QUALDISCOUNT
        )
    ) as p
)

-- Step 2: Combine invoice lines with charge data
select 
    InvCTE.*,
    ISNULL(cp.PUACost, 0) as PUACost,
    ISNULL(cp.SeafreightCost, 0) as SeafreightCost,
    ISNULL(cp.DutyCost, 0) as DutyCost,
    ISNULL(cp.StorageCost, 0) as StorageCost,
    ISNULL(cp.OutfreightCost, 0) as OutfreightCost,
    ISNULL(cp.InlandFreightCost, 0) as InlandFreightCost,
    ISNULL(cp.SmallCost, 0) as SmallCost,
    ISNULL(cp.UVYarnCost, 0) as UVYarnCost,
    ISNULL(cp.QualDiscountCost, 0) as QualDiscountCost
from
	InvCTE
	left join ChargePivot cp on 
		InvCTE.InvoiceNo = cp.InvoiceNo collate database_default
		and InvCTE.InvLineNo = cp.DocLineNo
		and InvCTE.ArticleNo = cp.ItemNo collate database_default
		and InvCTE.InvoicedQty = cp.ValuedQuantity
";
            const string OrderBy = "order by InvCTE.PostingDate, InvCTE.InvoiceNo, InvCTE.InvLineNo";

            app.MapGet("/api/PurchInvAndExpenses", async (IDbConnection c, DBAccess context) =>
            {
                try
                {
                    var lines = await c.QueryAsync<PIAndExpenses>(sqlPurchInvoicesAndExpenses.Replace("{DBName}", context.dynDBName));
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            }
            );

            app.MapGet("/api/PurchInvAndExpenses/betweenDates", async (IDbConnection c, DBAccess context, [AsParameters] DateInterval betweenDates) =>
            {
                try
                { 
                    StringBuilder sqlWithInvNo = new(sqlPurchInvoicesAndExpenses);
                    sqlWithInvNo.Replace("{DBName}", context.dynDBName);
                    sqlWithInvNo.AppendLine($"where InvCTE.PostingDate between '{betweenDates.FromDate}' and '{betweenDates.ToDate}'");
                    sqlWithInvNo.AppendLine(OrderBy);

                    var lines = await c.QueryAsync<PIAndExpenses>(sqlWithInvNo.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }   
            });

            app.MapGet("/api/BankAccountLedgerItems", async (IDbConnection c, DBAccess context, [AsParameters] LedgerQuery query) =>
            {
                StringBuilder sqlBankAccountLedgerEntries = new($@"
select
	[Entry No_] as EntryNo, 
	[Document Type] as DocumentType,
	[Document No_] as DocumentNo,
	[Bank Account No_] as BankAccountNo, 
	[Description],
	[Posting Date] as PostingDate, 
	CASE [Currency Code] 
		WHEN '' THEN 'EUR'
		ELSE [Currency Code]
	END as CurrencyCode,
	Amount, 
	[Remaining Amount] as RemainingAmount,
	[Amount (LCY)] as AmountLCY,
	[Open],
	[Debit Amount] as DebitAmount,
    [Credit Amount] as CreditAmount,
	Sum(Amount) OVER (PARTITION BY [Bank Account No_] ORDER BY [Posting Date], [Entry No_]) AS RunningTotalAmount,
	Sum([Amount (LCY)]) OVER (PARTITION BY [Bank Account No_] ORDER BY [Posting Date], [Entry No_]) AS RunningTotalAmountLCY
from
	{context.dynDBName}.[CDF$Bank Account Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
where
	[Bank Account No_] in ('00010588253', '00010043333', '00018000262', '00010058708', '11210EUR', '11210GBP', '11210SEK', '11210USD', '11220EUR', '11220GBP', '11220SEK', '11220USD' @MoreAccntList)
    AND [Posting Date] <= '{query.ToDate:yyyy-MM-dd}'
order by
    [Posting Date] Desc
");
                try
                {
                    sqlBankAccountLedgerEntries.Replace("@toDate", query.ToDate.ToString("yyyy/MM/dd"));
                    sqlBankAccountLedgerEntries.Replace("@MoreAccntList", (query.ExtraAccList?.Length ?? 0) == 0 ? "" : ", " + query.ExtraAccList);
                    var lines = await c.QueryAsync<BankAccountLedgerEntries>(sqlBankAccountLedgerEntries.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }   
            }
            );

            app.MapGet("/api/CustomerInvoices", async (IDbConnection c, DBAccess context, [AsParameters] CustInvQuery query) =>
            {
                string DefCur;
                try
                {
                    DefCur = GetDefaultCurrency(query.Company, context.dynDBName, c);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }

                StringBuilder sql = new($@"
select
    cle.[Document No_] as InvoiceNo,
    cle.[Customer No_] as CustomerNo,
    cust.[Name] as CustomerName,
    cast(cle.[Posting Date] as Date) as InvoiceDate,
    cle.[Description],
    cast(cle.[Due Date] as Date) as DueDate,
    (select SUM(Amount) from {context.dynDBName}.[{query.Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
     where [Entry Type] = 1
            and [Cust_ Ledger Entry No_] = cle.[Entry No_] 
            and [Posting Date] = cle.[Posting Date]) as Amount,
    case cle.[Currency Code]    
        when '' then '{DefCur}'
        else cle.[Currency Code]
    end As Cur,
    (select SUM([Amount (LCY)]) from {context.dynDBName}.[{query.Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972]
     where [Entry Type] = 1
        and [Cust_ Ledger Entry No_] = cle.[Entry No_]
        and [Posting Date] = cle.[Posting Date]) as AmountLCY,
    cast(ClosedBy.[Posting Date] as Date) as PaymentDate,
    ClosedBy.[Document No_] as PaymentDocNo
from 
    {context.dynDBName}.[{query.Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
    left join {context.dynDBName}.[{query.Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ClosedBy 
        on ClosedBy.[Entry No_] = cle.[Closed by Entry No_]
    inner join {context.dynDBName}.[{query.Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust on cle.[Customer No_] = cust.[No_]
where
    cle.[Posting Date] BETWEEN '{query.FromDate:yyyy-MM-dd}' AND '{query.ToDate:yyyy-MM-dd}'
    AND cle.[Document Type] = 2");

                try
                {
                    var lines = await c.QueryAsync<CustomerInvoices>(sql.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });

            app.MapGet("/api/LateCustomerInvoices", async (IDbConnection c, DBAccess context, string Company) =>
            {
                string DefCur;
                try
                {
                    DefCur = GetDefaultCurrency(Company, context.dynDBName, c);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }

                StringBuilder sql = new($@"
select
	cle.[Document No_] as InvoiceNo,
	cle.[Customer No_] as CustomerNo,
	cust.[Name] as CustomerName,
	cast(cle.[Posting Date] as Date) as InvoiceDate,
	cle.[Description],
	cast(cle.[Due Date] as Date) as DueDate,
	(select SUM(Amount) from {context.dynDBName}.[{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Entry Type] = 1 
						and [Cust_ Ledger Entry No_] = cle.[Entry No_] 
						and [Posting Date] = cle.[Posting Date]) as Amount,
	CASE cle.[Currency Code] 
	    WHEN '' THEN '{DefCur}'
		ELSE cle.[Currency Code]
	END As Cur,
	(select SUM([Amount (LCY)]) from {context.dynDBName}.[{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                            where [Entry Type] = 1
								and [Cust_ Ledger Entry No_] = cle.[Entry No_]
								and [Posting Date] = cle.[Posting Date]) as AmountLCY,
    DATEDIFF(DAY, cle.[Due Date], GETDATE()) as LateDays
from
	{context.dynDBName}.[{Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
	inner join {context.dynDBName}.[{Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust on cle.[Customer No_] = cust.[No_]
where 
	[Open] = 1
	and (cle.[Document Type] = 2 OR cle.[Document Type] = 3)");

                try
                {
                    var lines = await c.QueryAsync<CustomerInvoices>(sql.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });

            app.MapGet("/api/VendorInvoices", async (IDbConnection c, DBAccess context, [AsParameters] VendorInvQuery query) =>
            {
                string DefCur;
                try
                {
                    DefCur = GetDefaultCurrency(query.Company, context.dynDBName, c);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }

                StringBuilder sql = new($@"
select
	vle.[Document No_] as InvoiceNo,
	vle.[Vendor No_] as VendorNo,
	vendor.[Name] as VendorName,
	cast(vle.[Posting Date] as Date) as InvoiceDate,
	vle.[Description],
	cast(vle.[Due Date] as Date) as DueDate,
	vle.[External Document No_] as ExtInvNo,
	(select SUM(-1 * Amount) from {context.dynDBName}.[{query.Company}$Detailed Vendor Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Ledger Entry Amount] = 1 
						      and [Vendor Ledger Entry No_] = vle.[Entry No_] 
						      and [Posting Date] = vle.[Posting Date]) as Amount,
	CASE vle.[Currency Code] 
		WHEN '' THEN '{DefCur}' 
		ELSE vle.[Currency Code]
	END As Cur,
	(select SUM(-1 * [Amount (LCY)]) from {context.dynDBName}.[{query.Company}$Detailed Vendor Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] 
	                    where [Ledger Entry Amount] = 1 
						      and [Vendor Ledger Entry No_] = vle.[Entry No_] 
						      and [Posting Date] = vle.[Posting Date]) as AmountLCY,
	cast(ClosedBy.[Posting Date] as Date) as PaymentDate,
	ClosedBy.[Document No_] as PaymentDocNo
from 
	{context.dynDBName}.[{query.Company}$Vendor Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] vle
	left join {context.dynDBName}.[{query.Company}$Vendor Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] ClosedBy on ClosedBy.[Entry No_] = vle.[Closed by Entry No_]
	inner join {context.dynDBName}.[{query.Company}$Vendor$437dbf0e-84ff-417a-965d-ed2bb9650972] vendor on vle.[Vendor No_] = vendor.[No_]
where 
	(vle.[Document Type] = 2 OR vle.[Document Type] = 3)
    AND vle.[Posting Date] BETWEEN '{query.FromDate:yyyy-MM-dd}' AND '{query.ToDate:yyyy-MM-dd}'");

                try
                {
                    if (query.OnlyOpen.HasValue && query.OnlyOpen.Value)
                        sql.AppendLine("AND vle.[Open] = 1");

                    var lines = await c.QueryAsync<VendorInvoices>(sql.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });

            app.MapGet("/api/GetEnabledComps", async (IDbConnection c, DBAccess context, string UserName) =>
            {
                try
                {
                    string sql = $"select [AccessTo] from [SatV2].[Webpages].[WebUserRights] where [UserName] = '{UserName}'";
                    var lines = await c.QueryAsync<string>(sql);
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });

            app.MapGet("/api/CustAgedInvoices", async (IDbConnection c, DBAccess context, string Company) =>
            {
                string DefCur;
                try
                {
                    DefCur = GetDefaultCurrency(Company, context.dynDBName, c);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }

                StringBuilder sql = new($@"
WITH EntryDetails AS (
    SELECT
        cle.[Entry No_] AS EntryNo,
        cle.[Document No_] AS invoiceNo,
        cle.[Customer No_] AS customerNo,
        cust.[Name] AS customerName,
        CAST(cle.[Posting Date] AS DATE) AS invoiceDate,
        cle.[Description] AS descr,
        CAST(cle.[Due Date] AS DATE) AS dueDate,
        CASE cle.[Currency Code]
            WHEN '' THEN '{DefCur}'
            ELSE cle.[Currency Code]
        END AS cur,
        DATEDIFF(DAY, cle.[Due Date], GETDATE()) AS lateDays,
        (
            SELECT SUM(Amount)
            FROM {context.dynDBName}.[{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] dcle
            WHERE dcle.[Entry Type] = 1
              AND dcle.[Cust_ Ledger Entry No_] = cle.[Entry No_]
              AND dcle.[Posting Date] = cle.[Posting Date]
        ) AS amount,

        (
            SELECT SUM([Amount (LCY)])
            FROM {context.dynDBName}.[{Company}$Detailed Cust_ Ledg_ Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] dcle
            WHERE dcle.[Entry Type] = 1
              AND dcle.[Cust_ Ledger Entry No_] = cle.[Entry No_]
              AND dcle.[Posting Date] = cle.[Posting Date]
        ) AS amountLCY

    FROM
        {context.dynDBName}.[{Company}$Cust_ Ledger Entry$437dbf0e-84ff-417a-965d-ed2bb9650972] cle
        INNER JOIN {context.dynDBName}.[{Company}$Customer$437dbf0e-84ff-417a-965d-ed2bb9650972] cust
            ON cle.[Customer No_] = cust.[No_]
    WHERE
        cle.[Open] = 1
        AND cle.[Document Type] IN (2, 3)
)

SELECT
    InvoiceNo,
    CustomerNo,
    CustomerName,
    InvoiceDate,
    Descr,
    DueDate,
    Cur,
    LateDays,

    CASE WHEN lateDays < 30 THEN amount ELSE NULL END AS [acc30],
    CASE WHEN lateDays BETWEEN 30 AND 60 THEN amount ELSE NULL END AS [acc3060],
    CASE WHEN lateDays BETWEEN 61 AND 90 THEN amount ELSE NULL END AS [acc6090],
	CASE WHEN lateDays > 90 THEN amount ELSE NULL END AS [acc90],

	CASE WHEN lateDays < 30 THEN amountLCY ELSE NULL END AS [lcy30],
    CASE WHEN lateDays BETWEEN 30 AND 60 THEN amountLCY ELSE NULL END AS [lcy3060],
    CASE WHEN lateDays BETWEEN 61 AND 90 THEN amountLCY ELSE NULL END AS [lcy6090],
	CASE WHEN lateDays > 90 THEN amountLCY ELSE NULL END AS [lcy90]
FROM EntryDetails");

                try
                {
                    var lines = await c.QueryAsync<CustAgedInvoices>(sql.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });

            app.MapGet("/api/HulkenInvoices", async (IDbConnection c, DBAccess context, [AsParameters] HulkenQuery hulkenQuery) =>
            {
                string DefCur;
                try
                {
                    DefCur = GetDefaultCurrency("CDF", context.dynDBName, c);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }

                StringBuilder sql = new($@"
SELECT 
    sh.[No_] as InvoiceNo,
    sh.[Sell-to Customer Name] as Customer,
    sh.[Sell-to Country_Region Code] as CustCountry,
    sh.[Bill-to Name] as InvCustomer,
    sh.[Bill-to Country_Region Code] as InvCustCountry,
    sh.[Posting Date] as InvDate,
    CASE sl.[Type]
		WHEN 1 THEN 'Discount'
		WHEN 2 THEN 'Invoice'
		ELSE CAST(sl.[Type] as varchar)
	END as [EntryType],
    sl.[No_] AS ArticleNo,
    sl.[Item Category Code] as ItemCat,
    sl.[Description] as Article,
	Art.strSize as Size,
	Art.strColor as Color,
	Art.strRemark as Rem,
	Art.strBagType as BagType,
	Art.strBagtypeVersion as BagVersion,
    CASE sl.[Type]
		WHEN 2 THEN sl.Quantity
		ELSE 0
	END as Quantity,
    sl.[Unit Price] as UnitPrice,
    CASE sh.[Currency Code]
		WHEN '' THEN 'EUR'
		ELSE sh.[Currency Code]
	END as Cur,
    sl.[Amount],
    CASE sh.[Currency Code]
		WHEN '' THEN sl.[Amount] * SatV2.dbo.getDynExchangeRate('EUR', '{hulkenQuery.ConvertTo}', sh.[Posting Date])
		ELSE sl.[Amount] * SatV2.dbo.getDynExchangeRate(sh.[Currency Code], '{hulkenQuery.ConvertTo}', sh.[Posting Date])
	END as AmountConv	
FROM 
    {context.dynDBName}.[CDF$Sales Invoice Header$437dbf0e-84ff-417a-965d-ed2bb9650972] sh
	INNER JOIN {context.dynDBName}.[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sl ON sh.[No_] = sl.[Document No_]
	LEFT OUTER JOIN Codefine.dbo.PRODUCT_K Art on LEFT(sl.[No_], LEN(sl.[No_]) - 4) = CAST(Art.[ARTICLE_NUMBER] as varchar)
WHERE 
    EXISTS (
        SELECT 1
        FROM {context.dynDBName}.[CDF$Sales Invoice Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sub
        WHERE 
            sub.[Document No_] = sh.[No_]
            AND sub.[Item Category Code] = 'HULKEN')
    AND ([Item Category Code] = 'HULKEN' OR sl.[Type] = 1) 
    AND sh.[Posting Date] >= '{hulkenQuery.FromDate:yyyy-MM-dd}' AND sh.[Posting Date] <= '{hulkenQuery.ToDate:yyyy-MM-dd}'
    {(hulkenQuery.showUSSales ? "": "AND sh.[Sell-to Customer Name] not like 'Hulken Inc.%'")}
UNION  

select 
	ch.[No_] as InvoiceNo,
    ch.[Sell-to Customer Name],
    ch.[Sell-to Country_Region Code],
    ch.[Bill-to Name],
    ch.[Bill-to Country_Region Code],
    ch.[Posting Date],
    CASE cl.[Type]
		WHEN 1 THEN 'CR Discount'
		WHEN 2 THEN 'CR Invoice'
		ELSE CAST(cl.[Type] as varchar)
	END as [EntryType],
    cl.[No_] AS ArticleNo,
    cl.[Item Category Code],
    cl.[Description],
	Art.strSize as Size,
	Art.strColor as Color,
	Art.strRemark as Rem,
	Art.strBagType as BagType,
	Art.strBagtypeVersion as BagVersion,
    CASE cl.[Type]
		WHEN 2 THEN cl.Quantity * -1
		ELSE 0
	END as Quantity,
    cl.[Unit Price] * -1,
    CASE ch.[Currency Code]
		WHEN '' THEN 'EUR'
		ELSE ch.[Currency Code]
	END as Cur,
    cl.[Amount] * -1,
    CASE ch.[Currency Code]
		WHEN '' THEN cl.[Amount] * SatV2.dbo.getDynExchangeRate('EUR', '{hulkenQuery.ConvertTo}', ch.[Posting Date]) * -1
		ELSE cl.[Amount] * SatV2.dbo.getDynExchangeRate(ch.[Currency Code], '{hulkenQuery.ConvertTo}', ch.[Posting Date]) * -1
	END as AmountConv
from 
	{context.dynDBName}.[CDF$Sales Cr_Memo Header$437dbf0e-84ff-417a-965d-ed2bb9650972] ch
	inner join {context.dynDBName}.[CDF$Sales Cr_Memo Line$437dbf0e-84ff-417a-965d-ed2bb9650972] cl on ch.No_ = cl.[Document No_]
	LEFT OUTER JOIN Codefine.dbo.PRODUCT_K Art on LEFT(cl.[No_], LEN(cl.[No_]) - 4) = CAST(Art.[ARTICLE_NUMBER] as varchar)
where 
	EXISTS (
        SELECT 1
        FROM {context.dynDBName}.[CDF$Sales Cr_Memo Line$437dbf0e-84ff-417a-965d-ed2bb9650972] sub
        WHERE 
            sub.[Document No_] = ch.[No_]
            AND sub.[Item Category Code] = 'HULKEN')
	AND ([Item Category Code] = 'HULKEN' OR cl.[Type] = 1)
    AND ch.[Posting Date] >= '{hulkenQuery.FromDate:yyyy-MM-dd}' AND ch.[Posting Date] <= '{hulkenQuery.ToDate:yyyy-MM-dd}'
    {(hulkenQuery.showUSSales ? "": "AND ch.[Sell-to Customer Name] not like 'Hulken Inc.%'")}
");
                try
                {
                    var lines = await c.QueryAsync<HulkenInvoices>(sql.ToString());
                    return Results.Ok(lines);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500);
                }
            });
        }
    }
}

