using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien.Api
{
    [RoutePrefix("api/files")]
    [ApiAuthorize]
    public class FileApiController : ApiController
    {
        [HttpPost, Route("upload")]
        [ApiAuthorize(Permission = "FILE_UPLOAD")]
        public IHttpActionResult Upload()
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count == 0)
                return BadRequest("Không có file nào được tải lên.");

            var file = httpRequest.Files[0];
            if (file.ContentLength == 0)
                return BadRequest("File rỗng.");

            long maxSize = 52428800; // 50MB
            if (file.ContentLength > maxSize)
                return BadRequest("File vượt quá dung lượng cho phép (50MB).");

            var module = httpRequest.Form["module"];
            var desc = httpRequest.Form["description"];

            var userId = (int)Request.Properties["CurrentUserId"];

            using (var db = new AppDbContext())
            {
                var fileSvc = new FileService(db);
                var attachment = fileSvc.SaveFile(new HttpPostedFileWrapper(file), userId, module, null, desc);

                // If Excel, parse it
                ExcelParseResult parseResult = null;
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (ext == ".xlsx" || ext == ".xls")
                {
                    try
                    {
                        var excelSvc = new ExcelService();
                        var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                        parseResult = excelSvc.ParseExcel(fullPath);

                        attachment.SheetCount = parseResult.SheetCount;
                        attachment.SheetNames = JsonConvert.SerializeObject(
                            parseResult.Sheets.Select(s => s.SheetName).ToList());
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // Excel parse failed, still return file info
                        return Ok(new {
                            success = true,
                            file = new {
                                attachment.Id, attachment.OriginalName, attachment.FilePath,
                                attachment.FileSize, attachment.UploadedAt
                            },
                            parseError = ex.Message
                        });
                    }
                }

                new AuditService(db).Log(userId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "UPLOAD_FILE", module ?? "HE_THONG",
                    $"Upload file: {attachment.OriginalName} (ID: {attachment.Id})");

                return Ok(new {
                    success = true,
                    file = new {
                        attachment.Id, attachment.OriginalName, attachment.FilePath,
                        attachment.FileSize, attachment.ContentType,
                        attachment.SheetCount, attachment.UploadedAt
                    },
                    excelData = parseResult
                });
            }
        }

        [HttpPost, Route("upload-multiple")]
        [ApiAuthorize(Permission = "FILE_UPLOAD")]
        public IHttpActionResult UploadMultiple()
        {
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count == 0)
                return BadRequest("Không có file nào được tải lên.");

            long maxSize = 52428800; // 50MB per file
            var module = httpRequest.Form["module"];
            var userId = (int)Request.Properties["CurrentUserId"];
            var username = Request.Properties["CurrentUsername"]?.ToString();

            var results = new System.Collections.Generic.List<object>();

            using (var db = new AppDbContext())
            {
                var fileSvc = new FileService(db);
                var excelSvc = new ExcelService();

                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    var file = httpRequest.Files[i];

                    if (file.ContentLength == 0)
                    {
                        results.Add(new { success = false, fileName = file.FileName, error = "File rỗng." });
                        continue;
                    }
                    if (file.ContentLength > maxSize)
                    {
                        results.Add(new { success = false, fileName = file.FileName, error = "File vượt quá 50MB." });
                        continue;
                    }

                    try
                    {
                        var attachment = fileSvc.SaveFile(new HttpPostedFileWrapper(file), userId, module, null, null);

                        ExcelParseResult parseResult = null;
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            try
                            {
                                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                                parseResult = excelSvc.ParseExcel(fullPath);
                                attachment.SheetCount = parseResult.SheetCount;
                                attachment.SheetNames = JsonConvert.SerializeObject(
                                    parseResult.Sheets.Select(s => s.SheetName).ToList());
                                db.SaveChanges();
                            }
                            catch { /* ignore parse error, still save file */ }
                        }

                        new AuditService(db).Log(userId, username,
                            "UPLOAD_FILE", module ?? "HE_THONG",
                            $"Upload file: {attachment.OriginalName} (ID: {attachment.Id})");

                        results.Add(new {
                            success = true,
                            file = new {
                                attachment.Id, attachment.OriginalName, attachment.FilePath,
                                attachment.FileSize, attachment.ContentType,
                                attachment.SheetCount, attachment.UploadedAt
                            },
                            excelData = parseResult
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { success = false, fileName = file.FileName, error = ex.Message });
                    }
                }
            }

            return Ok(new { success = true, results });
        }
        [HttpGet, Route("")]
        [ApiAuthorize(Permission = "FILE_VIEW")]
        public IHttpActionResult GetAll(int page = 1, int pageSize = 20, string module = null, string search = "")
        {
            using (var db = new AppDbContext())
            {
                var q = db.FileAttachments.AsQueryable();
                if (!string.IsNullOrEmpty(module))
                    q = q.Where(f => f.Module == module);
                if (!string.IsNullOrEmpty(search))
                    q = q.Where(f => f.OriginalName.Contains(search));

                var total = q.Count();
                var items = q.OrderByDescending(f => f.UploadedAt)
                    .Skip((page - 1) * pageSize).Take(pageSize)
                    .ToList()
                    .Select(f => new {
                        f.Id, f.OriginalName, f.FilePath, f.FileSize, f.ContentType,
                        f.Module, f.SheetCount, f.Description, f.UploadedAt,
                        uploader = "Admin"
                    }).ToList();

                new AuditService(db).Log((int)Request.Properties["CurrentUserId"],
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "GET_FILES", "FILE", $"Lấy danh sách file (Total: {total})");

                return Ok(new { total, page, pageSize, items });
            }
        }

        [HttpGet, Route("{id:int}/download")]
        public HttpResponseMessage Download(int id)
        {
            using (var db = new AppDbContext())
            {
                var attachment = db.FileAttachments.Find(id);
                if (attachment == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File không tồn tại.");

                var fileSvc = new FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!File.Exists(fullPath))
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File không tìm thấy trên server.");

                var result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(File.OpenRead(fullPath));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(
                    attachment.ContentType ?? "application/octet-stream");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = attachment.OriginalName
                };
                return result;
            }
        }

        [HttpGet, Route("{id:int}/parse")]
        [ApiAuthorize(Permission = "FILE_VIEW")]
        public IHttpActionResult ParseExcel(int id)
        {
            using (var db = new AppDbContext())
            {
                var attachment = db.FileAttachments.Find(id);
                if (attachment == null) return NotFound();

                var fileSvc = new FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!File.Exists(fullPath))
                    return BadRequest("File không tìm thấy trên server.");

                var ext = Path.GetExtension(attachment.OriginalName).ToLower();
                if (ext != ".xlsx" && ext != ".xls")
                    return BadRequest("File không phải định dạng Excel.");

                var excelSvc = new ExcelService();
                var result = excelSvc.ParseExcel(fullPath);
                return Ok(result);
            }
        }

        [HttpDelete, Route("{id:int}")]
        [ApiAuthorize(Permission = "FILE_DELETE")]
        public IHttpActionResult Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var fileSvc = new FileService(db);
                if (!fileSvc.DeleteFile(id)) return NotFound();
                return Ok(new { success = true, message = "Đã xóa file." });
            }
        }
    }
}
