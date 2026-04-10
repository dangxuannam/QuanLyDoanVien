import { Component, OnInit, HostListener } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { MenuItem } from '../../core/models/models';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent implements OnInit {
  sidebarOpen = window.innerWidth > 768;
  isMobile = window.innerWidth <= 768;
  menus: MenuItem[] = [];
  rootMenus: MenuItem[] = [];
  childMenus: { [key: number]: MenuItem[] } = {};
  user = this.authService.getUser();

  get sidebarMode(): 'over' | 'side' { return this.isMobile ? 'over' : 'side'; }

  constructor(
    public authService: AuthService,
    private apiService: ApiService,
    private router: Router
  ) { }

  @HostListener('window:resize', ['$event'])
  onResize() {
    this.isMobile = window.innerWidth <= 768;
    this.sidebarOpen = window.innerWidth > 768;
  }

  ngOnInit(): void {
    this.loadMenus();
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      if (this.isMobile) this.sidebarOpen = false;
    });
  }

  loadMenus(): void {
    this.apiService.getMenus().subscribe({
      next: (menus) => {
        this.menus = menus;
        this.rootMenus = menus.filter(m => !m.parentId);
        const children: { [k: number]: MenuItem[] } = {};
        menus.filter(m => m.parentId).forEach(m => {
          if (!children[m.parentId!]) children[m.parentId!] = [];
          children[m.parentId!].push(m);
        });
        this.childMenus = children;
      },
      error: () => { }
    });
  }

  hasChildren(id: number): boolean {
    return !!(this.childMenus[id] && this.childMenus[id].length > 0);
  }

  navigate(url?: string): void {
    if (url) {
      const path = url.replace('#!/', '/');
      this.router.navigateByUrl(path);
    }
  }

  isActive(url: string): boolean {
    if (!url) return false;
    const path = url.replace('#!/', '/');
    return this.router.isActive(path, { paths: 'subset', queryParams: 'ignored', fragment: 'ignored', matrixParams: 'ignored' });
  }

  logout(): void {
    this.apiService.logout().subscribe({
      complete: () => { this.authService.clearSession(); this.router.navigate(['/login']); }
    });
  }

  toggleSidebar(): void { this.sidebarOpen = !this.sidebarOpen; }

  hasUnitMenuInDynamic(): boolean {
    return this.menus.some(m =>
      (m.url || '').replace('#!/', '/').includes('/units') ||
      (m.menuName || '').toLowerCase().includes('đơn vị')
    );
  }

  getMaterialIcon(faIcon?: string): string {
    const map: { [k: string]: string } = {
      'fa-tachometer': 'dashboard', 'fa-cogs': 'settings', 'fa-building': 'apartment',
      'fa-industry': 'factory', 'fa-money': 'payments', 'fa-map-marker': 'map',
      'fa-bar-chart': 'bar_chart', 'fa-users': 'groups', 'fa-user': 'person',
      'fa-shield': 'shield', 'fa-list': 'list', 'fa-history': 'history',
      'fa-paperclip': 'attach_file', 'fa-list-alt': 'list_alt', 'fa-plus': 'add',
      'fa-dollar': 'paid', 'fa-line-chart': 'trending_up', 'fa-upload': 'upload',
      'fa-file-text': 'description', 'fa-briefcase': 'business_center',
      'fa-calculator': 'calculate', 'fa-arrow-down': 'arrow_downward',
      'fa-arrow-up': 'arrow_upward', 'fa-globe': 'public', 'fa-map-pin': 'place',
      'fa-th-list': 'grid_on', 'fa-pencil': 'edit', 'fa-user-plus': 'person_add',
      'fa-object-group': 'folder_special', 'fa-building-o': 'business',
      'business': 'business'
    };
    return map[faIcon || ''] || faIcon || 'circle';
  }
}

