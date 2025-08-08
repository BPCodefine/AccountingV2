import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InvExpensesModel } from './InvExpenses.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class InvExpensesService {
  private apiUrl = environment.apiUrl + '/api/PurchInvAndExpenses';

  constructor(private httpClient: HttpClient) {}

  getInvoices(fromDate: string, toDate: string): Observable<InvExpensesModel[]> {
    return this.httpClient.get<InvExpensesModel[]>(this.apiUrl +
      `/betweenDates?FromDate=${fromDate}&ToDate=${toDate}`);
    }

  getAllInvoices(): Observable<InvExpensesModel[]> {
    return this.httpClient.get<InvExpensesModel[]>(this.apiUrl);
  }
}
