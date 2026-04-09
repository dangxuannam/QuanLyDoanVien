using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using QuanLyDoanVien.Api;
using QuanLyDoanVien.Models;
using QuanLyDoanVien.Models.Entities;

namespace QuanLyDoanVien.Helpers
{
    /// <summary>
    /// Tái tạo SummaryJson cho Unit từ dữ liệu Member live trong database.
    /// Được gọi sau mỗi thao tác Create/Update/Delete đoàn viên.
    /// </summary>
    public static class UnitSummaryHelper
    {
        public static void RebuildSummary(AppDbContext db, int? unitId)
        {
            if (!unitId.HasValue) return;

            var unit = db.Units.Find(unitId.Value);
            if (unit == null) return;

            // Lấy tất cả đoàn viên đang hoạt động của đơn vị
            var members = db.Members
                .Where(m => m.UnitId == unitId.Value && m.IsActive)
                .ToList();

            var summary = BuildSummaryFromMembers(members, unit.UnitName);

            var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            unit.SummaryJson  = js.Serialize(summary);
            unit.TotalMembers = summary.TotalMembers;
            unit.LastImportAt = DateTime.Now;   // cập nhật thời gian "lần cuối cập nhật"

            db.SaveChanges();
        }

        private static UnitSummary BuildSummaryFromMembers(List<Member> members, string unitName)
        {
            var summary = new UnitSummary
            {
                UnitName   = unitName,
                ImportedAt = DateTime.Now,
                Religions         = new Dictionary<string, int>(),
                PositionRoles     = new Dictionary<string, int>
                {
                    { "Ban chấp hành", 0 }, { "Ban thường vụ", 0 }, { "Bí thư", 0 },
                    { "Phó Bí thư", 0 },    { "Cấp trưởng", 0 },    { "Cấp phó", 0 }
                },
                Professions = new Dictionary<string, int>
                    { { "Công chức", 0 }, { "Viên chức", 0 }, { "Sinh viên", 0 }, { "Khác", 0 } },
                Educations = new Dictionary<string, int>
                    { { "THCS", 0 }, { "THPT", 0 }, { "Khác", 0 } },
                Expertises = new Dictionary<string, int>
                    { { "Tiến sĩ", 0 }, { "Thạc sĩ", 0 }, { "Đại học", 0 }, { "Cao đẳng", 0 }, { "Khác", 0 } },
                PoliticalTheories = new Dictionary<string, int>
                    { { "Sơ cấp", 0 }, { "Trung cấp", 0 }, { "Cao cấp", 0 }, { "Cử nhân", 0 }, { "Khác", 0 } },
                MemberRows = new List<UnitMemberRow>()
            };

            var today = DateTime.Today;

            foreach (var m in members)
            {
                summary.TotalMembers++;

                // ── Giới tính ────────────────────────────────────────────────
                if (m.Gender == "Nam")       summary.MaleCount++;
                else if (m.Gender == "Nữ")  summary.FemaleCount++;

                // ── Tuổi ────────────────────────────────────────────────────
                int? age = null;
                if (m.DateOfBirth.HasValue)
                {
                    age = today.Year - m.DateOfBirth.Value.Year;
                    if (m.DateOfBirth.Value.AddYears(age.Value) > today) age--;
                    if      (age >= 18 && age <= 25) summary.Age18To25++;
                    else if (age >= 26 && age <= 30) summary.Age26To30++;
                    else if (age >= 31)              summary.Age31Plus++;
                }

                // ── Dân tộc ─────────────────────────────────────────────────
                var eth = m.Ethnicity ?? "";
                if (string.IsNullOrEmpty(eth) || eth.ToLower().Contains("kinh"))
                    summary.EthnicityKinh++;
                else
                    summary.EthnicityOther++;

                // ── Tôn giáo ────────────────────────────────────────────────
                var rel = string.IsNullOrEmpty(m.Religion) ? "Không" : m.Religion;
                IncrementDict(summary.Religions, rel);

                // ── Đảng viên ───────────────────────────────────────────────
                if (m.PartyDateProbationary.HasValue) summary.PartyProbationaryCount++;
                if (m.PartyDateOfficial.HasValue)     summary.PartyOfficialCount++;

                // ── Nghề nghiệp ─────────────────────────────────────────────
                var prof = MapCategory(m.Profession,
                    ("Công chức",  new[] { "công chức" }),
                    ("Viên chức",  new[] { "viên chức" }),
                    ("Sinh viên",  new[] { "sinh viên", "học sinh" })) ?? "Khác";
                summary.Professions[prof]++;

                // ── Học vấn ─────────────────────────────────────────────────
                var edu = MapCategory(m.Education,
                    ("THCS", new[] { "thcs", "cấp 2" }),
                    ("THPT", new[] { "thpt", "cấp 3" })) ?? "Khác";
                summary.Educations[edu]++;

                // ── Chuyên môn ─────────────────────────────────────────────
                var exp = MapCategory(m.Expertise,
                    ("Tiến sĩ",  new[] { "tiến sĩ", "ts", "tiến sỹ" }),
                    ("Thạc sĩ",  new[] { "thạc sĩ", "thạc sỹ", "ths" }),
                    ("Đại học",  new[] { "đại học", "đh" }),
                    ("Cao đẳng", new[] { "cao đẳng", "cđ" })) ?? "Khác";
                summary.Expertises[exp]++;

                // ── Lý luận chính trị ───────────────────────────────────────
                var pol = MapCategory(m.PoliticalTheory,
                    ("Cử nhân",  new[] { "cử nhân" }),
                    ("Cao cấp",  new[] { "cao cấp" }),
                    ("Trung cấp",new[] { "trung cấp" }),
                    ("Sơ cấp",   new[] { "sơ cấp" })) ?? "Khác";
                summary.PoliticalTheories[pol]++;

                // ── Chức vụ chủ chốt ─────────────────────────────────────
                var pos = (m.Position ?? "").ToLower();
                var roles = new List<string>();
                MapRole(pos, "ban chấp hành", "Ban chấp hành", roles, summary.PositionRoles);
                MapRole(pos, "ban thường vụ", "Ban thường vụ", roles, summary.PositionRoles);
                MapRole(pos, "bí thư",        "Bí thư",        roles, summary.PositionRoles);
                MapRole(pos, "phó bí thư",    "Phó Bí thư",    roles, summary.PositionRoles);
                MapRole(pos, "trưởng",        "Cấp trưởng",    roles, summary.PositionRoles);
                MapRole(pos, "phó",           "Cấp phó",       roles, summary.PositionRoles);

                summary.MemberRows.Add(new UnitMemberRow
                {
                    FullName              = m.FullName,
                    Gender                = m.Gender,
                    DateOfBirth           = m.DateOfBirth,
                    Age                   = age,
                    Ethnicity             = eth,
                    Religion              = rel,
                    Profession            = prof,
                    Education             = edu,
                    Expertise             = exp,
                    PoliticalTheory       = pol,
                    PartyDateProbationary = m.PartyDateProbationary,
                    PartyDateOfficial     = m.PartyDateOfficial,
                    CommunistAboveBase    = false,
                    CommunistBase         = false,
                    PositionRoles         = string.Join(", ", roles)
                });
            }

            return summary;
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private static void IncrementDict(Dictionary<string, int> dict, string key)
        {
            if (!dict.ContainsKey(key)) dict[key] = 0;
            dict[key]++;
        }

        private static void MapRole(string posLower, string keyword, string roleKey,
            List<string> roles, Dictionary<string, int> roleDict)
        {
            if (!posLower.Contains(keyword)) return;
            roles.Add(roleKey);
            if (roleDict.ContainsKey(roleKey)) roleDict[roleKey]++;
        }

        /// <summary>Kiểm tra input có chứa một trong các keywords không, trả về tên category.</summary>
        private static string MapCategory(string input,
            params (string Category, string[] Keywords)[] rules)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var lower = input.ToLower();
            foreach (var (cat, kws) in rules)
                foreach (var kw in kws)
                    if (lower.Contains(kw)) return cat;
            return null;
        }
    }
}
