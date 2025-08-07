import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxDateRangeBoxModule, DxCheckBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"
import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';

import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { HulkenInvoicesService } from './HulkenInvoices.service';
import { HulkenInvoices } from './HulkenInvoices.model';

@Component({
  selector: 'app-hulken-invoices.component',
  imports: [CommonModule,
            DxDataGridModule,
            DxCheckBoxModule,
            DxDateRangeBoxModule],
  templateUrl: './HulkenInvoices.component.html',
  styleUrl: './HulkenInvoices.component.css'
})
export class HulkenInvoicesComponent implements OnInit{
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];
  showUSSales: boolean = false;

  invoices: HulkenInvoices[] = [];

  constructor(private hulkenInvoicesService: HulkenInvoicesService) {}

  ngOnInit(): void {
    this.FetchInvoices();
  }

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
  }

  FetchInvoices(): void {
    this.loading = true;
    this.hulkenInvoicesService.getHulkenInvoices(this.startDate, this.endDate, this.showUSSales).subscribe({
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
