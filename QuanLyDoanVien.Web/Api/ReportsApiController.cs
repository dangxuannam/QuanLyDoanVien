using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;
using QuanLyDoanVien.Api;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Models;

namespace QuanLyDoanVien.Api
{
    [RoutePrefix("api/reports")]
    [ApiAuthorize]
    public class ReportsApiController : ApiController
    {
        [HttpGet, Route("by-unit")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult GetByUnit()
        {
            using (var db = new AppDbContext())
            {
                var units = db.Units.Where(u => u.IsActive).OrderBy(u => u.UnitName).ToList();
                var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

                var result = units.Select(u =>
                {
                    if (!string.IsNullOrEmpty(u.SummaryJson))
                    {
                        try
                        {
                            var s = js.Deserialize<UnitSummary>(u.SummaryJson);
                            return new
                            {
                                unitId   = u.Id,
                                unitCode = u.UnitCode,
                                unitName = u.UnitName,
                                total    = s.TotalMembers,
                                male     = s.MaleCount,
                                female   = s.FemaleCount
                            };
                        }
                        catch { }
                    }
                    return new
                    {
                        unitId   = u.Id,
                        unitCode = u.UnitCode,
                        unitName = u.UnitName,
                        total    = db.Members.Count(m => m.IsActive && m.UnitId == u.Id),
                        male     = db.Members.Count(m => m.IsActive && m.UnitId == u.Id && m.Gender == "Nam"),
                        female   = db.Members.Count(m => m.IsActive && m.UnitId == u.Id && m.Gender == "Nữ")
                    };
                }).ToList();

                int grandTotal  = result.Sum(r => r.total);
                int totalMale   = result.Sum(r => r.male);
                int totalFemale = result.Sum(r => r.female);
                int noUnit      = db.Members.Count(m => m.IsActive && m.UnitId == null);

                return Ok(new { grandTotal, totalMale, totalFemale, noUnit, units = result });
            }
        }

        [HttpGet, Route("demographics")]
        [ApiAuthorize(Permission = "DV_VIEW")]
        public IHttpActionResult GetDemographics(int? unitId = null)
        {
            using (var db = new AppDbContext())
            {
                if (unitId.HasValue)
                {
                    var unit = db.Units.Find(unitId.Value);
                    if (unit != null && !string.IsNullOrEmpty(unit.SummaryJson))
                    {
                        try
                        {
                            var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                            var summary = js.Deserialize<UnitSummary>(unit.SummaryJson);
                            return Ok(BuildDemographicsFromSummary(summary));
                        }
                        catch { }
                    }

                    var members = db.Members.Where(m => m.IsActive && m.UnitId == unitId.Value).ToList();
                    return Ok(BuildDemographicsFromMembers(members));
                }

                // Tất cả đơn vị: query live từ tất cả Members đang hoạt động
                var allMembers = db.Members.Where(m => m.IsActive).ToList();
                return Ok(BuildDemographicsFromMembers(allMembers));
            }
        }

        private object BuildDemographicsFromSummary(UnitSummary s)
        {
            int total = s.TotalMembers;

            var byGender = new List<object>();
            if (s.MaleCount > 0)   byGender.Add(new { label = "Nam", count = s.MaleCount });
            if (s.FemaleCount > 0) byGender.Add(new { label = "Nữ", count = s.FemaleCount });
            int genderUnknown = total - s.MaleCount - s.FemaleCount;
            if (genderUnknown > 0) byGender.Add(new { label = "Chưa xác định", count = genderUnknown });

            var byAge = new List<object>();
            if (s.Age18To25 > 0)  byAge.Add(new { label = "18 - 25 tuổi",      count = s.Age18To25  });
            if (s.Age26To30 > 0)  byAge.Add(new { label = "26 - 30 tuổi",      count = s.Age26To30  });
            if (s.Age31Plus > 0)  byAge.Add(new { label = "31 tuổi trở lên",   count = s.Age31Plus  });
            int ageUnknown = total - s.Age18To25 - s.Age26To30 - s.Age31Plus;
            if (ageUnknown > 0)   byAge.Add(new { label = "Chưa xác định",     count = ageUnknown   });

            var byEthnicity = new List<object>();
            if (s.EthnicityKinh  > 0) byEthnicity.Add(new { label = "Kinh",  count = s.EthnicityKinh  });
            if (s.EthnicityOther > 0) byEthnicity.Add(new { label = "Khác",  count = s.EthnicityOther });

            var byReligion       = DictToList(s.Religions);
            var byProfession     = DictToList(s.Professions);
            var byEducation      = DictToList(s.Educations);
            var byExpertise      = DictToList(s.Expertises);
            var byPoliticalTheory = DictToList(s.PoliticalTheories);

            return new
            {
                total,
                byGender,
                byAge,
                byEthnicity,
                byReligion,
                byEducation,
                byExpertise,
                byProfession,
                byPoliticalTheory
            };
        }

        private List<object> DictToList(Dictionary<string, int> dict)
        {
            if (dict == null) return new List<object>();
            return dict
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => (object)new { label = kv.Key, count = kv.Value })
                .ToList();
        }

