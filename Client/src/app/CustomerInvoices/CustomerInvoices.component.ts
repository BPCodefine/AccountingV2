import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

import { DxSelectBoxModule, DxDateRangeBoxModule, DxCheckBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"
import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';

import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { CustInvoices } from './CustInvoices.model';
import { CustInvoiceService } from './CustInvoices.service';
import { UserService } from '../GeneralData/WinUserName.service';

@Component({
  selector: 'app-customer-invoices',
  imports: [CommonModule,
            DxSelectBoxModule,
            DxDataGridModule,
            DxCheckBoxModule,
            DxDateRangeBoxModule],
  providers: [DatePipe],
  templateUrl: './CustomerInvoices.component.html',
  styleUrl: './CustomerInvoices.component.css'
})
export class CustomerInvoicesComponent implements OnInit {
  username = '';
  loading: boolean = true;

  minDate: Date = new Date(1999, 12, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  strStartDate: string = '';
  strEndDate: string = '';
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  showOverdue: boolean = false;

  invoices: CustInvoices[] = [];
  enabledComps: string[] = [];
  selectedComp!: string;

  constructor(private datePipe: DatePipe, private userService: UserService, private custInvoicesService: CustInvoiceService) {}

  ngOnInit(): void {
    this.userService.getUsername().subscribe({
      next: res => {
        this.username = res;

        this.userService.getEnabledCompanies(this.username).subscribe({
          next: comps => {
            this.enabledComps = comps;

            if (this.enabledComps.length > 0) {
              this.selectedComp = this.enabledComps[0];
            }
            this.FetchInvoices();
          },
          error: err => {
            console.error('Failed to get enabled companies', err);
          }
        });
      },
      error: err => {
        console.error('Failed to get username', err);
        this.username = 'Unknown User';
      }
    });
  }

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
  }

  FetchInvoices() {
    this.loading = true;

    this.strStartDate = this.datePipe.transform(this.startDate, 'yyyy-MM-dd')!;
    this.strEndDate = this.datePipe.transform(this.endDate, 'yyyy-MM-dd')!;

    const invoiceObservable = this.showOverdue
      ? this.custInvoicesService.getLateCustInvoices(this.selectedComp)
      : this.custInvoicesService.getCustInvoices(this.strStartDate, this.strEndDate, this.selectedComp);

    invoiceObservable.subscribe({
      next: (data) => {
        this.invoices = data.map(invoice => ({
          ...invoice,
          paymentDate: invoice.paymentDate === '0001-01-01T00:00:00' ? '' : invoice.paymentDate
        }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      }
    });
  }

  onExporting(e: DxDataGridTypes.ExportingEvent) {
      const workbook = new Workbook();
      const worksheet = workbook.addWorksheet('Data');

      exportDataGrid({
        component: e.component,
        worksheet,
        autoFilterEnabled: true,
      }).then(() => {
        workbook.xlsx.writeBuffer().then((buffer) => {
          saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'DataGrid.xlsx');
        });
      });
    }
}
