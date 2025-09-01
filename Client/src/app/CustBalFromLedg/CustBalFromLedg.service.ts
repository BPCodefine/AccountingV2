import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustBalanceFromLedger } from './CustBal.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class CustBalFromLedgService {

  constructor(private httpClient: HttpClient) {}

  getCustBalance(Company: string): Observable<CustBalanceFromLedger[]> {
    return this.httpClient.get<CustBalanceFromLedger[]>(environment.apiUrl + '/api/CustomerInvoices' +
      `?Company=${Company}`);
    }
}
