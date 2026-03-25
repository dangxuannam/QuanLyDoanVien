using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using QuanLyDauTu.Filters;
using QuanLyDauTu.Models;
using QuanLyDauTu.Models.Entities;
using QuanLyDauTu.Services;

namespace QuanLyDauTu.Api
{
    [RoutePrefix("api/members")]
    [ApiAuthorize]
    public class MembersApiController : ApiController
    {
        [HttpGet, Route("")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult GetAll(int page = 1, int pageSize = 20,
            string search = "", int? groupId = null, bool? isActive = null)
        {
            using (var db = new AppDbContext())
            {
                var q = db.Members.AsQueryable();
                if (isActive == null) q = q.Where(m => m.IsActive);
                else q = q.Where(m => m.IsActive == isActive.Value);

                if (!string.IsNullOrEmpty(search))
                    q = q.Where(m => m.FullName.Contains(search) || m.MemberCode.Contains(search) || m.Phone.Contains(search));

                var total = q.Count();
                var items = q.OrderBy(m => m.FullName)
                    .Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new {
                        m.Id, m.MemberCode, m.FullName, m.DateOfBirth, m.Gender,
                        m.Phone, m.Email, m.JoinDate, m.Position, m.IsActive, m.IsUnionMember,
                        m.Ethnicity, m.Religion, m.Profession, m.Education, m.Expertise, m.PoliticalTheory,
                        m.PartyDateProbationary, m.PartyDateOfficial, m.Notes,
                        groupName = db.MemberGroups.Where(g => g.Id == m.GroupId)
                            .Select(g => g.GroupName).FirstOrDefault()
                    }).ToList();

                return Ok(new { total, page, pageSize, items });
            }
        }

        [HttpGet, Route("{id:int}")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                var m = db.Members.Find(id);
                if (m == null) return NotFound();
                var group = m.GroupId.HasValue ? db.MemberGroups.Find(m.GroupId.Value) : null;
                return Ok(new {
                    m.Id, m.MemberCode, m.FullName, m.DateOfBirth, m.Gender,
                    m.Phone, m.Email, m.Address, m.JoinDate, m.GroupId, m.Position,
                    m.CardNumber, m.IsActive, m.IsUnionMember, m.Notes, m.CreatedAt,
                    m.Ethnicity, m.Religion, m.Profession, m.Education, m.Expertise, m.PoliticalTheory,
                    m.IdentityNumber, m.HealthStatus,
                    m.PartyDateProbationary, m.PartyDateOfficial,
                    groupName = group?.GroupName
                });
            }
        }

