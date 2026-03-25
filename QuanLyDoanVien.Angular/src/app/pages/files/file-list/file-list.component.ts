import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({ selector: 'app-file-list', templateUrl: './file-list.component.html' })
export class FileListComponent implements OnInit {
  loading = false;
  data: any = {};
  items: any[] = [];
  total = 0;
  search = '';
  page = 1; pageSize = 15;

  constructor(protected api: ApiService, protected router: Router, protected route: ActivatedRoute, protected snack: MatSnackBar) {}
  ngOnInit(): void {}
}
