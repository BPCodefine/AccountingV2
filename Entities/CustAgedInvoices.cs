namespace AccountingV2.Entities
{
    public class CustAgedInvoices
    {
        public required string invoiceNo { get; set; }
        public required string customerNo { get; set; }
        public required string customerName { get; set; }
        public DateTime invoiceDate { get; set; }
        public required string descr { get; set; }
        public decimal amount { get; set; }
        public DateTime dueDate { get; set; }
        public required string cur { get; set; }   
        public int lateDays { get; set; }
        public decimal acc30 { get; set; }
        public decimal acc3060 { get; set; }
        public decimal acc6090 { get; set; }
        public decimal acc90 { get; set; }
        public decimal lcy30 { get; set; }
        public decimal lcy3060 { get; set; }
        public decimal lcy6090 { get; set; }
        public decimal lcy90 { get; set; }
    }
}
