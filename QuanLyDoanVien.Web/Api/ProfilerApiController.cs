using System.Web.Http;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Helpers;

namespace QuanLyDoanVien.Api
{
    /// <summary>
    /// API endpoint để xem kết quả profiling (tốc độ Controller + SQL queries).
    ///
    /// Endpoints:
    ///   GET  /api/profiler/results  → xem toàn bộ kết quả (50 requests + 50 SQL gần nhất)
    ///   POST /api/profiler/clear    → xóa dữ liệu, reset để đo lại từ đầu
    /// </summary>
    [RoutePrefix("api/profiler")]
    [ApiAuthorize]   // Chỉ user đã đăng nhập mới xem được
    public class ProfilerApiController : ApiController
    {
        /// <summary>
        /// Trả về toàn bộ dữ liệu profiling dạng JSON.
        ///
        /// Cấu trúc response:
        /// {
        ///   totalRequests: 25,         → số request đã đo trong 30 phút qua
        ///   avgRequestMs: 45.2,        → thời gian xử lý trung bình (ms)
        ///   maxRequestMs: 320.5,       → request chậm nhất (ms)
        ///   slowRequests: 2,           → số request > 500ms (cần tối ưu)
        ///   totalSqlQueries: 87,       → tổng số SQL query EF đã chạy
        ///   avgSqlMs: 12.3,            → thời gian SQL trung bình (ms)
        ///   slowSqlQueries: 1,         → số query > 100ms (cần thêm index)
        ///   requests: [...],           → 50 request gần nhất, mỗi cái có method/path/elapsed/status
        ///   sqlQueries: [...]          → 50 SQL query gần nhất, có text SQL + elapsed
        /// }
        /// </summary>
        [HttpGet, Route("results")]
        public IHttpActionResult GetResults()
        {
            var result = ProfileStore.GetResults();
            return Ok(result);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu đang lưu.
        /// Dùng khi muốn đo một tính năng cụ thể từ đầu (xóa noise từ trước).
        /// </summary>
        [HttpPost, Route("clear")]
        public IHttpActionResult Clear()
        {
            ProfileStore.Clear();
            return Ok(new { success = true, message = "Đã xóa toàn bộ dữ liệu profiling." });
        }
    }
}
