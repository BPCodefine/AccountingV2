import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustInvoices } from './CustInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class CustInvoiceService {

  constructor(private httpClient: HttpClient) {}

  getCustInvoices(fromDate: string, toDate: string, Company: string): Observable<CustInvoices[]> {
    return this.httpClient.get<CustInvoices[]>(environment.apiUrl + '/api/CustomerInvoices' +
      `?FromDate=${fromDate}&ToDate=${toDate}&Company=${Company}`);
    }

  getLateCustInvoices(Company: string): Observable<CustInvoices[]> {
    return this.httpClient.get<CustInvoices[]>(environment.apiUrl + '/api/LateCustomerInvoices' + `?Company=${Company}`);
  }
}
