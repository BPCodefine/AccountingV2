import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav } from '@angular/material/sidenav';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    RouterModule,
    MatMenuModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'angular-sidenav';
  showFiller = false;
  @ViewChild('sidenav') snav?: MatSidenav;

  menuItems: any[] = [
    { icon: 'home', label: 'Home', route: 'home' },
    { icon: 'people',
      label: 'Customers',
      children: [
        { icon: 'receipt_long', label: 'Invoice list', route: 'CustomerInvoices' },
        { icon: 'request_quote', label: 'Aged Invoices', route: 'CustomerAgedInvoices' },
        { icon: 'account_balance', label: 'Cust. Balance from Ledger', route: 'CustBalFromLedg'},
        { icon: 'receipt', label: 'Hulken Invoices', route: 'HulkenInvoices' }
      ]
    },
    {
      icon: 'inventory',
      label: 'Vendors',
      children: [
        { icon: 'assignment', label: 'Invoices and Expenses', route: 'InvExpenses' },
        { icon: 'description', label: 'Invoice list', route: 'VendorInvoices' },
      ]
    },
    { icon: 'account_balance', label: 'Bank Account Ledger', route: 'BankAccountLedger' },
  ];

  close()
  {
    const sidenav = document.querySelector('mat-sidenav');
    if (sidenav) {
      this.snav?.close();
    }
  }
}