        [HttpPost, Route("")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult Create([FromBody] Member req)
        {
            if (string.IsNullOrEmpty(req?.FullName))
                return BadRequest("Họ tên đoàn viên không được để trống.");

            using (var db = new AppDbContext())
            {
                if (string.IsNullOrEmpty(req.MemberCode))
                    req.MemberCode = "DV" + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (db.Members.Any(m => m.MemberCode == req.MemberCode))
                    return BadRequest("Mã đoàn viên đã tồn tại.");

                req.CreatedBy = (int)Request.Properties["CurrentUserId"];
                req.CreatedAt = DateTime.Now;
                req.IsActive = true;
                db.Members.Add(req);
                db.SaveChanges();

                new AuditService(db).Log(req.CreatedBy,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "CREATE_MEMBER", "DOAN_VIEN", $"Thêm đoàn viên: {req.FullName}");

                return Ok(new { success = true, id = req.Id, message = "Thêm đoàn viên thành công." });
            }
        }

        [HttpPut, Route("{id:int}")]
        [ApiAuthorize(Permission = "DV_EDIT")]
        public IHttpActionResult Update(int id, [FromBody] Member req)
        {
            using (var db = new AppDbContext())
            {
                var m = db.Members.Find(id);
                if (m == null) return NotFound();

                m.FullName = req.FullName ?? m.FullName;
                m.DateOfBirth = req.DateOfBirth ?? m.DateOfBirth;
                m.Gender = req.Gender ?? m.Gender;
                m.Address = req.Address ?? m.Address;
                m.JoinDate = req.JoinDate ?? m.JoinDate;
                m.GroupId = req.GroupId ?? m.GroupId;
                m.Position = req.Position ?? m.Position;
                m.CardNumber = req.CardNumber ?? m.CardNumber;
                m.IsActive = req.IsActive;
                m.IsUnionMember = req.IsUnionMember;
                m.Notes = req.Notes ?? m.Notes;
                // Bổ sung các trường mới
                m.Ethnicity = req.Ethnicity ?? m.Ethnicity;
                m.Religion = req.Religion ?? m.Religion;
                m.Profession = req.Profession ?? m.Profession;
                m.Education = req.Education ?? m.Education;
                m.Expertise = req.Expertise ?? m.Expertise;
                m.PoliticalTheory = req.PoliticalTheory ?? m.PoliticalTheory;
                m.UpdatedAt = DateTime.Now;
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
                var m = db.Members.Find(id);
                if (m == null) return NotFound();
                m.IsActive = false;
                m.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return Ok(new { success = true, message = "Đã xóa đoàn viên." });
            }
        }

        [HttpPost, Route("delete-multiple")]
        [ApiAuthorize(Permission = "DV_DELETE")]
        public IHttpActionResult DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID trống.");
            try
            {
                using (var db = new AppDbContext())
                {
                    var members = db.Members.Where(m => ids.Contains(m.Id)).ToList();
                    foreach (var m in members)
                    {
                        m.IsActive = false;
                        m.UpdatedAt = DateTime.Now;
                    }
                    db.SaveChanges();

                    var currentUserId = (int)Request.Properties["CurrentUserId"];
                    var currentUsername = Request.Properties["CurrentUsername"]?.ToString();
                    new AuditService(db).Log(currentUserId, currentUsername, 
                        "DELETE_MEMBER", "DOAN_VIEN", $"Xóa {members.Count} đoàn viên.");

                    return Ok(new { success = true, message = $"Đã xóa {members.Count} đoàn viên." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("delete-all")]
        [ApiAuthorize(Permission = "DV_DELETE")]
        public IHttpActionResult DeleteAll()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var members = db.Members.Where(m => m.IsActive).ToList();
                    int count = members.Count;
                    foreach (var m in members)
                    {
                        m.IsActive = false;
                        m.UpdatedAt = DateTime.Now;
                    }
                    db.SaveChanges();

                    var currentUserId = (int)Request.Properties["CurrentUserId"];
                    var currentUsername = Request.Properties["CurrentUsername"]?.ToString();
                    new AuditService(db).Log(currentUserId, currentUsername, 
                        "DELETE_ALL_MEMBERS", "DOAN_VIEN", "Xóa toàn bộ danh sách đoàn viên.");

                    return Ok(new { success = true, message = $"Đã xóa toàn bộ {count} đoàn viên." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route("groups")]
        public IHttpActionResult GetGroups()
        {
            using (var db = new AppDbContext())
            {
                var groups = db.MemberGroups.Where(g => g.IsActive)
                    .Select(g => new { g.Id, g.GroupCode, g.GroupName, g.Description })
                    .ToList();
                var counts = db.Members.Where(m => m.IsActive && m.GroupId.HasValue)
                    .GroupBy(m => m.GroupId)
                    .Select(g => new { groupId = g.Key, count = g.Count() }).ToList();

                var result = groups.Select(g => new {
                    g.Id, g.GroupCode, g.GroupName, g.Description,
                    memberCount = counts.FirstOrDefault(c => c.groupId == g.Id)?.count ?? 0
                }).ToList();
                return Ok(result);
            }
        }

        [HttpPost, Route("groups")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult CreateGroup([FromBody] MemberGroup req)
        {
            if (string.IsNullOrEmpty(req?.GroupName)) return BadRequest("Tên nhóm không được để trống.");
            using (var db = new AppDbContext())
            {
                if (string.IsNullOrEmpty(req.GroupCode))
                    req.GroupCode = "GR" + DateTime.Now.ToString("yyyyMMddHHmmss");
                req.IsActive = true;
                req.CreatedAt = DateTime.Now;
                db.MemberGroups.Add(req);
                db.SaveChanges();
                return Ok(new { success = true, id = req.Id });
            }
        }

        [HttpGet, Route("stats")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult Stats()
        {
            using (var db = new AppDbContext())
            {
                var total = db.Members.Count(m => m.IsActive);
                var byGender = db.Members.Where(m => m.IsActive)
                    .GroupBy(m => m.Gender)
                    .Select(g => new { gender = g.Key, count = g.Count() }).ToList();
                var byGroup = db.Members.Where(m => m.IsActive && m.GroupId.HasValue)
                    .GroupBy(m => m.GroupId)
                    .Select(g => new { groupId = g.Key, count = g.Count() }).ToList();
                return Ok(new { total, byGender, byGroup });
            }
        }

        [HttpGet, Route("export")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult ExportExcel(string search = "")
        {
            using (var db = new AppDbContext())
            {
                var query = db.Members.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(m => (m.FullName != null && m.FullName.Contains(search)) || (m.Phone != null && m.Phone.Contains(search)));

                var allData = query.Where(m => m.IsActive).OrderBy(m => m.FullName).ToList();
                // Deduplicate by FullName + DateOfBirth
                var seen = new HashSet<string>();
                var data = allData.Where(m => seen.Add(m.FullName + "_" + m.DateOfBirth?.ToString("yyyyMMdd"))).ToList();

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Danh sách đoàn viên");
                    string[] headers = { 
                        "STT", "Họ và tên", "Ngày sinh", "Phân loại", "Dân tộc", "Tôn giáo",
                        "Nghề nghiệp", "Học vấn", "Chuyên môn", "Lý luận chính trị",
                        "Ngày vào Đảng DB", "Ngày vào Đảng CT", "Chức vụ", "SĐT", "Email"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[1, i + 1].Value = headers[i];
                        ws.Cells[1, i + 1].Style.Font.Bold = true;
                        ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    for (int i = 0; i < data.Count; i++)
                    {
                        var m = data[i];
                        int r = i + 2;
                        ws.Cells[r, 1].Value = i + 1;
                        ws.Cells[r, 2].Value = m.FullName;
                        ws.Cells[r, 3].Value = m.DateOfBirth?.ToString("dd/MM/yyyy");
                        ws.Cells[r, 4].Value = m.PartyDateOfficial.HasValue ? "Đảng viên" : (m.IsUnionMember ? "Đoàn viên" : "Thanh niên");
                        ws.Cells[r, 5].Value = m.Ethnicity;
                        ws.Cells[r, 6].Value = m.Religion;
                        ws.Cells[r, 7].Value = m.Profession;
                        ws.Cells[r, 8].Value = m.Education;
                        ws.Cells[r, 9].Value = m.Expertise;
                        ws.Cells[r, 10].Value = m.PoliticalTheory;
                        ws.Cells[r, 11].Value = m.PartyDateProbationary?.ToString("dd/MM/yyyy");
                        ws.Cells[r, 12].Value = m.PartyDateOfficial?.ToString("dd/MM/yyyy");
                        ws.Cells[r, 13].Value = m.Position;
                        ws.Cells[r, 14].Value = m.Phone;
                        ws.Cells[r, 15].Value = m.Email;
                    }

                    ws.Cells.AutoFitColumns();
                    
                    var stream = new System.IO.MemoryStream(package.GetAsByteArray());
                    var result = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.ByteArrayContent(stream.ToArray())
                    };
                    result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "DanhSachDoanVien_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
                    };
                    return ResponseMessage(result);
                }
            }
        }

        [HttpPost, Route("import")]
        [ApiAuthorize(Permission = "DV_CREATE")]
        public IHttpActionResult Import([FromBody] ImportRequest req)
        {
            if (req == null || req.FileId <= 0) return BadRequest("Thông tin file không hợp lệ.");

            using (var db = new AppDbContext())
            {
                var attachment = db.FileAttachments.Find(req.FileId);
                if (attachment == null) return NotFound();

                var fileSvc = new Services.FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!System.IO.File.Exists(fullPath)) return BadRequest("File không tồn tại trên server.");

                var excelSvc = new Services.ExcelService();
                var data = excelSvc.ReadSheetForImport(fullPath, req.SheetName ?? "Sheet1");
                
                if (data.Any())
                {
                    var debugPath = System.IO.Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/uploads"), "headers_debug.txt");
                    System.IO.File.WriteAllLines(debugPath, data.First().Keys);
                }

                int count = 0;
                var userId = (int)Request.Properties["CurrentUserId"];
                var username = Request.Properties["CurrentUsername"]?.ToString();

                var existing = db.Members.Where(x => x.IsActive).Select(x => new { x.FullName, x.DateOfBirth, x.Phone }).ToList();

                foreach (var row in data)
                {
                    // ── Họ và tên 
                    string fullName = GetValue(row, "Họ và tên", "Họ tên", "Full Name", "HOTEN", "FullName", "Tên đoàn viên");
                    if (string.IsNullOrEmpty(fullName)) continue;

                    // ── Ngày sinh/ Nam/Nữ 
                    string dobNamStr = GetValue(row, "Ngày, tháng, năm sinh - Nam");
                    string dobNuStr = GetValue(row, "Ngày, tháng, năm sinh - Nữ");
                    string dobChungStr = GetValue(row, "Ngày, tháng, năm sinh", "Ngày tháng năm sinh", "Ngày sinh", "Birthday");

                    string dobStr = !string.IsNullOrEmpty(dobNamStr) ? dobNamStr : 
                                   (!string.IsNullOrEmpty(dobNuStr) ? dobNuStr : dobChungStr);
                    var dob = ParseDate(dobStr);

                    // ── Giới tính 
                    string gender = "";
                    if (!string.IsNullOrEmpty(dobNamStr) && ParseDate(dobNamStr) != null) gender = "Nam";
                    else if (!string.IsNullOrEmpty(dobNuStr) && ParseDate(dobNuStr) != null) gender = "Nữ";

                    var phone = GetValue(row, "SĐT", "Phone", "Điện thoại", "SDT", "Số điện thoại") ?? "";
              
                    if (existing.Any(e => e.FullName == fullName && e.DateOfBirth == dob)) continue;

                    // ── Dân tộc
                    string ethnicity = GetValue(row, "Dân tộc", "Ethnic");
                    if (string.IsNullOrEmpty(ethnicity) || IsCheckedMark(ethnicity))
                        ethnicity = "Kinh"; // default khi không resolve được

                    // ── Tôn giáo 
                    string religion = GetValue(row, "Tôn giáo");
                    if (string.IsNullOrEmpty(religion) || IsCheckedMark(religion))
                        religion = "Không";

                    // ── Nghề nghiệp, Học vấn, Chuyên môn, LLCT (checkbox → sub-tên) ──
                    string profession = GetValue(row, "Nghề nghiệp");
                    if (IsCheckedMark(profession)) profession = "";

                    string education = GetValue(row, "Học vấn");
                    if (IsCheckedMark(education)) education = "";

                    string expertise = GetValue(row, "Chuyên môn");
                    if (IsCheckedMark(expertise)) expertise = "";

                    string politicalTheory = GetValue(row, "Lý luận chính trị");
                    if (IsCheckedMark(politicalTheory)) politicalTheory = "";

                    // ── Đảng viên: ngày kết nạp 
                    var partyProb = ParseDate(GetValue(row,
                        "Đoàn viên là đảng viên - Ngày kết nạp dự bị",
                        "Ngày kết nạp dự bị", "Dự bị"));
                    var partyOff = ParseDate(GetValue(row,
                        "Đoàn viên là đảng viên - Ngày kết nạp chính thức",
                        "Ngày kết nạp chính thức", "Chính thức"));

                    // ── Cấp ủy  
                    var notesList = new System.Collections.Generic.List<string>();
                    if (IsCheckedMark(GetValue(row, "Tham gia cấp ủy cấp trên cơ sở")))
                        notesList.Add("Tham gia cấp ủy cấp trên cơ sở");
                    if (IsCheckedMark(GetValue(row, "Tham gia cấp ủy cơ sở")))
                        notesList.Add("Tham gia cấp ủy cơ sở");
                    string notes = string.Join(", ", notesList);

                    // ── Chức vụ 
                    var posList = new System.Collections.Generic.List<string>();
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban chấp hành"))) posList.Add("Ban chấp hành");
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban thường vụ"))) posList.Add("Ban thường vụ");
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Bí thư"))) posList.Add("Bí thư");
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Phó Bí thư"))) posList.Add("Phó Bí thư");
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Chuyên môn"))) posList.Add("Cấp trưởng (Chuyên môn)");
                    if (IsCheckedMark(GetValue(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt"))) posList.Add("Cấp phó (Chuyên môn)");
                    
                    string position = string.Join(", ", posList);
                    if (string.IsNullOrEmpty(position))
                        position = GetValue(row, "Chức vụ", "Position");

                    // ── Phân loại 
                    bool isUnionMember = true; 
                    string phanLoai = GetValue(row, "Phân loại", "Loại");
                    if (!string.IsNullOrEmpty(phanLoai))
                        isUnionMember = phanLoai.ToLower().Contains("đoàn") || phanLoai.ToLower().Contains("đảng");

                    var m = new Member
                    {
                        FullName = fullName,
                        MemberCode = GetValue(row, "Mã", "Code", "Mã đoàn viên") ?? ("DV" + DateTime.Now.ToString("yyyyMMddHHmmss") + count),
                        Phone = phone,
                        Email = GetValue(row, "Email", "Thư điện tử"),
                        Address = GetValue(row, "Địa chỉ", "Address", "Quê quán"),
                        Position = position,
                        CardNumber = GetValue(row, "Số thẻ", "Card"),
                        Gender = gender,
                        Ethnicity = ethnicity,
                        Religion = religion,
                        Profession = profession,
                        Education = education,
                        Expertise = expertise,
                        PoliticalTheory = politicalTheory,
                        Notes = notes,
                        IdentityNumber = GetValue(row, "Số CMND", "CCCD", "CMND"),
                        HealthStatus = GetValue(row, "Sức khỏe", "SK"),
                        IsActive = true,
                        IsUnionMember = isUnionMember,
                        DateOfBirth = dob,
                        JoinDate = ParseDate(GetValue(row, "Ngày vào đoàn", "Ngày kết nạp Đoàn")),
                        PartyDateProbationary = partyProb,
                        PartyDateOfficial = partyOff,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    db.Members.Add(m);
                    count++;
                }

                if (count == 0 && data.Any())
                {
                    var firstRow = data.First();
                    var debugInfo = string.Join(" | ", firstRow.Take(8).Select(kv => $"{kv.Key}: {kv.Value}"));
                    return Ok(new { success = false, count = 0, message = $"Không tìm thấy dữ liệu hợp lệ. Keys: {debugInfo}" });
                }

                db.SaveChanges();
                
                new AuditService(db).Log(userId, username, "IMPORT_MEMBER", "DOAN_VIEN", 
                    $"Import thành công {count} đoàn viên từ file: {attachment.OriginalName} (ID: {attachment.Id})");

                return Ok(new { success = true, count, message = $"Đã import thành công {count} bản ghi." });
            }
        }

        private bool IsCheckedMark(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            var v = val.ToLower().Trim();
            return v == "x" || v == "✓" || v == "✔" || v == "v";
        }

        private bool IsTrue(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            return val == "true" || val == "1" || val == "x" || val == "có" || val == "yes" || val == "đúng";
        }

        private DateTime? ParseDate(string val)
        {
            if (string.IsNullOrEmpty(val)) return null;
            if (IsCheckedMark(val)) return null;
            DateTime dt;
            string[] fmts = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(val, fmts, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return dt;
            if (DateTime.TryParse(val, out dt))
                return dt;
            return null;
        }

        private string GetValue(Dictionary<string, string> row, params string[] keys)
        {
            foreach (var k in keys)
            {
                var normK = System.Text.RegularExpressions.Regex.Replace(k, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                
                // Direct match
                var match = row.Keys.FirstOrDefault(x => {
                    var normX = System.Text.RegularExpressions.Regex.Replace(x, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                    return normX.Equals(normK);
                });
                if (match != null) return row[match]?.Trim();

                // Fuzzy match (Contains)
                if (normK.Length > 3)
                {
                    match = row.Keys.FirstOrDefault(x => {
                        var normX = System.Text.RegularExpressions.Regex.Replace(x, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                        return normX.Contains(normK) || normK.Contains(normX);
                    });
                    if (match != null) return row[match]?.Trim();
                }
            }
            return null;
        }
    }

    public class ImportRequest
    {
        public int FileId { get; set; }
        public string SheetName { get; set; }
    }

}
