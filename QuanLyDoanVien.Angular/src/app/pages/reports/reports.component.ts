import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../core/services/api.service';

interface UnitReportItem {
  unitId: number;
  unitCode: string;
  unitName: string;
  total: number;
  male: number;
  female: number;
}

interface ByUnitReport {
  grandTotal: number;
  totalMale: number;
  totalFemale: number;
  noUnit: number;
  units: UnitReportItem[];
}

interface ChartItem {
  label: string;
  count: number;
}

interface DemographicsReport {
  total: number;
  byGender: ChartItem[];
  byAge: ChartItem[];
  byEthnicity: ChartItem[];
  byReligion: ChartItem[];
  byEducation: ChartItem[];
  byExpertise: ChartItem[];
  byProfession: ChartItem[];
  byPoliticalTheory: ChartItem[];
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  activeTab: 'unit' | 'demographics' = 'unit';
  loadingUnit = false;
  loadingDemographics = false;

  unitReport: ByUnitReport | null = null;
  demographics: DemographicsReport | null = null;

  selectedUnitId: number | null = null;
  units: { id: number; unitName: string }[] = [];

  constructor(private api: ApiService, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.loadUnitReport();
    this.loadUnits();
  }

  loadUnits(): void {
    this.api.getUnits({ pageSize: 500 }).subscribe({
      next: res => {
        this.units = (res.items || []).map((u: any) => ({ id: u.id, unitName: u.unitName }));
      }
    });
  }

  loadUnitReport(): void {
    this.loadingUnit = true;
    this.api.getReportByUnit().subscribe({
      next: (data: ByUnitReport) => { this.unitReport = data; this.loadingUnit = false; },
      error: () => {
        this.snackBar.open('Lỗi tải báo cáo theo đơn vị', 'Đóng', { duration: 3000 });
        this.loadingUnit = false;
      }
    });
  }

  loadDemographics(): void {
    this.loadingDemographics = true;
    const uid = this.selectedUnitId ?? undefined;
    this.api.getReportDemographics(uid).subscribe({
      next: (data: DemographicsReport) => { this.demographics = data; this.loadingDemographics = false; },
      error: () => {
        this.snackBar.open('Lỗi tải báo cáo cơ cấu', 'Đóng', { duration: 3000 });
        this.loadingDemographics = false;
      }
    });
  }

  switchTab(tab: 'unit' | 'demographics'): void {
    this.activeTab = tab;
    if (tab === 'unit' && !this.unitReport) this.loadUnitReport();
    if (tab === 'demographics' && !this.demographics) this.loadDemographics();
  }

  onUnitFilterChange(newValue: number | null): void {
    this.selectedUnitId = newValue;
    this.demographics = null;
    this.loadDemographics();
  }

  pct(count: number, total: number): number {
    if (!total) return 0;
    return Math.round((count / total) * 100);
  }

  getBarColor(index: number): string {
    const palette = [
      '#4f8ef7', '#2ec4b6', '#e26d5c', '#f7c94e',
      '#9b72cf', '#5cb85c', '#f0ad4e', '#d9534f'
    ];
    return palette[index % palette.length];
  }
}
