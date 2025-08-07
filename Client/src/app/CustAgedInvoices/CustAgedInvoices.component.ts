import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxSelectBoxModule, DxDataGridModule } from 'devextreme-angular';
import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';

import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { CustAgedInvoices } from './CustAgedInvoices.model';
import { CustAgedInvoiceService } from './CustAgedInvoices.service';
import { UserService } from '../GeneralData/WinUserName.service';

@Component({
  selector: 'app-cust-aged-invoices',
  imports: [CommonModule,
            DxSelectBoxModule,
            DxDataGridModule],
  templateUrl: './CustAgedInvoices.component.html',
  styleUrl: './CustAgedInvoices.component.css'
})
export class CustAgedInvoicesComponent implements OnInit {

  username = '';
  loading: boolean = true;
  enabledComps: string[] = [];
  selectedComp!: string;;
  invoices: CustAgedInvoices[] = [];

  constructor(private userService: UserService, private custAgedInvoicesService: CustAgedInvoiceService) {}

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

  FetchInvoices() {
    this.loading = true;
    this.custAgedInvoicesService.getCustAgedInvoices(this.selectedComp).subscribe({
      next: (data) => {
        this.invoices = data;
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
}
