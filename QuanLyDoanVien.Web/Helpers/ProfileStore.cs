using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyDoanVien.Helpers
{
    /// <summary>
    /// Kho lưu trữ dữ liệu profiling trong RAM.
    /// Thread-safe, tự động xóa các entry cũ hơn 30 phút.
    /// </summary>
    public static class ProfileStore
    {
        // Giới hạn 500 entry mới nhất để không tốn RAM
        private const int MaxEntries = 500;

        // ConcurrentQueue đảm bảo thread-safe khi nhiều request đồng thời
        private static readonly ConcurrentQueue<RequestEntry> _requests
            = new ConcurrentQueue<RequestEntry>();

        // Lưu riêng SQL queries (một request có thể có nhiều query)
        private static readonly ConcurrentQueue<SqlEntry> _sqlQueries
            = new ConcurrentQueue<SqlEntry>();

        /// <summary>
        /// Ghi lại một HTTP request đã hoàn tất.
        /// Được gọi từ Application_EndRequest trong Global.asax.cs
        /// </summary>
        public static void Record(string method, string path, double elapsedMs, int statusCode)
        {
            _requests.Enqueue(new RequestEntry
            {
                Method     = method,
                Path       = path,
                ElapsedMs  = Math.Round(elapsedMs, 2),
                StatusCode = statusCode,
                Timestamp  = DateTime.Now
            });

            // Giữ tối đa MaxEntries bản ghi
            while (_requests.Count > MaxEntries)
                _requests.TryDequeue(out _);
        }

        /// <summary>
        /// Ghi lại một câu SQL query đã thực thi.
        /// Được gọi từ EfTimingInterceptor.
        /// </summary>
        public static void RecordSql(string sql, double elapsedMs, bool isError = false)
        {
            // Rút gọn SQL dài để dễ đọc
            var shortSql = sql?.Length > 300 ? sql.Substring(0, 300) + "..." : sql;

            _sqlQueries.Enqueue(new SqlEntry
            {
                Sql       = shortSql,
                ElapsedMs = Math.Round(elapsedMs, 2),
                IsError   = isError,
                Timestamp = DateTime.Now
            });

            while (_sqlQueries.Count > MaxEntries)
                _sqlQueries.TryDequeue(out _);
        }

        /// <summary>
        /// Lấy kết quả để hiển thị qua API.
        /// </summary>
        public static ProfilerResult GetResults()
        {
            var cutoff   = DateTime.Now.AddMinutes(-30);
            var requests = _requests
                .Where(r => r.Timestamp >= cutoff)
                .OrderByDescending(r => r.Timestamp)
                .ToList();

            var sqlQueries = _sqlQueries
                .Where(q => q.Timestamp >= cutoff)
                .OrderByDescending(q => q.Timestamp)
                .ToList();

            return new ProfilerResult
            {
                TotalRequests   = requests.Count,
                AvgRequestMs    = requests.Any() ? Math.Round(requests.Average(r => r.ElapsedMs), 2) : 0,
                MaxRequestMs    = requests.Any() ? requests.Max(r => r.ElapsedMs) : 0,
                SlowRequests    = requests.Where(r => r.ElapsedMs > 500).Count(),
                TotalSqlQueries = sqlQueries.Count,
                AvgSqlMs        = sqlQueries.Any() ? Math.Round(sqlQueries.Average(q => q.ElapsedMs), 2) : 0,
                SlowSqlQueries  = sqlQueries.Where(q => q.ElapsedMs > 100).Count(),
                Requests        = requests.Take(50).ToList(),
                SqlQueries      = sqlQueries.Take(50).ToList(),
            };
        }

        /// <summary>Xóa toàn bộ dữ liệu đang lưu (dùng để reset khi test)</summary>
        public static void Clear()
        {
            while (_requests.TryDequeue(out _)) { }
            while (_sqlQueries.TryDequeue(out _)) { }
        }
    }

    // ─── Data models ─────────────────────────────────────────────────────────

    public class ProfilerResult
    {
        public int    TotalRequests   { get; set; }
        public double AvgRequestMs    { get; set; }
        public double MaxRequestMs    { get; set; }
        public int    SlowRequests    { get; set; }   // > 500ms
        public int    TotalSqlQueries { get; set; }
        public double AvgSqlMs        { get; set; }
        public int    SlowSqlQueries  { get; set; }   // > 100ms
        public List<RequestEntry> Requests   { get; set; }
        public List<SqlEntry>     SqlQueries { get; set; }
    }

    public class RequestEntry
    {
        public string   Method     { get; set; }
        public string   Path       { get; set; }
        public double   ElapsedMs  { get; set; }
        public int      StatusCode { get; set; }
        public DateTime Timestamp  { get; set; }
    }

    public class SqlEntry
    {
        public string   Sql       { get; set; }
        public double   ElapsedMs { get; set; }
        public bool     IsError   { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
