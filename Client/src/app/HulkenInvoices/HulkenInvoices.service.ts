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

  getHulkenInvoices(fromDate: string, toDate: string, showUSSales: boolean): Observable<HulkenInvoices[]> {
    return this.httpClient.get<HulkenInvoices[]>(this.apiUrl +
      `?FromDate=${fromDate}&ToDate=${toDate}&ShowUSSales=${showUSSales}`);
    }
  }

