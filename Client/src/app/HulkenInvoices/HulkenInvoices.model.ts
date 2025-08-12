export interface HulkenInvoices {
  invoiceNo: string;
  customer: string;
  custCountry?: string;
  invCustomer: string;
  invCustCountry?: string;
  invDate: string;
  entryType: string;
  articleNo: string;
  itemCat: string;
  article: string;
  size?: string;
  color?: string;
  rem?: string;
  bagType?: string;
  bagVersion?: string;
  quantity: number;
  unitPrice: number;
  cur: string;
  amount: number;
  amountConv: number;
}
