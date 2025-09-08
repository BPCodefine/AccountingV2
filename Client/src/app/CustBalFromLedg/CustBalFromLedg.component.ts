import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

import { DxSelectBoxModule, DxDateRangeBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"
import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';

import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { CustBalanceFromLedger } from './CustBal.model';
import { CustBalFromLedgService } from './CustBalFromLedg.service';
import { UserService } from '../GeneralData/WinUserName.service';

@Component({
  selector: 'app-cust-bal-from-ledg',
  imports: [CommonModule,
            DxSelectBoxModule,
            DxDataGridModule,
            DxDateRangeBoxModule],
  providers: [DatePipe],
  templateUrl: './CustBalFromLedg.component.html',
  styleUrl: './CustBalFromLedg.component.css'
})
export class CustBalFromLedgComponent implements OnInit {

  username = '';
  loading: boolean = true;

  minDate: Date = new Date(1999, 12, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  strStartDate: string = '';
  strEndDate: string = '';
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  showOverdue: boolean = false;

  invoices: CustBalanceFromLedger[] = [];
  enabledComps: string[] = [];
  selectedComp!: string;

  constructor(private datePipe: DatePipe, private userService: UserService, private custInvService: CustBalFromLedgService) {}

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

    this.custInvService.getCustBalanceRecords(this.strStartDate, this.strEndDate, this.selectedComp).subscribe({
      next: (data) => {
        this.invoices = data.map(inv => {
          const today = new Date();
          const dueDate = new Date(inv.dueDate); // adjust field name if needed

          let status = 'open'; // default

          if (inv.remainingAmount === 0) {
            status = 'paid';
          } else if (dueDate < today) {
            status = 'overdue';
          } else if (inv.remainingAmount > 0 && inv.remainingAmount != inv.origAmount) {
            status = 'partial';
          }

          return { ...inv, status };
        });

        this.loading = false;
      },
      error: err => {
        console.error('Failed to fetch invoices', err);
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

    onRowPrepared(e: any): void {
      if (e.rowType === 'data') {
        const status = e.data.status?.toLowerCase();
        switch (status) {
          case 'open':
            e.rowElement.classList.add('open-entry');
            break;
          case 'partial':
            e.rowElement.classList.add('partial-entry');
            break;
          case 'paid':
            e.rowElement.classList.add('paid-entry');
            break;
          case 'overdue':
            e.rowElement.classList.add('overdue-entry');
            break;
        }
      }
    }

}
