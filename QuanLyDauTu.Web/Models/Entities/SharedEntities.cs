using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDauTu.Models.Entities
{
    [Table("Roles")]
    public class Role
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string RoleCode { get; set; }
        [Required, MaxLength(200)] public string RoleName { get; set; }
        [MaxLength(500)] public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Permission> Permissions { get; set; } = new HashSet<Permission>();
    }

    [Table("Users")]
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string Username { get; set; }
        [Required, MaxLength(256)] public string PasswordHash { get; set; }
        [Required, MaxLength(100)] public string PasswordSalt { get; set; }
        [Required, MaxLength(200)] public string FullName { get; set; }
        [MaxLength(200)] public string Email { get; set; }
        [MaxLength(20)] public string Phone { get; set; }
        [MaxLength(300)] public string DonVi { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int? CreatedBy { get; set; }
        public virtual ICollection<Role> Roles { get; set; } = new HashSet<Role>();
        public virtual ICollection<Permission> DirectPermissions { get; set; } = new HashSet<Permission>();
    }

    [Table("UserTokens")]
    public class UserToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(256)] public string Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        [ForeignKey("UserId")] public virtual User User { get; set; }
    }

    [Table("Permissions")]
    public class Permission
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string PermissionCode { get; set; }
        [Required, MaxLength(200)] public string PermissionName { get; set; }
        [MaxLength(100)] public string Module { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [Table("MenuItems")]
    public class MenuItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        [Required, MaxLength(200)] public string MenuName { get; set; }
        [MaxLength(500)] public string Url { get; set; }
        [MaxLength(100)] public string Icon { get; set; }
        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        [MaxLength(100)] public string RequiredPermission { get; set; }
        [MaxLength(100)] public string Module { get; set; }
    }

    [Table("AuditLogs")]
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        [MaxLength(50)] public string Username { get; set; }
        [Required, MaxLength(100)] public string Action { get; set; }
        [MaxLength(100)] public string Module { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        [MaxLength(50)] public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Categories")]
    public class Category
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string CategoryCode { get; set; }
        [Required, MaxLength(200)] public string CategoryName { get; set; }
        [MaxLength(500)] public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [Table("CategoryItems")]
    public class CategoryItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [Required, MaxLength(50)] public string ItemCode { get; set; }
        [Required, MaxLength(300)] public string ItemName { get; set; }
        public bool IsActive { get; set; } = true;
        public int OrderIndex { get; set; } = 0;
        [ForeignKey("CategoryId")] public virtual Category Category { get; set; }
    }

    [Table("FileAttachments")]
    public class FileAttachment
    {
        public int Id { get; set; }
        [Required, MaxLength(500)] public string FileName { get; set; }
        [Required, MaxLength(500)] public string OriginalName { get; set; }
        [Required, MaxLength(1000)] public string FilePath { get; set; }
        public long? FileSize { get; set; }
        [MaxLength(200)] public string ContentType { get; set; }
        [MaxLength(100)] public string Module { get; set; }
        public int? RelatedId { get; set; }
        [MaxLength(500)] public string Description { get; set; }
        public int? SheetCount { get; set; }
        public string SheetNames { get; set; }
        public int? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        [ForeignKey("UploadedBy")] public virtual User Uploader { get; set; }
    }

    [Table("ExcelImportLogs")]
    public class ExcelImportLog
    {
        public int Id { get; set; }
        public int FileAttachmentId { get; set; }
        [MaxLength(200)] public string SheetName { get; set; }
        public int? TotalRows { get; set; }
        public int? ImportedRows { get; set; }
        public int? FailedRows { get; set; }
        [MaxLength(50)] public string Status { get; set; }
        public string ErrorLog { get; set; }
        public int? ImportedBy { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.Now;
        [ForeignKey("FileAttachmentId")] public virtual FileAttachment FileAttachment { get; set; }
    }
}
