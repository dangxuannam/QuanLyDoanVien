import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';

@Component({ selector: 'app-dashboard', templateUrl: './dashboard.component.html' })
export class DashboardComponent implements OnInit {
  year = new Date().getFullYear();
  memberStats: any = {};
  unitStats: any   = {};
  recentFiles: any[] = [];
  loading = true;

  // Combobox
  groups: any[] = [];
  units: any[]  = [];
  selectedGroupId: number | null = null;
  selectedUnitId:  number | null = null;
  selectedLevel: string | null   = null;

  readonly LEVELS = [
    'Trung ương', 'Tỉnh', 'Thành phố', 'Huyện', 'Xã / Phường', 'Cơ sở'
  ];

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadMemberStats();
    this.loadUnitStats();
    this.loadGroups();
    this.loadUnits();
    this.api.getFiles({ page: 1, pageSize: 6 }).subscribe({
      next: r => { this.recentFiles = r.items; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  loadMemberStats() {
    this.api.getMemberStats(
      this.selectedGroupId ?? undefined,
      this.selectedLevel ?? undefined,
      this.selectedUnitId ?? undefined
    ).subscribe({
      next: r => this.memberStats = r,
      error: () => {}
    });
  }

  loadUnitStats() {
    this.api.getUnitStats(this.selectedUnitId ?? undefined).subscribe({
      next: r => this.unitStats = r,
      error: () => {}
    });
  }

  loadGroups() {
    this.api.getMemberGroups().subscribe({
      next: r => this.groups = r,
      error: () => {}
    });
  }

  loadUnits() {
    this.api.getUnits({ page: 1, pageSize: 200 }).subscribe({
      next: r => this.units = r.items || [],
      error: () => {}
    });
  }

  get filteredGroups(): any[] {
    if (!this.selectedLevel) return this.groups;
    return this.groups.filter(g => g.level === this.selectedLevel);
  }

  onLevelChange() {
    this.selectedGroupId = null;
    this.loadMemberStats();
  }

  onGroupChange() {
    this.loadMemberStats();
  }

  onUnitChange() {
    this.selectedGroupId = null;
    this.loadMemberStats();
    this.loadUnitStats();
  }

  getMale(): number    { return this.memberStats.byGender?.find((g:any) => g.gender === 'Nam')?.count ?? 0; }
  getFemale(): number  { return this.memberStats.byGender?.find((g:any) => g.gender === 'Nữ')?.count ?? 0; }
  getGroupName(id: number | null): string {
    if (!id) return '';
    return this.groups.find(g => g.id === id)?.groupName ?? '';
  }
  getUnitName(id: number | null): string {
    if (!id) return '';
    return this.units.find(u => u.id === id)?.unitName ?? '';
  }
}
