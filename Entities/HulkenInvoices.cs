namespace AccountingV2.Entities
{
    public class HulkenInvoices
    {
        public required string InvoiceNo { get; set; }
        public required string Customer { get; set; }
        public string? CustCountry { get; set; }
        public required string InvCustomer { get; set; }
        public string? InvCustCountry { get; set; }
        public DateTime InvDate { get; set; }
        public required string EntryType { get; set; }
        public required string ArticleNo { get; set; }
        public required string ItemCat { get; set; }
        public required string Article { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Rem { get; set; }
        public string? BagType { get; set; }
        public string? BagVersion { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public required string Cur { get; set; }
        public decimal Amount { get; set; }
    }
}
