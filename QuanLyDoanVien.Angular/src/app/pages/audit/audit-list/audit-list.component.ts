import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { AuditLog, PagedResult } from '../../../core/models/models';
import { PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-audit-list',
  templateUrl: './audit-list.component.html'
})
export class AuditListComponent implements OnInit {
  loading = false;
  items: AuditLog[] = [];
  total = 0;
  search = '';
  page = 1;
  pageSize = 20;
  displayedColumns = ['time', 'user', 'action', 'module', 'description', 'ip'];

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.api.getAuditLogs(this.page, this.pageSize, this.search).subscribe({
      next: (res) => {
        this.items = res.items;
        this.total = res.total;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onPageChange(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.loadData();
  }

  onSearch(): void {
    this.page = 1;
    this.loadData();
  }

  getModuleDisplay(module?: string): string {
    switch(module) {
      case 'HE_THONG': return 'Hệ thống';
      case 'DOAN_VIEN': return 'Đoàn viên';
      case 'DU_AN': return 'Dự án';
      default: return module || '-';
    }
  }

  getActionColor(action: string): string {
    if (action.includes('CREATE')) return 'success';
    if (action.includes('UPDATE')) return 'primary';
    if (action.includes('DELETE')) return 'warn';
    if (action.includes('IMPORT')) return 'accent';
    return '';
  }
}
