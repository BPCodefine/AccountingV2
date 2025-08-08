
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VendorInvoices } from './VendorInvoices.model';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class VendorInvoicesService {

  constructor(private httpClient: HttpClient) {}

  getVendorInvoices(fromDate: string, toDate: string, Company: string): Observable<VendorInvoices[]> {
    return this.httpClient.get<VendorInvoices[]>(environment.apiUrl + '/api/VendorInvoices' +
      `?FromDate=${fromDate}&ToDate=${toDate}&Company=${Company}`);
    }

  getUnpaidVendorInvoices(fromDate: string, toDate: string, Company: string): Observable<VendorInvoices[]> {
    return this.httpClient.get<VendorInvoices[]>(environment.apiUrl + '/api/VendorInvoices' +
      `?FromDate=${fromDate}&ToDate=${toDate}&Company=${Company}&OnlyOpen=true`);
    }
  }
