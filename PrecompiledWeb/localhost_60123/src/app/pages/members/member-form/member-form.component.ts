import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { MemberGroup } from '../../../core/models/models';

@Component({ selector: 'app-member-form', templateUrl: './member-form.component.html' })
export class MemberFormComponent implements OnInit {
  form: FormGroup;
  loading = false;
  isEdit = false;
  memberId?: number;
  groups: MemberGroup[] = [];

  // --- Độ tuổi ---
  ageLabel: string = '';

  // --- Dropdown options ---
  educationOptions = ['THCS', 'THPT'];
  expertiseOptions = ['Cao đẳng', 'Đại học', 'Thạc sỹ', 'Tiến sỹ'];
  politicalTheoryOptions = ['Sơ cấp', 'Trung cấp', 'Cao cấp', 'Cử nhân'];
  mainPositionOptions = ['Ban chấp hành', 'Ban thường vụ', 'Bí thư', 'Phó bí thư'];
  ageCategoryOptions = ['Từ 18 đến đủ 25 tuổi', 'Từ 26 đến đủ 30 tuổi', 'Trên 30 tuổi'];

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
      memberCode: [''],
      address: [''],
      dateOfBirth: [null],
      joinDate: [null],
      groupId: [null],
      gender: [''],
      ethnicity: [''],
      religion: [''],
      profession: [''],
      cardNumber: [''],             // Sổ đoàn viên
      mainPosition: [''],           // Chức vụ chủ chốt
      education: [''],              // Học vấn (dropdown)
      expertise: [''],              // Chuyên môn (dropdown)
      politicalTheory: [''],        // Lý luận chính trị (dropdown)
      ageCategory: [''],            // Độ tuổi (tính tự động, user xác nhận)
      isUnionMember: [true],
      isActive: [true],
      notes: [''],
      // Trường position vẫn giữ nhưng ẩn (dùng cho import)
      position: ['']
    });
  }

  ngOnInit(): void {
    this.memberId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEdit = !!this.memberId;
    if (this.isEdit) this.loadMember();
    this.loadGroups();

    this.form.get('dateOfBirth')?.valueChanges.subscribe(dob => {
      this.calculateAge(dob);
    });
  }

  calculateAge(dob: any) {
    if (!dob) {
      this.ageLabel = '';
      this.form.get('ageCategory')?.setValue('', { emitEvent: false });
      return;
    }
    const birthDate = new Date(dob);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    let category = '';
    if (age >= 18 && age <= 25) {
      category = 'Từ 18 đến đủ 25 tuổi';
    } else if (age >= 26 && age <= 30) {
      category = 'Từ 26 đến đủ 30 tuổi';
    } else if (age > 30) {
      category = 'Trên 30 tuổi';
    }

    this.ageLabel = age >= 0 ? `${age} tuổi` : '';
    this.form.get('ageCategory')?.setValue(category, { emitEvent: false });
  }

  loadGroups() { this.api.getMemberGroups().subscribe(g => this.groups = g); }

  loadMember() {
    if (!this.memberId) return;
    this.loading = true;
    this.api.getMember(this.memberId).subscribe({
      next: (res) => {
        this.form.patchValue(res);
        if (res.dateOfBirth) this.calculateAge(res.dateOfBirth);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;
    const value = { ...this.form.value, position: this.form.value.mainPosition };
    const obs = this.isEdit
      ? this.api.updateMember(this.memberId!, value)
      : this.api.createMember(value);

    obs.subscribe({
      next: () => {
        this.snack.open('Lưu thành công', 'Đóng', { duration: 3000 });
        this.router.navigate(['/members']);
      },
      error: () => { this.loading = false; }
    });
  }

  cancel() { this.router.navigate(['/members']); }
}