        private object BuildDemographicsFromMembers(List<Models.Entities.Member> members)
        {
            int total = members.Count;

            var byGender = members
                .GroupBy(m => string.IsNullOrEmpty(m.Gender) ? "Chưa xác định" : m.Gender)
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var today = DateTime.Today;
            int age18to25 = 0, age26to30 = 0, age31plus = 0, ageUnknown = 0;
            foreach (var m in members)
            {
                if (!m.DateOfBirth.HasValue) { ageUnknown++; continue; }
                int age = today.Year - m.DateOfBirth.Value.Year;
                if (m.DateOfBirth.Value.AddYears(age) > today) age--;
                if      (age >= 18 && age <= 25) age18to25++;
                else if (age >= 26 && age <= 30) age26to30++;
                else if (age >= 31)              age31plus++;
                else                             ageUnknown++;
            }
            var byAge = new List<object>();
            if (age18to25  > 0) byAge.Add(new { label = "18 - 25 tuổi",    count = age18to25  });
            if (age26to30  > 0) byAge.Add(new { label = "26 - 30 tuổi",    count = age26to30  });
            if (age31plus  > 0) byAge.Add(new { label = "31 tuổi trở lên", count = age31plus  });
            if (ageUnknown > 0) byAge.Add(new { label = "Chưa xác định",   count = ageUnknown });

            var byEthnicity = members
                .GroupBy(m => string.IsNullOrEmpty(m.Ethnicity) ? "Chưa xác định" : Capitalize(m.Ethnicity))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var byReligion = members
                .GroupBy(m => string.IsNullOrEmpty(m.Religion) ? "Không" : Capitalize(m.Religion))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var byEducation = members
                .GroupBy(m => NormalizeEducation(m.Education))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var byExpertise = members
                .GroupBy(m => NormalizeExpertise(m.Expertise))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var byProfession = members
                .GroupBy(m => NormalizeProfession(m.Profession))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            var byPoliticalTheory = members
                .GroupBy(m => NormalizePolitical(m.PoliticalTheory))
                .Select(g => (object)new { label = g.Key, count = g.Count() })
                .OrderByDescending(g => ((dynamic)g).count)
                .ToList();

            return new
            {
                total,
                byGender,
                byAge,
                byEthnicity,
                byReligion,
                byEducation,
                byExpertise,
                byProfession,
                byPoliticalTheory
            };
        }

        
        private string Capitalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private string NormalizeEducation(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "Chưa xác định";
            var v = val.ToLower();
            if (v.Contains("thcs") || v.Contains("cấp 2")) return "THCS";
            if (v.Contains("thpt") || v.Contains("cấp 3")) return "THPT";
            return Capitalize(val.Trim());
        }

        private string NormalizeExpertise(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "Chưa xác định";
            var v = val.ToLower();
            if (v.Contains("tiến sĩ") || v.Contains("tiến sỹ")) return "Tiến sĩ";
            if (v.Contains("thạc sĩ") || v.Contains("thạc sỹ") || v.Contains("ths")) return "Thạc sĩ";
            if (v.Contains("đại học") || v.Contains("cử nhân")) return "Đại học";
            if (v.Contains("cao đẳng")) return "Cao đẳng";
            if (v.Contains("trung cấp")) return "Trung cấp";
            if (v.Contains("sơ cấp")) return "Sơ cấp";
            return Capitalize(val.Trim());
        }

        private string NormalizeProfession(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "Chưa xác định";
            var v = val.ToLower();
            if (v.Contains("công chức")) return "Công chức";
            if (v.Contains("viên chức")) return "Viên chức";
            if (v.Contains("sinh viên") || v.Contains("học sinh")) return "Sinh viên/Học sinh";
            return Capitalize(val.Trim());
        }

        private string NormalizePolitical(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "Chưa xác định";
            var v = val.ToLower();
            if (v.Contains("cử nhân")) return "Cử nhân";
            if (v.Contains("cao cấp")) return "Cao cấp";
            if (v.Contains("trung cấp")) return "Trung cấp";
            if (v.Contains("sơ cấp")) return "Sơ cấp";
            return Capitalize(val.Trim());
        }
    }
}
