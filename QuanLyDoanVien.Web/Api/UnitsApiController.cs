using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;
using QuanLyDoanVien.Services;
using System.Web.Script.Serialization;

namespace QuanLyDoanVien.Api
{
    [RoutePrefix("api/units")]
    [ApiAuthorize]
    public class UnitsApiController : ApiController
    {
        //DANH SÁCH ĐƠN VỊ
        [HttpGet, Route("")]
        public IHttpActionResult GetAll(int page = 1, int pageSize = 20, string search = "")
        {
            using (var db = new AppDbContext())
            {
                var q = db.Units.Where(u => u.IsActive);
                if (!string.IsNullOrEmpty(search))
                    q = q.Where(u => u.UnitName.Contains(search) || u.UnitCode.Contains(search));

                var total = q.Count();
                var items = q.OrderBy(u => u.UnitName)
                    .Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(u => new {
                        u.Id, u.UnitCode, u.UnitName, u.Description,
                        u.TotalMembers, u.LastImportAt, u.CreatedAt,
                        hasSummary = u.SummaryJson != null && u.SummaryJson != ""
                    }).ToList();

                return Ok(new { total, page, pageSize, items });
            }
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var u = db.Units.Find(id);
                if (u == null) return NotFound();
                return Ok(new {
                    u.Id, u.UnitCode, u.UnitName, u.Description,
                    u.TotalMembers, u.LastImportAt, u.CreatedAt,
                    hasSummary = !string.IsNullOrEmpty(u.SummaryJson)
                });
            }
        }

