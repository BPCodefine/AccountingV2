import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BankAccountLedgerModel } from './BankAccountLedger.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class BankAccountLedgerService {

  private apiUrl = environment.apiUrl + '/api/BankAccountLedgerItems';

  constructor(private httpClient: HttpClient) {}

  getLedgerEntries(toDate: string, fromAccList: string[]): Observable<BankAccountLedgerModel[]> {
    return this.httpClient.get<BankAccountLedgerModel[]>(this.apiUrl + `?ToDate=${toDate}&ExtraAccList=${encodeURIComponent(fromAccList.join(','))}`);
  }

}
