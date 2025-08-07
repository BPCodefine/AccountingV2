import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HulkenInvoices } from './HulkenInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class HulkenInvoicesService {
  private apiUrl = environment.apiUrl + '/api/HulkenInvoices';

  constructor(private httpClient: HttpClient) {}

  getHulkenInvoices(fromDate: Date, toDate: Date, showUSSales: boolean): Observable<HulkenInvoices[]> {
    return this.httpClient.get<HulkenInvoices[]>(this.apiUrl +
      `?FromDate=${fromDate.toISOString().split('T')[0]}&ToDate=${toDate.toISOString().split('T')[0]}&ShowUSSales=${showUSSales}`);
    }
  }

