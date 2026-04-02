using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanVien.Models.Entities
{
    [Table("MemberGroups")]
    public class MemberGroup
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]  public string GroupCode { get; set; }
        [Required, MaxLength(200)] public string GroupName { get; set; }
        [MaxLength(500)]           public string Description { get; set; }
        [MaxLength(100)]           public string Level { get; set; }  // Trung ương, Tỉnh, TP, Huyện, Xã...
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Members")]
    public class Member
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string MemberCode { get; set; }
        [Required, MaxLength(200)] public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(10)] public string Gender { get; set; }
        [MaxLength(20)] public string Phone { get; set; }
        [MaxLength(200)] public string Email { get; set; }
        [MaxLength(500)] public string Address { get; set; }
        public DateTime? JoinDate { get; set; }
        public int? GroupId { get; set; }
        [MaxLength(200)] public string Position { get; set; }
        [MaxLength(50)] public string CardNumber { get; set; }
        public bool IsUnionMember { get; set; } = true;
        public bool IsActive { get; set; } = true;
        [MaxLength(1000)] public string Notes { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(200)] public string Ethnicity { get; set; }
        [MaxLength(200)] public string Religion { get; set; }
        [MaxLength(500)] public string Profession { get; set; }
        [MaxLength(500)] public string Education { get; set; }
        [MaxLength(500)] public string Expertise { get; set; }
        [MaxLength(500)] public string PoliticalTheory { get; set; }
        [MaxLength(20)] public string IdentityNumber { get; set; }
        [MaxLength(200)] public string HealthStatus { get; set; }
        public DateTime? PartyDateProbationary { get; set; }
        public DateTime? PartyDateOfficial { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UnitId { get; set; }
        
        [ForeignKey("GroupId")] public virtual MemberGroup Group { get; set; }
        [ForeignKey("UnitId")] public virtual Unit Unit { get; set; }
    }

    /// <summary>Đơn vị (ví dụ: Trung tâm xúc tiến đầu tư, Sở Tài chính...)</summary>
    [Table("Units")]
    public class Unit
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]  public string UnitCode { get; set; }
        [Required, MaxLength(300)] public string UnitName { get; set; }
        [MaxLength(500)]           public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Lưu dữ liệu tổng hợp dạng JSON sau khi import
        public string SummaryJson { get; set; }

        // Metadata lần import cuối
        public int?      LastImportFileId { get; set; }
        public DateTime? LastImportAt     { get; set; }
        public int?      LastImportBy     { get; set; }
        public int?      TotalMembers     { get; set; }
    }
}
