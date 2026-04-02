export interface User {
  id: number;
  username: string;
  fullName: string;
  email?: string;
  phone?: string;
  donVi?: string;
  isActive: boolean;
  isAdmin: boolean;
  createdAt?: string;
  lastLoginAt?: string;
}

export interface Role {
  id: number;
  roleCode: string;
  roleName: string;
  description?: string;
  isActive: boolean;
}

export interface Permission {
  id: number;
  permissionCode: string;
  permissionName: string;
  module?: string;
  assigned?: boolean;
}

export interface MenuItem {
  id: number;
  parentId?: number;
  menuName: string;
  url?: string;
  icon?: string;
  orderIndex: number;
  module?: string;
}

export interface AuthResponse {
  success: boolean;
  token: string;
  userId: number;
  username: string;
  fullName: string;
  isAdmin: boolean;
  donVi?: string;
  permissions: string[];
}

export interface Project {
  id: number;
  projectCode: string;
  projectName: string;
  investorName?: string;
  projectGroup?: string;
  field?: string;
  location?: string;
  district?: string;
  totalInvestment?: number;
  fundingSource?: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  description?: string;
  decisionNo?: string;
  decisionDate?: string;
  isActive: boolean;
  createdAt?: string;
}

export interface OutsideProject {
  id: number;
  projectCode: string;
  projectName: string;
  investorName?: string;
  status?: string;
  field?: string;
  location?: string;
  district?: string;
  landArea?: number;
  totalCapital?: number;
  startDate?: string;
  endDate?: string;
  isPublic: boolean;
  createdAt?: string;
}

export interface Member {
  id: number;
  memberCode: string;
  fullName: string;
  dateOfBirth?: string;
  gender?: string;
  phone?: string;
  email?: string;
  address?: string;
  joinDate?: string;
  groupId?: number;
  groupName?: string;
  groupLevel?: string;
  unitId?: number;
  unitName?: string;
  position?: string;
  cardNumber?: string;
  isUnionMember: boolean;
  isActive: boolean;
  notes?: string;
  ethnicity?: string;
  religion?: string;
  profession?: string;
  education?: string;
  expertise?: string;
  politicalTheory?: string;
  identityNumber?: string;
  healthStatus?: string;
  partyDateProbationary?: Date;
  partyDateOfficial?: Date;
}

export interface MemberGroup {
  id: number;
  groupCode: string;
  groupName: string;
  description?: string;
  memberCount?: number;
  level?: string;
}

export interface FileAttachment {
  id: number;
  originalName: string;
  filePath: string;
  fileSize?: number;
  contentType?: string;
  module?: string;
  sheetCount?: number;
  description?: string;
  uploadedAt: string;
  uploader?: string;
}

export interface ExcelSheet {
  sheetName: string;
  headers: string[];
  rows: string[][];
  totalRows: number;
  isEmpty: boolean;
  hasMore: boolean;
}

export interface ExcelParseResult {
  sheetCount: number;
  sheets: ExcelSheet[];
}

export interface PagedResult<T> {
  total: number;
  page: number;
  pageSize: number;
  items: T[];
}

export interface AuditLog {
  id: number;
  username: string;
  action: string;
  module?: string;
  description?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface MapProject {
  id: number;
  projectCode?: string;
  projectName: string;
  investorName?: string;
  location?: string;
  district?: string;
  latitude?: number;
  longitude?: number;
  status?: string;
  field?: string;
  totalCapital?: number;
  isPublic: boolean;
}

export interface KTXHIndicator {
  id: number;
  code: string;
  name: string;
  groupId?: number;
  unit?: string;
  period?: string;
  isActive: boolean;
}

export interface KTXHReport {
  id: number;
  indicatorId: number;
  indicatorName?: string;
  year: number;
  month?: number;
  quarter?: number;
  value?: number;
  notes?: string;
}

export interface Unit {
  id: number;
  unitCode: string;
  unitName: string;
  description?: string;
  isActive: boolean;
  totalMembers?: number;
  lastImportAt?: string;
  createdAt?: string;
  hasSummary?: boolean;
}
