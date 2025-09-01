namespace AccountingV2.Entities
{
    public class CustBalanceFromLegder
    {
        public required string CustomerName { get; set; }           // cust.[Name]
        public int DocType { get; set; }                   // cle.[Document Type]
        public required string DocNo { get; set; }                  // cle.[Document No_]
        public DateTime PostingDate { get; set; }          // cle.[Posting Date]
        public required string Description { get; set; }            // cle.[Description]
        public DateTime DueDate { get; set; }              // cle.[Due Date]
        public required string Cur { get; set; }                    // cle.[Currency Code] with fallback to 'EUR'
        public decimal OrigAmount { get; set; }            // ds.OrigAmount
        public decimal RemainingAmount { get; set; }       // ds.RemainingAmount
        public string? AppliedDocNo { get; set; }           // dcle.[Document No_]
        public decimal AppliedAmount { get; set; }         // dcle.Amount
        public DateTime? AppliedPostingDate { get; set; }  // dcle.[Posting Date] (nullable if no match)
        
    }
}
