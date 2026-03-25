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
                return BadRequest("KhÃ´ng cÃ³ file nÃ o Ä‘Æ°á»£c táº£i lÃªn.");

            var file = httpRequest.Files[0];
            if (file.ContentLength == 0)
                return BadRequest("File rá»—ng.");

            long maxSize = 52428800; // 50MB
            if (file.ContentLength > maxSize)
                return BadRequest("File vÆ°á»£t quÃ¡ dung lÆ°á»£ng cho phÃ©p (50MB).");

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
                    "GET_FILES", "FILE", $"Láº¥y danh sÃ¡ch file (Total: {total})");

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
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File khÃ´ng tá»“n táº¡i.");

                var fileSvc = new FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!File.Exists(fullPath))
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File khÃ´ng tÃ¬m tháº¥y trÃªn server.");

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
                    return BadRequest("File khÃ´ng tÃ¬m tháº¥y trÃªn server.");

                var ext = Path.GetExtension(attachment.OriginalName).ToLower();
                if (ext != ".xlsx" && ext != ".xls")
                    return BadRequest("File khÃ´ng pháº£i Ä‘á»‹nh dáº¡ng Excel.");

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
                return Ok(new { success = true, message = "ÄÃ£ xÃ³a file." });
            }
        }
    }
}

