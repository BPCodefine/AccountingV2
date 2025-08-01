export interface CustAgedInvoices {
  invoiceNo: string ,
  customerNo: string ,
  customerName: string ,
  invoiceDate: Date ,
  descr: string ,
  amount: number ,
  dueDate: Date ,
  cur: string ,
  lateDays: number ,
  acc30: number ,
  acc3060: number ,
  acc6090: number ,
  acc90: number ,
  lcy30: number ,
  lcy3060: number ,
  lcy6090: number ,
  lcy90: number
}