        [HttpPost, Route("")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult Create([FromBody] Unit req)
        {
            if (string.IsNullOrEmpty(req?.UnitName))
                return BadRequest("Tên đơn vị không được để trống.");

            using (var db = new AppDbContext())
            {
                if (string.IsNullOrEmpty(req.UnitCode))
                    req.UnitCode = "DV" + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (db.Units.Any(u => u.UnitCode == req.UnitCode))
                    return BadRequest("Mã đơn vị đã tồn tại.");

                req.CreatedBy = (int)Request.Properties["CurrentUserId"];
                req.CreatedAt = DateTime.Now;
                req.IsActive  = true;
                db.Units.Add(req);
                db.SaveChanges();

                new AuditService(db).Log(req.CreatedBy.Value,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "CREATE_UNIT", "DON_VI", $"Thêm đơn vị: {req.UnitName}");

                return Ok(new { success = true, id = req.Id, message = "Thêm đơn vị thành công." });
            }
        }

        [HttpPut, Route("{id:int}")]
        [ApiAuthorize(Permission = "DV_EDIT")]
        public IHttpActionResult Update(int id, [FromBody] Unit req)
        {
            using (var db = new AppDbContext())
            {
                var u = db.Units.Find(id);
                if (u == null) return NotFound();

                u.UnitName    = req.UnitName    ?? u.UnitName;
                u.UnitCode    = req.UnitCode    ?? u.UnitCode;
                u.Description = req.Description ?? u.Description;
                u.UpdatedAt   = DateTime.Now;
                db.SaveChanges();

                return Ok(new { success = true, message = "Cập nhật thành công." });
            }
        }

        [HttpDelete, Route("{id:int}")]
        [ApiAuthorize(Permission = "DV_DELETE")]
        public IHttpActionResult Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var u = db.Units.Find(id);
                if (u == null) return NotFound();
                u.IsActive  = false;
                u.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return Ok(new { success = true, message = "Đã xóa đơn vị." });
            }
        }

        [HttpPost, Route("delete-multiple")]
        [ApiAuthorize(Permission = "DV_DELETE")]
        public IHttpActionResult DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Không có ID nào được chọn.");

            using (var db = new AppDbContext())
            {
                var units = db.Units.Where(u => ids.Contains(u.Id) && u.IsActive).ToList();
                if (!units.Any()) return BadRequest("Đơn vị không tồn tại.");

                var now = DateTime.Now;
                foreach (var u in units)
                {
                    u.IsActive  = false;
                    u.UpdatedAt = now;
                }
                db.SaveChanges();

                int userId = (int)Request.Properties["CurrentUserId"];
                new AuditService(db).Log(userId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "DELETE_UNITS", "DON_VI",
                    $"Xóa hàng loạt {units.Count} đơn vị: {string.Join(", ", units.Select(u => u.UnitName))}");

                return Ok(new { success = true, count = units.Count, message = $"Đã xóa {units.Count} đơn vị." });
            }
        }

        //IMPORT EXCEL → TỔNG HỢP 
        [HttpPost, Route("{id:int}/import")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult Import(int id, [FromBody] UnitImportRequest req)
        {
            if (req == null || req.FileId <= 0)
                return BadRequest("Thông tin file không hợp lệ.");

            using (var db = new AppDbContext())
            {
                var unit = db.Units.Find(id);
                if (unit == null) return NotFound();

                var attachment = db.FileAttachments.Find(req.FileId);
                if (attachment == null) return NotFound();

                var fileSvc  = new FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!System.IO.File.Exists(fullPath))
                    return BadRequest("File không tồn tại trên server.");

                var excelSvc = new ExcelService();
                var data     = excelSvc.ReadSheetForImport(fullPath, req.SheetName ?? "Sheet1");

                if (!data.Any())
                    return Ok(new { success = false, count = 0, message = "Không đọc được dữ liệu từ file." });

                var now    = DateTime.Now;
                int userId = (int)Request.Properties["CurrentUserId"];
                var summary = BuildSummary(data, unit.UnitName, now);

                var existingMembers = db.Members.Where(x => x.IsActive).ToList();
                int importedCount = 0;
                
                foreach (var row in data)
                {
                    string fullName = GetVal(row, "Họ và tên", "Họ tên", "Full Name", "HOTEN", "FullName") ?? GetVal(row, "Ho va ten");
                    if (string.IsNullOrEmpty(fullName)) continue;

                    string dobNam = GetVal(row, "Ngày, tháng, năm sinh - Nam");
                    string dobNu = GetVal(row, "Ngày, tháng, năm sinh - Nữ");
                    string dobStr = !string.IsNullOrEmpty(dobNam) ? dobNam : (!string.IsNullOrEmpty(dobNu) ? dobNu : GetVal(row, "Ngày, tháng, năm sinh", "Ngày sinh"));
                    
                    string gender = "";
                    if (!string.IsNullOrEmpty(dobNam) && ParseDate(dobNam) != null) gender = "Nam";
                    else if (!string.IsNullOrEmpty(dobNu) && ParseDate(dobNu) != null) gender = "Nữ";
                    else gender = GetVal(row, "Giới tính") ?? (dobNam != null ? "Nam" : (dobNu != null ? "Nữ" : ""));

                    var dobParsed = ParseDate(dobStr);

                    var existing = existingMembers.FirstOrDefault(x => x.FullName == fullName && x.DateOfBirth == dobParsed);
                    if (existing != null)
                    {
                        existing.UnitId = unit.Id;
                        existing.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        var m = new Member
                        {
                            FullName = fullName,
                            MemberCode = "DV" + unit.Id + "_" + DateTime.Now.ToString("yyMMddHHmmss") + importedCount,
                            Gender = gender,
                            DateOfBirth = dobParsed,
                            Phone = GetVal(row, "Điện thoại", "SĐT", "Phone"),
                            Email = GetVal(row, "Email", "Thư điện tử"),
                            Address = GetVal(row, "Địa chỉ", "Address", "Quê quán"),
                            Ethnicity = GetVal(row, "Dân tộc"),
                            Religion = GetVal(row, "Tôn giáo"),
                            Profession = GetVal(row, "Nghề nghiệp"),
                            Education = GetVal(row, "Học vấn"),
                            Expertise = GetVal(row, "Chuyên môn"),
                            PoliticalTheory = GetVal(row, "Trình độ chính trị", "Lý luận chính trị"),
                            PartyDateProbationary = ParseDate(GetVal(row, "Đoàn viên là đảng viên - Ngày kết nạp dự bị", "Ngày kết nạp dự bị", "Dự bị")),
                            PartyDateOfficial = ParseDate(GetVal(row, "Đoàn viên là đảng viên - Ngày kết nạp chính thức", "Ngày kết nạp chính thức", "Chính thức")),
                            IsActive = true,
                            IsUnionMember = true,
                            CreatedBy = userId,
                            CreatedAt = DateTime.Now,
                            UnitId = unit.Id,
                            GroupId = null 
                        };
                        db.Members.Add(m);
                        existingMembers.Add(m); 
                    }
                    importedCount++;
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                unit.SummaryJson       = js.Serialize(summary);
                unit.TotalMembers      = summary.TotalMembers;
                unit.LastImportFileId  = req.FileId;
                unit.LastImportAt      = now;
                unit.LastImportBy      = userId;
                unit.UpdatedAt         = now;

                db.SaveChanges();

                new AuditService(db).Log(userId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "IMPORT_UNIT", "DON_VI",
                    $"Import {summary.TotalMembers} bản ghi vào đơn vị '{unit.UnitName}' từ file: {attachment.OriginalName}");

                return Ok(new {
                    success      = true,
                    count        = summary.TotalMembers,
                    message      = $"Đã tổng hợp thành công {summary.TotalMembers} bản ghi.",
                    summary
                });
            }
        }

        //IMPORT NHIỀU FILE EXCEL → BÁO CÁO GỘP 
        [HttpPost, Route("{id:int}/import-multiple")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult ImportMultiple(int id, [FromBody] UnitImportMultipleRequest req)
        {
            if (req == null || req.FileIds == null || !req.FileIds.Any())
                return BadRequest("Vui lòng chọn ít nhất 1 file.");

            using (var db = new AppDbContext())
            {
                var unit = db.Units.Find(id);
                if (unit == null) return NotFound();

                var fileSvc  = new FileService(db);
                var excelSvc = new ExcelService();
                var allData  = new List<Dictionary<string, string>>();
                var fileNames = new List<string>();

                foreach (var fileId in req.FileIds)
                {
                    var attachment = db.FileAttachments.Find(fileId);
                    if (attachment == null) continue;

                    var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                    if (!System.IO.File.Exists(fullPath)) continue;

                    var rows = excelSvc.ReadSheetForImport(fullPath, req.SheetName ?? "Sheet1");
                    allData.AddRange(rows);
                    fileNames.Add(attachment.OriginalName);
                }

                if (!allData.Any())
                    return Ok(new { success = false, count = 0, message = "Không đọc được dữ liệu từ các file đã chọn." });

                var now    = DateTime.Now;
                int userId = (int)Request.Properties["CurrentUserId"];
                var summary = BuildSummary(allData, unit.UnitName, now);

                var existingMembers = db.Members.Where(x => x.IsActive).ToList();
                int importedCount = 0;

                foreach (var row in allData)
                {
                    string fullName = GetVal(row, "Họ và tên", "Họ tên", "Full Name", "HOTEN", "FullName") ?? GetVal(row, "Ho va ten");
                    if (string.IsNullOrEmpty(fullName)) continue;

                    string dobNam = GetVal(row, "Ngày, tháng, năm sinh - Nam");
                    string dobNu  = GetVal(row, "Ngày, tháng, năm sinh - Nữ");
                    string dobStr = !string.IsNullOrEmpty(dobNam) ? dobNam : (!string.IsNullOrEmpty(dobNu) ? dobNu : GetVal(row, "Ngày, tháng, năm sinh", "Ngày sinh"));

                    string gender = "";
                    if (!string.IsNullOrEmpty(dobNam) && ParseDate(dobNam) != null) gender = "Nam";
                    else if (!string.IsNullOrEmpty(dobNu) && ParseDate(dobNu) != null) gender = "Nữ";
                    else gender = GetVal(row, "Giới tính") ?? (dobNam != null ? "Nam" : (dobNu != null ? "Nữ" : ""));

                    var dobParsed = ParseDate(dobStr);
                    var existing = existingMembers.FirstOrDefault(x => x.FullName == fullName && x.DateOfBirth == dobParsed);

                    if (existing != null)
                    {
                        existing.UnitId    = unit.Id;
                        existing.UpdatedAt = now;
                    }
                    else
                    {
                        var m = new Member
                        {
                            FullName = fullName,
                            MemberCode = "DV" + unit.Id + "_M" + DateTime.Now.ToString("yyMMddHHmmss") + importedCount,
                            Gender = gender,
                            DateOfBirth = dobParsed,
                            Phone = GetVal(row, "Điện thoại", "SĐT", "Phone"),
                            Email = GetVal(row, "Email", "Thư điện tử"),
                            Address = GetVal(row, "Địa chỉ", "Address", "Quê quán"),
                            Ethnicity = GetVal(row, "Dân tộc"),
                            Religion = GetVal(row, "Tôn giáo"),
                            Profession = GetVal(row, "Nghề nghiệp"),
                            Education = GetVal(row, "Học vấn"),
                            Expertise = GetVal(row, "Chuyên môn"),
                            PoliticalTheory = GetVal(row, "Trình độ chính trị", "Lý luận chính trị"),
                            PartyDateProbationary = ParseDate(GetVal(row, "Đoàn viên là đảng viên - Ngày kết nạp dự bị", "Dự bị")),
                            PartyDateOfficial = ParseDate(GetVal(row, "Đoàn viên là đảng viên - Ngày kết nạp chính thức", "Chính thức")),
                            IsActive = true, IsUnionMember = true,
                            CreatedBy = userId, CreatedAt = now,
                            UnitId = unit.Id, GroupId = null
                        };
                        db.Members.Add(m);
                        existingMembers.Add(m);
                    }
                    importedCount++;
                }

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                unit.SummaryJson      = js.Serialize(summary);
                unit.TotalMembers     = summary.TotalMembers;
                unit.LastImportFileId = req.FileIds.Last();
                unit.LastImportAt     = now;
                unit.LastImportBy     = userId;
                unit.UpdatedAt        = now;
                db.SaveChanges();

                new AuditService(db).Log(userId,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "IMPORT_UNIT_MULTI", "DON_VI",
                    $"Import gộp {req.FileIds.Count} file ({string.Join(", ", fileNames)}) vào '{unit.UnitName}': {summary.TotalMembers} bản ghi.");

                return Ok(new {
                    success = true,
                    count   = summary.TotalMembers,
                    message = $"Đã tổng hợp thành công {summary.TotalMembers} bản ghi từ {req.FileIds.Count} file.",
                    summary
                });
            }
        }

        [HttpGet, Route("{id:int}/summary")]
        public IHttpActionResult GetSummary(int id)
        {
            using (var db = new AppDbContext())
            {
                var unit = db.Units.Find(id);
                if (unit == null) return NotFound();
                if (string.IsNullOrEmpty(unit.SummaryJson))
                    return Ok(new { hasSummary = false });

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var summary = js.Deserialize<UnitSummary>(unit.SummaryJson);
                return Ok(new { hasSummary = true, summary });
            }
        }

        [HttpPost, Route("combined-summary")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult CombinedSummary([FromBody] List<int> unitIds)
        {
            if (unitIds == null || !unitIds.Any())
                return BadRequest("Vui lòng chọn ít nhất 1 đơn vị.");

            using (var db = new AppDbContext())
            {
                var units = db.Units.Where(u => unitIds.Contains(u.Id) && u.IsActive && u.SummaryJson != null && u.SummaryJson != "").ToList();
                if (!units.Any())
                    return Ok(new { hasSummary = false, message = "Các đơn vị đã chọn chưa có dữ liệu tổng hợp." });

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var summaries = units.Select(u => js.Deserialize<UnitSummary>(u.SummaryJson)).ToList();

                // Gộp tất cả summary thành 1
                var combined = new UnitSummary
                {
                    UnitName    = string.Join(" + ", units.Select(u => u.UnitName)),
                    ImportedAt  = DateTime.Now,
                    TotalMembers           = summaries.Sum(s => s.TotalMembers),
                    MaleCount              = summaries.Sum(s => s.MaleCount),
                    FemaleCount            = summaries.Sum(s => s.FemaleCount),
                    EthnicityKinh          = summaries.Sum(s => s.EthnicityKinh),
                    EthnicityOther         = summaries.Sum(s => s.EthnicityOther),
                    PartyProbationaryCount = summaries.Sum(s => s.PartyProbationaryCount),
                    PartyOfficialCount     = summaries.Sum(s => s.PartyOfficialCount),
                    Age18To25              = summaries.Sum(s => s.Age18To25),
                    Age26To30              = summaries.Sum(s => s.Age26To30),
                    Age31Plus              = summaries.Sum(s => s.Age31Plus),
                    CommunistAboveBase     = summaries.Sum(s => s.CommunistAboveBase),
                    CommunistBase          = summaries.Sum(s => s.CommunistBase),

                    Religions     = MergeDicts(summaries.Where(s => s.Religions != null).Select(s => s.Religions)),
                    Professions   = MergeDicts(summaries.Where(s => s.Professions != null).Select(s => s.Professions)),
                    Educations    = MergeDicts(summaries.Where(s => s.Educations != null).Select(s => s.Educations)),
                    Expertises    = MergeDicts(summaries.Where(s => s.Expertises != null).Select(s => s.Expertises)),
                    PoliticalTheories = MergeDicts(summaries.Where(s => s.PoliticalTheories != null).Select(s => s.PoliticalTheories)),
                    PositionRoles = MergeDicts(summaries.Where(s => s.PositionRoles != null).Select(s => s.PositionRoles)),
                };

                return Ok(new {
                    hasSummary  = true,
                    unitCount   = units.Count,
                    unitNames   = units.Select(u => u.UnitName).ToList(),
                    summary     = combined
                });
            }
        }

        // Helper gộp nhiều dictionary cộng dồn
        private static Dictionary<string, int> MergeDicts(IEnumerable<Dictionary<string, int>> dicts)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in dicts)
            {
                foreach (var kv in d)
                {
                    string key = NormalizeKey(kv.Key);
                    result[key] = result.ContainsKey(key) ? result[key] + kv.Value : kv.Value;
                }
            }
            return result;
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            key = key.Trim().Normalize(System.Text.NormalizationForm.FormC);
            if (key.Length > 0)
                key = char.ToUpper(key[0]) + key.Substring(1).ToLower();
            return key;
        }

        [HttpGet, Route("{id:int}/export")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult ExportUnit(int id)
        {
            using (var db = new AppDbContext())
            {
                var unit = db.Units.Find(id);
                if (unit == null) return NotFound();
                if (string.IsNullOrEmpty(unit.SummaryJson))
                    return BadRequest("Đơn vị chưa có dữ liệu. Hãy import Excel trước.");

                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var summary = js.Deserialize<UnitSummary>(unit.SummaryJson);
                var bytes   = BuildExcelForUnit(summary, unit.UnitName, unit.LastImportAt);
                return ExcelResponse(bytes, $"TongHop_{SanitizeFileName(unit.UnitName)}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }


        [HttpGet, Route("export-all")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult ExportAll()
        {
            using (var db = new AppDbContext())
            {
                var units = db.Units.Where(u => u.IsActive && u.SummaryJson != null && u.SummaryJson != "").ToList();
                if (!units.Any()) return BadRequest("Chưa có đơn vị nào có dữ liệu tổng hợp.");

                var js       = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var summaries = units.Select(u => new {
                    unit    = u,
                    summary = js.Deserialize<UnitSummary>(u.SummaryJson)
                }).ToList();

                var bytes = BuildExcelAllUnits(summaries.Select(s => s.summary).ToList());
                return ExcelResponse(bytes, $"TongHopTatCaDonVi_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        //THỐNG KÊ TỔNG (DASHBOARD) 
        [HttpGet, Route("stats")]
        public IHttpActionResult Stats(int? unitId = null)
        {
            using (var db = new AppDbContext())
            {
                var units = db.Units.Where(u => u.IsActive).ToList();
                if (unitId.HasValue)
                {
                    var unit = units.FirstOrDefault(u => u.Id == unitId.Value);
                    if (unit == null || string.IsNullOrEmpty(unit.SummaryJson))
                        return Ok(new { unitId, totalMembers = 0, hasSummary = false });

                    var js      = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    var summary = js.Deserialize<UnitSummary>(unit.SummaryJson);
                    return Ok(new {
                        unitId,
                        unitName     = unit.UnitName,
                        totalMembers = summary.TotalMembers,
                        male         = summary.MaleCount,
                        female       = summary.FemaleCount,
                        kinhEthnicity   = summary.EthnicityKinh,
                        otherEthnicity  = summary.EthnicityOther,
                        partyProbationary = summary.PartyProbationaryCount,
                        partyOfficial     = summary.PartyOfficialCount,
                        age18to25    = summary.Age18To25,
                        age26to30    = summary.Age26To30,
                        age31plus    = summary.Age31Plus,
                        hasSummary   = true,
                        lastImportAt = unit.LastImportAt
                    });
                }
                else
                {
                    int totalAll  = units.Sum(u => u.TotalMembers ?? 0);
                    int unitCount = units.Count;
                    return Ok(new {
                        unitId       = (int?)null,
                        totalMembers = totalAll,
                        unitCount,
                        hasSummary   = true
                    });
                }
            }
        }

        private UnitSummary BuildSummary(List<Dictionary<string, string>> data, string unitName, DateTime importedAt)
        {
            var summary = new UnitSummary
            {
                UnitName   = unitName,
                ImportedAt = importedAt,
                Religions         = new Dictionary<string, int>(),
                PositionRoles     = new Dictionary<string, int> { { "Ban chấp hành", 0 }, { "Ban thường vụ", 0 }, { "Bí thư", 0 }, { "Phó Bí thư", 0 }, { "Cấp trưởng", 0 }, { "Cấp phó", 0 } },
                Professions       = new Dictionary<string, int> { { "Công chức", 0 }, { "Viên chức", 0 }, { "Sinh viên", 0 }, { "Khác", 0 } },
                Educations        = new Dictionary<string, int> { { "THCS", 0 }, { "THPT", 0 }, { "Khác", 0 } },
                Expertises        = new Dictionary<string, int> { { "Tiến sĩ", 0 }, { "Thạc sĩ", 0 }, { "Đại học", 0 }, { "Cao đẳng", 0 }, { "Khác", 0 } },
                PoliticalTheories = new Dictionary<string, int> { { "Sơ cấp", 0 }, { "Trung cấp", 0 }, { "Cao cấp", 0 }, { "Cử nhân", 0 }, { "Khác", 0 } },
                MemberRows        = new List<UnitMemberRow>()
            };

            foreach (var row in data)
            {
                string fullName = GetVal(row, "Họ và tên", "Họ tên", "Full Name", "HOTEN", "FullName");
                if (string.IsNullOrEmpty(fullName)) continue;

                summary.TotalMembers++;

                string dobNam = GetVal(row, "Ngày, tháng, năm sinh - Nam");
                string dobNu  = GetVal(row, "Ngày, tháng, năm sinh - Nữ");
                string dobStr = !string.IsNullOrEmpty(dobNam) ? dobNam
                              : (!string.IsNullOrEmpty(dobNu) ? dobNu
                              : GetVal(row, "Ngày, tháng, năm sinh", "Ngày sinh"));

                string gender = "";
                if (!string.IsNullOrEmpty(dobNam) && ParseDate(dobNam) != null) gender = "Nam";
                else if (!string.IsNullOrEmpty(dobNu)  && ParseDate(dobNu)  != null) gender = "Nữ";
                else gender = GetVal(row, "Giới tính") ?? "";
                if (string.IsNullOrEmpty(gender))
                    gender = dobNam != null ? "Nam" : (dobNu != null ? "Nữ" : "");

                if (gender == "Nam") summary.MaleCount++;
                else if (gender == "Nữ") summary.FemaleCount++;

                var dob = ParseDate(dobStr);
                int? age = null;
                if (dob.HasValue)
                {
                    var today = DateTime.Today;
                    age = today.Year - dob.Value.Year;
                    if (dob.Value.AddYears(age.Value) > today) age--;
                    if (age >= 18 && age <= 25) summary.Age18To25++;
                    else if (age >= 26 && age <= 30) summary.Age26To30++;
                    else if (age >= 31) summary.Age31Plus++;
                }

                string eth = GetVal(row, "Dân tộc");
                if (string.IsNullOrEmpty(eth) || IsChecked(eth) || 
                    eth.ToLower().Contains("kinh")) summary.EthnicityKinh++;
                else summary.EthnicityOther++;

                string rel = GetVal(row, "Tôn giáo") ?? "Không";
                if (IsChecked(rel)) rel = "Không";
                IncrementDict(summary.Religions, rel);

                var partyProb = ParseDate(GetVal(row,
                    "Đoàn viên là đảng viên - Ngày kết nạp dự bị",
                    "Ngày kết nạp dự bị", "Dự bị"));
                var partyOff  = ParseDate(GetVal(row,
                    "Đoàn viên là đảng viên - Ngày kết nạp chính thức",
                    "Ngày kết nạp chính thức", "Chính thức"));
                if (partyProb.HasValue) summary.PartyProbationaryCount++;
                if (partyOff.HasValue)  summary.PartyOfficialCount++;

                string MapCat(string inp, string[] words, string ret) {
                    if (string.IsNullOrEmpty(inp)) return null;
                    var lw = inp.ToLower();
                    foreach (var w in words) if (lw.Contains(w.ToLower())) return ret;
                    return null;
                }

                string rawProf = GetVal(row, "Nghề nghiệp");
                string mProf = MapCat(rawProf, new[]{"công chức"}, "Công chức") ?? MapCat(rawProf, new[]{"viên chức"}, "Viên chức") ?? MapCat(rawProf, new[]{"sinh viên", "học sinh"}, "Sinh viên") ?? "Khác";
                summary.Professions[mProf]++;

                string rawEdu = GetVal(row, "Học vấn");
                string mEdu = MapCat(rawEdu, new[]{"thcs", "cấp 2"}, "THCS") ?? MapCat(rawEdu, new[]{"thpt", "cấp 3"}, "THPT") ?? "Khác";
                summary.Educations[mEdu]++;

                string rawExp = GetVal(row, "Chuyên môn", "Trình độ chuyên môn");
                string mExp = MapCat(rawExp, new[]{"tiến sĩ", "ts", "tiến sỹ"}, "Tiến sĩ") ?? MapCat(rawExp, new[]{"thạc sĩ", "thạc sỹ", "ths"}, "Thạc sĩ") ?? MapCat(rawExp, new[]{"đại học", "đh"}, "Đại học") ?? MapCat(rawExp, new[]{"cao đẳng", "cđ"}, "Cao đẳng") ?? "Khác";
                summary.Expertises[mExp]++;

                string rawPol = GetVal(row, "Trình độ chính trị", "Lý luận chính trị");
                string mPol = MapCat(rawPol, new[]{"cử nhân"}, "Cử nhân") ?? MapCat(rawPol, new[]{"cao cấp"}, "Cao cấp") ?? MapCat(rawPol, new[]{"trung cấp"}, "Trung cấp") ?? MapCat(rawPol, new[]{"sơ cấp"}, "Sơ cấp") ?? "Khác";
                summary.PoliticalTheories[mPol]++;

                bool comAboveBase = IsChecked(GetVal(row, "Tham gia cấp ủy cấp trên cơ sở"));
                bool comBase = IsChecked(GetVal(row, "Tham gia cấp ủy cơ sở"));
                if (comAboveBase) summary.CommunistAboveBase++;
                if (comBase) summary.CommunistBase++;

                var roles = new List<string>();
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban chấp hành",  "Ban chấp hành", roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban thường vụ",   "Ban thường vụ", roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Bí thư",          "Bí thư", roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Phó Bí thư",      "Phó Bí thư", roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Chuyên môn",      "Cấp trưởng", roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt",                   "Cấp phó",    roles, summary.PositionRoles);

                summary.MemberRows.Add(new UnitMemberRow
                {
                    FullName            = fullName,
                    Gender              = gender,
                    DateOfBirth         = dob,
                    Age                 = age,
                    Ethnicity           = eth,
                    Religion            = rel,
                    Profession          = mProf,
                    Education           = mEdu,
                    Expertise           = mExp,
                    PoliticalTheory     = mPol,
                    PartyDateProbationary = partyProb,
                    PartyDateOfficial   = partyOff,
                    CommunistAboveBase  = comAboveBase,
                    CommunistBase       = comBase,
                    PositionRoles       = string.Join(", ", roles)
                });
            }

            return summary;
        }

        private void CheckRole(Dictionary<string, string> row, string key, string label,
            List<string> roles, Dictionary<string, int> posDict)
        {
            var val = GetVal(row, key);
            if (IsChecked(val))
            {
                roles.Add(label);
                IncrementDict(posDict, label);
            }
        }

        private byte[] BuildExcelForUnit(UnitSummary summary, string unitName, DateTime? importedAt)
        {
            using (var pkg = new OfficeOpenXml.ExcelPackage())
            {
                var wsSummary = pkg.Workbook.Worksheets.Add("Tổng hợp");
                WriteUnitSummarySheet(wsSummary, summary, unitName, importedAt);
                var wsDetail = pkg.Workbook.Worksheets.Add("Chi tiết");
                WriteUnitDetailSheet(wsDetail, summary);
                return pkg.GetAsByteArray();
            }
        }



        private void WriteUnitSummarySheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary, string unitName, DateTime? importedAt)
        {
            ws.Cells[1, 1].Value = $"TỔNG HỢP ĐƠN VỊ: {unitName.ToUpper()}";
            ws.Cells[1, 1, 1, 3].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            ws.Cells[2, 1].Value = $"Ngày tổng hợp: {(importedAt ?? DateTime.Now):dd/MM/yyyy HH:mm}";
            ws.Cells[2, 1, 2, 3].Merge = true;

            int r = 4;
            void Row(string label, object val)
            {
                ws.Cells[r, 1].Value = label; 
                ws.Cells[r, 1].Style.Font.Bold = true;
                ws.Cells[r, 2].Value = val;
                if (val is int num && num > 0)
                    ws.Cells[r, 2].Style.Font.Bold = true;
                r++;
            }
            void SubRow(string label, object val)
            {
                ws.Cells[r, 1].Value = $"    - {label}";
                ws.Cells[r, 2].Value = val;
                r++;
            }

            Row("Tổng số đoàn viên", summary.TotalMembers);
            Row("Nam", summary.MaleCount);
            Row("Nữ", summary.FemaleCount);
            r++;
            Row("Dân tộc Kinh", summary.EthnicityKinh);
            Row("Dân tộc Khác", summary.EthnicityOther);
            r++;
            Row("Tôn giáo", "");
            foreach (var kv in summary.Religions) SubRow(kv.Key, kv.Value);
            r++;
            Row("Đoàn viên là đảng viên (Dự bị)", summary.PartyProbationaryCount);
            Row("Đoàn viên là đảng viên (CT)", summary.PartyOfficialCount);
            r++;
            Row("Độ tuổi 18-25", summary.Age18To25);
            Row("Độ tuổi 26-30", summary.Age26To30);
            Row("Độ tuổi 31 trở lên", summary.Age31Plus);
            r++;
            Row("Nghề nghiệp", "");
            foreach (var kv in new[] { "Công chức", "Viên chức", "Sinh viên", "Khác" }) 
                SubRow(kv, summary.Professions != null && summary.Professions.ContainsKey(kv) ? summary.Professions[kv] : 0);
            r++;
            Row("Học vấn", "");
            foreach (var kv in new[] { "THCS", "THPT", "Khác" }) 
                SubRow(kv, summary.Educations != null && summary.Educations.ContainsKey(kv) ? summary.Educations[kv] : 0);
            r++;
            Row("Chuyên môn", "");
            foreach (var kv in new[] { "Tiến sĩ", "Thạc sĩ", "Đại học", "Cao đẳng", "Khác" }) 
                SubRow(kv, summary.Expertises != null && summary.Expertises.ContainsKey(kv) ? summary.Expertises[kv] : 0);
            r++;
            Row("Lý luận chính trị", "");
            foreach (var kv in new[] { "Sơ cấp", "Trung cấp", "Cao cấp", "Cử nhân", "Khác" }) 
                SubRow(kv, summary.PoliticalTheories != null && summary.PoliticalTheories.ContainsKey(kv) ? summary.PoliticalTheories[kv] : 0);
            r++;
            Row("Tham gia cấp ủy cấp trên cơ sở", summary.CommunistAboveBase);
            Row("Tham gia cấp ủy cơ sở", summary.CommunistBase);
            r++;
            Row("Chức vụ chủ chốt", "");
            foreach (var kv in new[] { "Ban chấp hành", "Ban thường vụ", "Bí thư", "Phó Bí thư", "Cấp trưởng", "Cấp phó" }) 
                SubRow(kv, summary.PositionRoles != null && summary.PositionRoles.ContainsKey(kv) ? summary.PositionRoles[kv] : 0);

            using (var range = ws.Cells[4, 1, r - 1, 2])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }
            
            ws.Column(1).Width = 45;
            ws.Column(2).Width = 15;
            ws.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        private void WriteUnitDetailSheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary)
        {
            string[] headers = {
                "STT","Họ và tên","Giới tính","Ngày sinh","Tuổi","Dân tộc","Tôn giáo",
                "Nghề nghiệp","Học vấn","Chuyên môn","Lý luận chính trị",
                "Đảng viên DB","Đảng viên CT",
                "Tuổi 18-25","Tuổi 26-30","Tuổi 31+",
                "Cấp ủy cấp trên","Cấp ủy cơ sở",
                "Chức vụ chủ chốt"
            };
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cells[1, c + 1].Value = headers[c];
                ws.Cells[1, c + 1].Style.Font.Bold = true;
                ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
                ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = 2;
            foreach (var m in summary.MemberRows)
            {
                int age = m.Age ?? 0;
                ws.Cells[row, 1].Value  = row - 1;
                ws.Cells[row, 2].Value  = m.FullName;
                ws.Cells[row, 3].Value  = m.Gender;
                ws.Cells[row, 4].Value  = m.DateOfBirth?.ToString("dd/MM/yyyy");
                ws.Cells[row, 5].Value  = m.Age;
                ws.Cells[row, 6].Value  = m.Ethnicity;
                ws.Cells[row, 7].Value  = m.Religion;
                ws.Cells[row, 8].Value  = m.Profession;
                ws.Cells[row, 9].Value  = m.Education;
                ws.Cells[row, 10].Value = m.Expertise;
                ws.Cells[row, 11].Value = m.PoliticalTheory;
                ws.Cells[row, 12].Value = m.PartyDateProbationary?.ToString("dd/MM/yyyy");
                ws.Cells[row, 13].Value = m.PartyDateOfficial?.ToString("dd/MM/yyyy");
                ws.Cells[row, 14].Value = (age >= 18 && age <= 25) ? "x" : "";
                ws.Cells[row, 15].Value = (age >= 26 && age <= 30) ? "x" : "";
                ws.Cells[row, 16].Value = (age >= 31)              ? "x" : "";
                ws.Cells[row, 17].Value = m.CommunistAboveBase ? "x" : "";
                ws.Cells[row, 18].Value = m.CommunistBase      ? "x" : "";
                ws.Cells[row, 19].Value = m.PositionRoles;
                row++;
            }
            ws.Cells.AutoFitColumns();
        }

        private byte[] BuildExcelAllUnits(List<UnitSummary> summaries)
        {
            using (var pkg = new OfficeOpenXml.ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Tổng hợp tất cả đơn vị");

                string[] headers = {
                    "STT","Tên đơn vị","Tổng số","Nam","Nữ","DT Kinh","DT Khác",
                    "Tôn giáo","ĐV-ĐV DB","ĐV-ĐV CT",
                    "Tuổi 18-25","Tuổi 26-30","Tuổi 31+",
                    "Nghề nghiệp","Học vấn","Chuyên môn",
                    "CU Cấp trên","CU Cơ sở",
                    "BCH","BTV","Bí thư","Phó BT","Cấp trưởng","Cấp phó"
                };

                for (int c = 0; c < headers.Length; c++)
                {
                    ws.Cells[1, c + 1].Value = headers[c];
                    ws.Cells[1, c + 1].Style.Font.Bold = true;
                    ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
                    ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                int row = 2;
                for (int i = 0; i < summaries.Count; i++)
                {
                    var s = summaries[i];
                    string religions = s.Religions != null
                        ? string.Join("; ", s.Religions.Select(x => $"{x.Key}: {x.Value}"))
                        : "";
                    string professions = s.Professions != null
                        ? string.Join("; ", s.Professions.Select(x => $"{x.Key}: {x.Value}"))
                        : "";
                    string educations = s.Educations != null
                        ? string.Join("; ", s.Educations.Select(x => $"{x.Key}: {x.Value}"))
                        : "";
                    string expertises = s.Expertises != null
                        ? string.Join("; ", s.Expertises.Select(x => $"{x.Key}: {x.Value}"))
                        : "";

                    ws.Cells[row, 1].Value  = i + 1;
                    ws.Cells[row, 2].Value  = s.UnitName;
                    ws.Cells[row, 3].Value  = s.TotalMembers;
                    ws.Cells[row, 4].Value  = s.MaleCount;
                    ws.Cells[row, 5].Value  = s.FemaleCount;
                    ws.Cells[row, 6].Value  = s.EthnicityKinh;
                    ws.Cells[row, 7].Value  = s.EthnicityOther;
                    ws.Cells[row, 8].Value  = religions;
                    ws.Cells[row, 9].Value  = s.PartyProbationaryCount;
                    ws.Cells[row, 10].Value = s.PartyOfficialCount;
                    ws.Cells[row, 11].Value = s.Age18To25;
                    ws.Cells[row, 12].Value = s.Age26To30;
                    ws.Cells[row, 13].Value = s.Age31Plus;
                    ws.Cells[row, 14].Value = professions;
                    ws.Cells[row, 15].Value = educations;
                    ws.Cells[row, 16].Value = expertises;
                    ws.Cells[row, 17].Value = s.CommunistAboveBase;
                    ws.Cells[row, 18].Value = s.CommunistBase;
                    ws.Cells[row, 19].Value = GetDictVal(s.PositionRoles, "Ban chấp hành");
                    ws.Cells[row, 20].Value = GetDictVal(s.PositionRoles, "Ban thường vụ");
                    ws.Cells[row, 21].Value = GetDictVal(s.PositionRoles, "Bí thư");
                    ws.Cells[row, 22].Value = GetDictVal(s.PositionRoles, "Phó Bí thư");
                    ws.Cells[row, 23].Value = GetDictVal(s.PositionRoles, "Cấp trưởng");
                    ws.Cells[row, 24].Value = GetDictVal(s.PositionRoles, "Cấp phó");
                    row++;
                }

                // Dòng tổng
                ws.Cells[row, 1].Value = "TỔNG";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 3].Value  = summaries.Sum(s => s.TotalMembers);
                ws.Cells[row, 4].Value  = summaries.Sum(s => s.MaleCount);
                ws.Cells[row, 5].Value  = summaries.Sum(s => s.FemaleCount);
                ws.Cells[row, 6].Value  = summaries.Sum(s => s.EthnicityKinh);
                ws.Cells[row, 7].Value  = summaries.Sum(s => s.EthnicityOther);
                ws.Cells[row, 9].Value  = summaries.Sum(s => s.PartyProbationaryCount);
                ws.Cells[row, 10].Value = summaries.Sum(s => s.PartyOfficialCount);
                ws.Cells[row, 11].Value = summaries.Sum(s => s.Age18To25);
                ws.Cells[row, 12].Value = summaries.Sum(s => s.Age26To30);
                ws.Cells[row, 13].Value = summaries.Sum(s => s.Age31Plus);
                ws.Cells[row, 17].Value = summaries.Sum(s => s.CommunistAboveBase);
                ws.Cells[row, 18].Value = summaries.Sum(s => s.CommunistBase);
                for (int c = 3; c <= 24; c++)
                    ws.Cells[row, c].Style.Font.Bold = true;

                ws.Cells.AutoFitColumns();
                return pkg.GetAsByteArray();
            }
        }

        private System.Web.Http.IHttpActionResult ExcelResponse(byte[] bytes, string fileName)
        {
            var result = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = fileName };
            return ResponseMessage(result);
        }

        private int GetDictVal(Dictionary<string, int> dict, string key)
        {
            if (dict == null) return 0;
            return dict.ContainsKey(key) ? dict[key] : 0;
        }

        private void IncrementDict(Dictionary<string, int> dict, string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            key = NormalizeKey(key);
            if (dict.ContainsKey(key)) dict[key]++;
            else dict[key] = 1;
        }

        private bool IsChecked(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            var v = val.ToLower().Trim();
            return v == "x" || v == "✓" || v == "✔" || v == "v" || v == "1" || v == "true" || v == "có";
        }

        private DateTime? ParseDate(string val)
        {
            if (string.IsNullOrEmpty(val) || IsChecked(val)) return null;
            DateTime dt;
            string[] fmts = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(val, fmts, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt)) return dt;
            if (DateTime.TryParse(val, out dt)) return dt;
            return null;
        }

        private string GetVal(Dictionary<string, string> row, params string[] keys)
        {
            foreach (var k in keys)
            {
                var normK = System.Text.RegularExpressions.Regex.Replace(
                    k, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();

                var match = row.Keys.FirstOrDefault(x => {
                    var normX = System.Text.RegularExpressions.Regex.Replace(
                        x, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                    return normX.Equals(normK);
                });
                if (match != null) return row[match]?.Trim();

                if (normK.Length > 3)
                {
                    match = row.Keys.FirstOrDefault(x => {
                        var normX = System.Text.RegularExpressions.Regex.Replace(
                            x, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                        return normX.Contains(normK) || normK.Contains(normX);
                    });
                    if (match != null) return row[match]?.Trim();
                }
            }
            return null;
        }

        private string SanitizeFileName(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-]", "_");
        }
    }
    public class UnitImportRequest
    {
        public int    FileId    { get; set; }
        public string SheetName { get; set; }
    }

    public class UnitImportMultipleRequest
    {
        public List<int> FileIds   { get; set; }
        public string    SheetName { get; set; }
    }

    public class UnitSummary
    {
        public string   UnitName    { get; set; }
        public DateTime ImportedAt  { get; set; }
        public int TotalMembers           { get; set; }
        public int MaleCount              { get; set; }
        public int FemaleCount            { get; set; }
        public int EthnicityKinh          { get; set; }
        public int EthnicityOther         { get; set; }
        public int PartyProbationaryCount { get; set; }
        public int PartyOfficialCount     { get; set; }
        public int Age18To25              { get; set; }
        public int Age26To30              { get; set; }
        public int Age31Plus              { get; set; }
        public int CommunistAboveBase     { get; set; }
        public int CommunistBase          { get; set; }
        public Dictionary<string, int>  Religions     { get; set; }
        public Dictionary<string, int>  Professions   { get; set; }
        public Dictionary<string, int>  Educations    { get; set; }
        public Dictionary<string, int>  Expertises    { get; set; }
        public Dictionary<string, int>  PoliticalTheories { get; set; }
        public Dictionary<string, int>  PositionRoles { get; set; }
        public List<UnitMemberRow>      MemberRows    { get; set; }
    }

    public class UnitMemberRow
    {
        public string    FullName              { get; set; }
        public string    Gender                { get; set; }
        public DateTime? DateOfBirth           { get; set; }
        public int?      Age                   { get; set; }
        public string    Ethnicity             { get; set; }
        public string    Religion              { get; set; }
        public string    Profession            { get; set; }
        public string    Education             { get; set; }
        public string    Expertise             { get; set; }
        public string    PoliticalTheory       { get; set; }
        public DateTime? PartyDateProbationary { get; set; }
        public DateTime? PartyDateOfficial     { get; set; }
        public bool      CommunistAboveBase    { get; set; }
        public bool      CommunistBase         { get; set; }
        public string    PositionRoles         { get; set; }
    }
}



