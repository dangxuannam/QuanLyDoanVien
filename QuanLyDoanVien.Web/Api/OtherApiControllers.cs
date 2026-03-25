using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;
using QuanLyDoanVien.Services;

namespace QuanLyDoanVien.Api
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
                return BadRequest("Há» tÃªn Ä‘oÃ n viÃªn khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");

            using (var db = new AppDbContext())
            {
                if (string.IsNullOrEmpty(req.MemberCode))
                    req.MemberCode = "DV" + DateTime.Now.ToString("yyyyMMddHHmmss");

                if (db.Members.Any(m => m.MemberCode == req.MemberCode))
                    return BadRequest("MÃ£ Ä‘oÃ n viÃªn Ä‘Ã£ tá»“n táº¡i.");

                req.CreatedBy = (int)Request.Properties["CurrentUserId"];
                req.CreatedAt = DateTime.Now;
                req.IsActive = true;
                db.Members.Add(req);
                db.SaveChanges();

                new AuditService(db).Log(req.CreatedBy,
                    Request.Properties["CurrentUsername"]?.ToString(),
                    "CREATE_MEMBER", "DOAN_VIEN", $"ThÃªm Ä‘oÃ n viÃªn: {req.FullName}");

                return Ok(new { success = true, id = req.Id, message = "ThÃªm Ä‘oÃ n viÃªn thÃ nh cÃ´ng." });
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
                // Bá»• sung cÃ¡c trÆ°á»ng má»›i
                m.Ethnicity = req.Ethnicity ?? m.Ethnicity;
                m.Religion = req.Religion ?? m.Religion;
                m.Profession = req.Profession ?? m.Profession;
                m.Education = req.Education ?? m.Education;
                m.Expertise = req.Expertise ?? m.Expertise;
                m.PoliticalTheory = req.PoliticalTheory ?? m.PoliticalTheory;
                m.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Ok(new { success = true, message = "Cáº­p nháº­t thÃ nh cÃ´ng." });
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
                return Ok(new { success = true, message = "ÄÃ£ xÃ³a Ä‘oÃ n viÃªn." });
            }
        }

        [HttpPost, Route("delete-multiple")]
        [ApiAuthorize(Permission = "DV_DELETE")]
        public IHttpActionResult DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sÃ¡ch ID trá»‘ng.");
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
                        "DELETE_MEMBER", "DOAN_VIEN", $"XÃ³a {members.Count} Ä‘oÃ n viÃªn.");

                    return Ok(new { success = true, message = $"ÄÃ£ xÃ³a {members.Count} Ä‘oÃ n viÃªn." });
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
                        "DELETE_ALL_MEMBERS", "DOAN_VIEN", "XÃ³a toÃ n bá»™ danh sÃ¡ch Ä‘oÃ n viÃªn.");

                    return Ok(new { success = true, message = $"ÄÃ£ xÃ³a toÃ n bá»™ {count} Ä‘oÃ n viÃªn." });
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
            if (string.IsNullOrEmpty(req?.GroupName)) return BadRequest("TÃªn nhÃ³m khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
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
                    var ws = package.Workbook.Worksheets.Add("Danh sÃ¡ch Ä‘oÃ n viÃªn");
                    string[] headers = { 
                        "STT", "Há» vÃ  tÃªn", "NgÃ y sinh", "PhÃ¢n loáº¡i", "DÃ¢n tá»™c", "TÃ´n giÃ¡o",
                        "Nghá» nghiá»‡p", "Há»c váº¥n", "ChuyÃªn mÃ´n", "LÃ½ luáº­n chÃ­nh trá»‹",
                        "NgÃ y vÃ o Äáº£ng DB", "NgÃ y vÃ o Äáº£ng CT", "Chá»©c vá»¥", "SÄT", "Email"
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
                        ws.Cells[r, 4].Value = m.PartyDateOfficial.HasValue ? "Äáº£ng viÃªn" : (m.IsUnionMember ? "ÄoÃ n viÃªn" : "Thanh niÃªn");
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
            if (req == null || req.FileId <= 0) return BadRequest("ThÃ´ng tin file khÃ´ng há»£p lá»‡.");

            using (var db = new AppDbContext())
            {
                var attachment = db.FileAttachments.Find(req.FileId);
                if (attachment == null) return NotFound();

                var fileSvc = new Services.FileService(db);
                var fullPath = fileSvc.GetFullPath(attachment.FilePath);
                if (!System.IO.File.Exists(fullPath)) return BadRequest("File khÃ´ng tá»“n táº¡i trÃªn server.");

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
                    // â”€â”€ Há» vÃ  tÃªn 
                    string fullName = GetValue(row, "Há» vÃ  tÃªn", "Há» tÃªn", "Full Name", "HOTEN", "FullName", "TÃªn Ä‘oÃ n viÃªn");
                    if (string.IsNullOrEmpty(fullName)) continue;

                    // â”€â”€ NgÃ y sinh/ Nam/Ná»¯ 
                    string dobNamStr = GetValue(row, "NgÃ y, thÃ¡ng, nÄƒm sinh - Nam");
                    string dobNuStr = GetValue(row, "NgÃ y, thÃ¡ng, nÄƒm sinh - Ná»¯");
                    string dobChungStr = GetValue(row, "NgÃ y, thÃ¡ng, nÄƒm sinh", "NgÃ y thÃ¡ng nÄƒm sinh", "NgÃ y sinh", "Birthday");

                    string dobStr = !string.IsNullOrEmpty(dobNamStr) ? dobNamStr : 
                                   (!string.IsNullOrEmpty(dobNuStr) ? dobNuStr : dobChungStr);
                    var dob = ParseDate(dobStr);

                    // â”€â”€ Giá»›i tÃ­nh 
                    string gender = "";
                    if (!string.IsNullOrEmpty(dobNamStr) && ParseDate(dobNamStr) != null) gender = "Nam";
                    else if (!string.IsNullOrEmpty(dobNuStr) && ParseDate(dobNuStr) != null) gender = "Ná»¯";

                    var phone = GetValue(row, "SÄT", "Phone", "Äiá»‡n thoáº¡i", "SDT", "Sá»‘ Ä‘iá»‡n thoáº¡i") ?? "";
              
                    if (existing.Any(e => e.FullName == fullName && e.DateOfBirth == dob)) continue;

                    // â”€â”€ DÃ¢n tá»™c
                    string ethnicity = GetValue(row, "DÃ¢n tá»™c", "Ethnic");
                    if (string.IsNullOrEmpty(ethnicity) || IsCheckedMark(ethnicity))
                        ethnicity = "Kinh"; // default khi khÃ´ng resolve Ä‘Æ°á»£c

                    // â”€â”€ TÃ´n giÃ¡o 
                    string religion = GetValue(row, "TÃ´n giÃ¡o");
                    if (string.IsNullOrEmpty(religion) || IsCheckedMark(religion))
                        religion = "KhÃ´ng";

                    // â”€â”€ Nghá» nghiá»‡p, Há»c váº¥n, ChuyÃªn mÃ´n, LLCT (checkbox â†’ sub-tÃªn) â”€â”€
                    string profession = GetValue(row, "Nghá» nghiá»‡p");
                    if (IsCheckedMark(profession)) profession = "";

                    string education = GetValue(row, "Há»c váº¥n");
                    if (IsCheckedMark(education)) education = "";

                    string expertise = GetValue(row, "ChuyÃªn mÃ´n");
                    if (IsCheckedMark(expertise)) expertise = "";

                    string politicalTheory = GetValue(row, "LÃ½ luáº­n chÃ­nh trá»‹");
                    if (IsCheckedMark(politicalTheory)) politicalTheory = "";

                    // â”€â”€ Äáº£ng viÃªn: ngÃ y káº¿t náº¡p 
                    var partyProb = ParseDate(GetValue(row,
                        "ÄoÃ n viÃªn lÃ  Ä‘áº£ng viÃªn - NgÃ y káº¿t náº¡p dá»± bá»‹",
                        "NgÃ y káº¿t náº¡p dá»± bá»‹", "Dá»± bá»‹"));
                    var partyOff = ParseDate(GetValue(row,
                        "ÄoÃ n viÃªn lÃ  Ä‘áº£ng viÃªn - NgÃ y káº¿t náº¡p chÃ­nh thá»©c",
                        "NgÃ y káº¿t náº¡p chÃ­nh thá»©c", "ChÃ­nh thá»©c"));

                    // â”€â”€ Cáº¥p á»§y  
                    var notesList = new System.Collections.Generic.List<string>();
                    if (IsCheckedMark(GetValue(row, "Tham gia cáº¥p á»§y cáº¥p trÃªn cÆ¡ sá»Ÿ")))
                        notesList.Add("Tham gia cáº¥p á»§y cáº¥p trÃªn cÆ¡ sá»Ÿ");
                    if (IsCheckedMark(GetValue(row, "Tham gia cáº¥p á»§y cÆ¡ sá»Ÿ")))
                        notesList.Add("Tham gia cáº¥p á»§y cÆ¡ sá»Ÿ");
                    string notes = string.Join(", ", notesList);

                    // â”€â”€ Chá»©c vá»¥ 
                    var posList = new System.Collections.Generic.List<string>();
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t - Ban cháº¥p hÃ nh"))) posList.Add("Ban cháº¥p hÃ nh");
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t - Ban thÆ°á»ng vá»¥"))) posList.Add("Ban thÆ°á»ng vá»¥");
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t - BÃ­ thÆ°"))) posList.Add("BÃ­ thÆ°");
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t - PhÃ³ BÃ­ thÆ°"))) posList.Add("PhÃ³ BÃ­ thÆ°");
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t - ChuyÃªn mÃ´n"))) posList.Add("Cáº¥p trÆ°á»Ÿng (ChuyÃªn mÃ´n)");
                    if (IsCheckedMark(GetValue(row, "Sá»‘ Ä‘oÃ n viÃªn Ä‘áº£m nhiá»‡m cÃ¡c chá»©c vá»¥ chá»§ chá»‘t"))) posList.Add("Cáº¥p phÃ³ (ChuyÃªn mÃ´n)");
                    
                    string position = string.Join(", ", posList);
                    if (string.IsNullOrEmpty(position))
                        position = GetValue(row, "Chá»©c vá»¥", "Position");

                    // â”€â”€ PhÃ¢n loáº¡i 
                    bool isUnionMember = true; 
                    string phanLoai = GetValue(row, "PhÃ¢n loáº¡i", "Loáº¡i");
                    if (!string.IsNullOrEmpty(phanLoai))
                        isUnionMember = phanLoai.ToLower().Contains("Ä‘oÃ n") || phanLoai.ToLower().Contains("Ä‘áº£ng");

                    var m = new Member
                    {
                        FullName = fullName,
                        MemberCode = GetValue(row, "MÃ£", "Code", "MÃ£ Ä‘oÃ n viÃªn") ?? ("DV" + DateTime.Now.ToString("yyyyMMddHHmmss") + count),
                        Phone = phone,
                        Email = GetValue(row, "Email", "ThÆ° Ä‘iá»‡n tá»­"),
                        Address = GetValue(row, "Äá»‹a chá»‰", "Address", "QuÃª quÃ¡n"),
                        Position = position,
                        CardNumber = GetValue(row, "Sá»‘ tháº»", "Card"),
                        Gender = gender,
                        Ethnicity = ethnicity,
                        Religion = religion,
                        Profession = profession,
                        Education = education,
                        Expertise = expertise,
                        PoliticalTheory = politicalTheory,
                        Notes = notes,
                        IdentityNumber = GetValue(row, "Sá»‘ CMND", "CCCD", "CMND"),
                        HealthStatus = GetValue(row, "Sá»©c khá»e", "SK"),
                        IsActive = true,
                        IsUnionMember = isUnionMember,
                        DateOfBirth = dob,
                        JoinDate = ParseDate(GetValue(row, "NgÃ y vÃ o Ä‘oÃ n", "NgÃ y káº¿t náº¡p ÄoÃ n")),
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
                    return Ok(new { success = false, count = 0, message = $"KhÃ´ng tÃ¬m tháº¥y dá»¯ liá»‡u há»£p lá»‡. Keys: {debugInfo}" });
                }

                db.SaveChanges();
                
                new AuditService(db).Log(userId, username, "IMPORT_MEMBER", "DOAN_VIEN", 
                    $"Import thÃ nh cÃ´ng {count} Ä‘oÃ n viÃªn tá»« file: {attachment.OriginalName} (ID: {attachment.Id})");

                return Ok(new { success = true, count, message = $"ÄÃ£ import thÃ nh cÃ´ng {count} báº£n ghi." });
            }
        }

        private bool IsCheckedMark(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            var v = val.ToLower().Trim();
            return v == "x" || v == "âœ“" || v == "âœ”" || v == "v";
        }

        private bool IsTrue(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            return val == "true" || val == "1" || val == "x" || val == "cÃ³" || val == "yes" || val == "Ä‘Ãºng";
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

