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
  units: any[] = [];

  // --- Độ tuổi ---
  ageLabel: string = '';

  // --- Dropdown options ---
  educationOptions = ['THCS', 'THPT'];
  expertiseOptions = ['Cao đẳng', 'Đại học', 'Thạc sỹ', 'Tiến sỹ'];
  politicalTheoryOptions = ['Sơ cấp', 'Trung cấp', 'Cao cấp', 'Cử nhân'];
  mainPositionOptions = ['Ban chấp hành', 'Ban thường vụ', 'Bí thư', 'Phó bí thư'];
  specialistPositionOptions = ['Cấp trưởng', 'Cấp phó'];
  partyCommitteeOptions = ['Tham gia cấp ủy cấp trên cơ sở', 'Tham gia cấp ủy cơ sở'];
  ageCategoryOptions = ['Từ 18 đến đủ 25 tuổi', 'Từ 26 đến đủ 30 tuổi', 'Từ 31 tuổi trở lên'];
  professionOptions = ['Công chức', 'Viên chức', 'Sinh viên', 'Khác'];

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
      dateOfBirth: [null],
      joinDate: [null],
      groupId: [null],
      unitId: [null],
      gender: [''],
      ethnicity: [''],
      religion: [''],
      mainPosition: [''],           // Số đoàn viên đảm nhiệm nhiệm vụ chủ chốt
      specialistPosition: [''],     // Chuyên môn (cấp trưởng/cấp phó)
      profession: [''],             // Nghề nghiệp
      notes: [''],                  // Lưu giá trị tham gia đảng ủy
      education: [''],              // Học vấn (dropdown)
      expertise: [''],              // Chuyên môn học vấn (dropdown)
      politicalTheory: [''],        // Lý luận chính trị (dropdown)
      ageCategory: [''],            // Độ tuổi (tính tự động, user xác nhận)
      isUnionMember: [true],
      isActive: [true],
      // Trường position vẫn giữ nhưng ẩn (dùng cho import)
      position: ['']
    });
  }

  ngOnInit(): void {
    this.memberId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEdit = !!this.memberId;
    if (this.isEdit) this.loadMember();
    this.loadGroups();
    this.loadUnits();

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
    } else if (age >= 31) {
      category = 'Từ 31 tuổi trở lên';
    }

    this.ageLabel = age >= 0 ? `${age} tuổi` : '';
    this.form.get('ageCategory')?.setValue(category, { emitEvent: false });
  }

  loadGroups() { this.api.getMemberGroups().subscribe(g => this.groups = g); }

  loadUnits() { this.api.getUnits({ page: 1, pageSize: 200 }).subscribe(res => this.units = res.items || []); }

  loadMember() {
    if (!this.memberId) return;
    this.loading = true;
    this.api.getMember(this.memberId).subscribe({
      next: (res) => {
        this.form.patchValue(res);
        if (res.dateOfBirth) this.calculateAge(res.dateOfBirth);
        // Parse position string to restore mainPosition and specialistPosition
        if (res.position) {
          const parts = (res.position as string).split(',').map((p: string) => p.trim());
          const mainPos = parts.find((p: string) => this.mainPositionOptions.some(mp => p.toLowerCase() === mp.toLowerCase())) || '';
          const specPos = parts.find((p: string) => this.specialistPositionOptions.some(sp => p.toLowerCase() === sp.toLowerCase())) || '';
          this.form.patchValue({ mainPosition: mainPos, specialistPosition: specPos });
        }
        // Restore partyCommittee from notes field
        if (res.notes) {
          const noteVal = res.notes as string;
          const matchedParty = this.partyCommitteeOptions.find(o => noteVal.includes(o));
          if (matchedParty) {
            this.form.patchValue({ notes: matchedParty });
          }
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;
    const positionParts = [this.form.value.mainPosition, this.form.value.specialistPosition].filter(p => !!p);
    const value = { ...this.form.value, position: positionParts.join(', ') };
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
