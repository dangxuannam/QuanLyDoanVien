import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';

@Component({ selector: 'app-dashboard', templateUrl: './dashboard.component.html' })
export class DashboardComponent implements OnInit {
  year = new Date().getFullYear();
  projectStats: any = {};
  memberStats: any = {};
  recentFiles: any[] = [];
  loading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getMemberStats().subscribe({ next: r => this.memberStats = r, error: () => {} });
    this.api.getFiles({ page: 1, pageSize: 6 }).subscribe({
      next: r => { this.recentFiles = r.items; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

}
