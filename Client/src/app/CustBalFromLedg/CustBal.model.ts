export interface CustBalanceFromLedger {
  customerName: string;           // cust.[Name]
  docType: number;                // cle.[Document Type]
  docNo: string;                  // cle.[Document No_]
  postingDate: Date;             // cle.[Posting Date]
  description: string;           // cle.[Description]
  dueDate: Date;                 // cle.[Due Date]
  cur: string;                   // cle.[Currency Code], fallback to 'EUR'
  origAmount: number;           // ds.OrigAmount
  remainingAmount: number;      // ds.RemainingAmount
  appliedDocNo: string;         // dcle.[Document No_]
  appliedAmount: number;        // dcle.Amount
  appliedPostingDate: Date;    // dcle.[Posting Date]
}
