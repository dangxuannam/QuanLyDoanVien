using System;
using System.IO;
using System.Web;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;

namespace QuanLyDoanVien.Services
{
    public class FileService
    {
        private readonly AppDbContext _db;
        private readonly string _uploadsRoot;

        public FileService(AppDbContext db)
        {
            _db = db;
            _uploadsRoot = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/Uploads/")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");

            if (!Directory.Exists(_uploadsRoot))
                Directory.CreateDirectory(_uploadsRoot);
        }

        public FileAttachment SaveFile(HttpPostedFileBase file, int? uploadedBy, string module = null, int? relatedId = null, string description = null)
        {
            var ext = Path.GetExtension(file.FileName);
            var uniqueName = Guid.NewGuid().ToString("N") + ext;
            var subFolder = DateTime.Now.ToString("yyyy/MM");
            var fullDir = Path.Combine(_uploadsRoot, subFolder);

            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            var fullPath = Path.Combine(fullDir, uniqueName);
            file.SaveAs(fullPath);

            var attachment = new FileAttachment
            {
                FileName = uniqueName,
                OriginalName = Path.GetFileName(file.FileName),
                FilePath = subFolder + "/" + uniqueName,
                FileSize = file.ContentLength,
                ContentType = file.ContentType,
                Module = module,
                RelatedId = relatedId,
                Description = description,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.Now
            };

            _db.FileAttachments.Add(attachment);
            _db.SaveChanges();

            return attachment;
        }

        public string GetFullPath(string filePath)
        {
            return Path.Combine(_uploadsRoot, filePath.Replace('/', Path.DirectorySeparatorChar));
        }

        public bool DeleteFile(int id)
        {
            var attachment = _db.FileAttachments.Find(id);
            if (attachment == null) return false;

            var fullPath = GetFullPath(attachment.FilePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _db.FileAttachments.Remove(attachment);
            _db.SaveChanges();
            return true;
        }
    }
}

