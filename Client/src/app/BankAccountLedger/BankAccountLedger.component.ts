import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

import { DxDateBoxModule, DxTextBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxTextBoxTypes } from 'devextreme-angular/ui/text-box'
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';
import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { BankAccountLedgerModel } from './BankAccountLedger.model';
import { BankAccountLedgerService } from './BankAccountLedger.service';

@Component({
  selector: 'app-bank-account-ledger',
  imports: [CommonModule,
            DxDateBoxModule,
            DxTextBoxModule,
            DxDataGridModule],
  providers: [DatePipe],
  templateUrl: './BankAccountLedger.component.html',
  styleUrl: './BankAccountLedger.component.css'
})
export class BankAccountLedgerComponent implements OnInit {
  loading: boolean = true;
  endDate: Date = new Date();
  strEndDate: string = '';
  accountsText: string = '';
  defAccounts: string[] = [];
  invoices : BankAccountLedgerModel[] = [];

  constructor(private datePipe: DatePipe, private ledgerService: BankAccountLedgerService) {}

  ngOnInit(): void {
    this.FetchLedgerLines();
  }

  accListValueChanged(data: DxTextBoxTypes.ValueChangedEvent) {
    this.defAccounts = this.accountsText.split(',').map(acc => acc.trim());
  }

  FetchLedgerLines(): void {
    this.loading = true;
    this.strEndDate = this.datePipe.transform(this.endDate, 'yyyy-MM-dd')!;

    this.ledgerService.getLedgerEntries(this.strEndDate, this.defAccounts).subscribe(
      {
      next: (data) => {
        this.invoices = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      },
    });
    console.log('Fetching invoices...');
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
