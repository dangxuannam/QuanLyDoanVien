$file = "c:\QuanLyDoanVien\QuanLyDoanVien.Web\Api\UnitsApiController.cs"
$content = Get-Content $file -Raw 

$oldInit = @"
        private UnitSummary BuildSummary(List<Dictionary<string, string>> data, string unitName, DateTime importedAt)
        {
            var summary = new UnitSummary
            {
                UnitName   = unitName,
                ImportedAt = importedAt,
                Professions       = new Dictionary<string, int>(),
                Educations        = new Dictionary<string, int>(),
                Expertises        = new Dictionary<string, int>(),
                Religions         = new Dictionary<string, int>(),
                PositionRoles     = new Dictionary<string, int>(),
                MemberRows        = new List<UnitMemberRow>()
            };
"@

$newInit = @"
        private string MapCategory(string input, string[] matchWords, string exactRet, ref Dictionary<string, int> dict)
        {
            if (string.IsNullOrEmpty(input)) { dict["Khác"]++; return "Khác"; }
            var lower = input.ToLower();
            foreach (var w in matchWords) {
                if (lower.Contains(w.ToLower())) { dict[exactRet]++; return exactRet; }
            }
            dict["Khác"]++;
            return "Khác";
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
"@

$content = $content.Replace($oldInit, $newInit)

$oldLoop = @"
                // ── Nghề nghiệp ───────────────────────────────────────────
                string prof = GetVal(row, "Nghề nghiệp");
                if (!string.IsNullOrEmpty(prof) && !IsChecked(prof))
                    IncrementDict(summary.Professions, prof);

                // ── Học vấn ───────────────────────────────────────────────
                string edu = GetVal(row, "Học vấn");
                if (!string.IsNullOrEmpty(edu) && !IsChecked(edu))
                    IncrementDict(summary.Educations, edu);

                // ── Chuyên môn ────────────────────────────────────────────
                string exp = GetVal(row, "Chuyên môn");
                if (!string.IsNullOrEmpty(exp) && !IsChecked(exp))
                    IncrementDict(summary.Expertises, exp);

                // ── Cấp ủy ────────────────────────────────────────────────
                if (IsChecked(GetVal(row, "Tham gia cấp ủy cấp trên cơ sở")))
                    summary.CommunistAboveBase++;
                if (IsChecked(GetVal(row, "Tham gia cấp ủy cơ sở")))
                    summary.CommunistBase++;

                // ── Chức vụ chủ chốt ──────────────────────────────────────
                var roles = new List<string>();
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban chấp hành",  "Ban chấp hành",  roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Ban thường vụ",   "Ban thường vụ",   roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Bí thư",          "Bí thư",          roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Phó Bí thư",      "Phó Bí thư",      roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt - Chuyên môn",      "Cấp trưởng",      roles, summary.PositionRoles);
                CheckRole(row, "Số đoàn viên đảm nhiệm các chức vụ chủ chốt",                   "Cấp phó",         roles, summary.PositionRoles);

                // ── Lưu chi tiết từng dòng ────────────────────────────────
                summary.MemberRows.Add(new UnitMemberRow
                {
                    FullName            = fullName,
                    Gender              = gender,
                    DateOfBirth         = dob,
                    Age                 = age,
                    Ethnicity           = eth,
                    Religion            = rel,
                    Profession          = GetVal(row, "Nghề nghiệp"),
                    Education           = GetVal(row, "Học vấn"),
                    Expertise           = GetVal(row, "Chuyên môn"),
                    PartyDateProbationary = partyProb,
                    PartyDateOfficial   = partyOff,
                    CommunistAboveBase  = IsChecked(GetVal(row, "Tham gia cấp ủy cấp trên cơ sở")),
                    CommunistBase       = IsChecked(GetVal(row, "Tham gia cấp ủy cơ sở")),
                    PositionRoles       = string.Join(", ", roles)
                });
"@

$newLoop = @"
                string profName = GetVal(row, "Nghề nghiệp");
                string mappedProf = string.IsNullOrEmpty(profName) ? "Khác" :
                    MapCategory(profName, new[]{"công chức"}, "Công chức", ref summary.Professions) == "Khác" ?
                    MapCategory(profName, new[]{"viên chức"}, "Viên chức", ref summary.Professions) == "Khác" ?
                    MapCategory(profName, new[]{"sinh viên", "học sinh"}, "Sinh viên", ref summary.Professions) == "Khác" ?
                    "Khác" : "Sinh viên" : "Viên chức" : "Công chức";
                if(mappedProf == "Khác") { summary.Professions["Khác"]++; } // Since mapcategory adds up, need to fix logic
                
                // Let's use simpler logic for Mapping
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
"@

$content = $content.Replace($oldLoop, $newLoop)
Set-Content $file $content
