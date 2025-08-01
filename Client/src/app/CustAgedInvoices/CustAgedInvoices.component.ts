import { Component, OnInit, HostListener, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DxSelectBoxModule, DxDataGridModule } from 'devextreme-angular';

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
export class CustAgedInvoicesComponent implements OnInit, AfterViewInit {

  username = '';
  loading: boolean = true;
  enabledComps: string[] = [];
  selectedComp!: string;
  gridHeight: number = 0;
  myStuff: CustAgedInvoices[] = [];

  constructor(private userService: UserService, private custAgedInvoicesService: CustAgedInvoiceService, private cdr: ChangeDetectorRef) {}

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
    ngAfterViewInit() {
    this.calculateGridHeight();
  }

  @HostListener('window:resize')
  onResize() {
    this.calculateGridHeight();
  }

  calculateGridHeight() {
    this.gridHeight = window.innerHeight - 120;
    this.cdr.detectChanges();
  }

  FetchInvoices() {
    this.loading = true;
    this.custAgedInvoicesService.getCustAgedInvoices(this.selectedComp).subscribe({
      next: (data) => {
        this.myStuff = data;
        this.loading = false;
      },
      error: err => {
        console.error('Failed to fetch invoices', err);
        this.loading = false;
      }
    });
  }
}
