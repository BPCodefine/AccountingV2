import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustAgedInvoices } from './CustAgedInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class CustAgedInvoiceService {

  constructor(private httpClient: HttpClient) {}

  getCustAgedInvoices(Company: string): Observable<CustAgedInvoices[]> {
    return this.httpClient.get<CustAgedInvoices[]>(environment.apiUrl + '/api/CustAgedInvoices' +
      `?Company=${Company}`);
    }

}
