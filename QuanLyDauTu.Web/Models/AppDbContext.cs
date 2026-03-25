using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using QuanLyDauTu.Models.Entities;

namespace QuanLyDauTu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=QuanLyDauTuContext")
        {
            Database.CommandTimeout = 60;
        }

        // Shared
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryItem> CategoryItems { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<ExcelImportLog> ExcelImportLogs { get; set; }

        // Doan Vien
        public DbSet<MemberGroup> MemberGroups { get; set; }
        public DbSet<Member> Members { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Many-to-many: User <-> Role
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany()
                .Map(m => { m.ToTable("UserRoles"); m.MapLeftKey("UserId"); m.MapRightKey("RoleId"); });

            // Many-to-many: Role <-> Permission
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany()
                .Map(m => { m.ToTable("RolePermissions"); m.MapLeftKey("RoleId"); m.MapRightKey("PermissionId"); });

            // Many-to-many: User <-> Permission (direct)
            modelBuilder.Entity<User>()
                .HasMany(u => u.DirectPermissions)
                .WithMany()
                .Map(m => { m.ToTable("UserPermissions"); m.MapLeftKey("UserId"); m.MapRightKey("PermissionId"); });

            base.OnModelCreating(modelBuilder);
        }
    }
}
