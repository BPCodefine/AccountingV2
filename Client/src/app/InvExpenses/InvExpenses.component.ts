import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxDateRangeBoxModule, DxDataGridModule } from 'devextreme-angular';
import { DxDateRangeBoxTypes } from "devextreme-angular/ui/date-range-box"
import { exportDataGrid } from 'devextreme-angular/common/export/excel';
import { DxDataGridTypes } from 'devextreme-angular/ui/data-grid';

import { Workbook } from 'exceljs';
import { saveAs } from 'file-saver';

import { InvExpensesModel } from './InvExpenses.model';
import { InvExpensesService } from './InvExpenses.service';

@Component({
  selector: 'app-inv-expenses-dev-express',
  imports: [CommonModule,
            DxDataGridModule,
            DxDateRangeBoxModule],
  templateUrl: './InvExpenses.component.html',
  styleUrl: './InvExpenses.component.css'
})
export class InvExpensesComponent implements OnInit {

  invoices: InvExpensesModel[] = [];
  loading: boolean = true;

  minDate: Date = new Date(2020, 7, 1);
  startDate: Date = new Date(new Date().setFullYear(new Date().getFullYear() - 1));
  endDate: Date = new Date();
  currentValue: [Date, Date] = [this.startDate, this.endDate];

  constructor(private invExpService: InvExpensesService) {}

  ngOnInit(): void {
    this.FetchInvoices();
  }

  onCurrentValueChanged(e: DxDateRangeBoxTypes.ValueChangedEvent) {
    this.startDate = e.value[0];
    this.endDate = e.value[1];
    this.FetchInvoices();
    //console.log('Current value changed:', this.currentValue);
  }

  FetchInvoices() {
    this.loading = true;
    this.invExpService.getInvoices(this.startDate, this.endDate).subscribe({
      next: (data) => {
        this.invoices = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.loading = false;
      },
    });
  }

  onCellPrepared(e: any) {
    if (e.rowType === "data") {
      if (e.column.dataField === "puaCost" && e.data.puaCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
      if (e.column.dataField === "seafreightCost" && e.data.seafreightCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
      if (e.column.dataField === "dutyCost" && e.data.dutyCost === 0) {
          e.cellElement.style.cssText = "color: white; background-color: #f5887c";
      }
    }
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
