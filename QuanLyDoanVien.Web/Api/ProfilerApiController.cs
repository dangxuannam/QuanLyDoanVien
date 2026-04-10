using System.Web.Http;
using QuanLyDoanVien.Filters;
using QuanLyDoanVien.Helpers;

namespace QuanLyDoanVien.Api
{

    [RoutePrefix("api/profiler")]
    [ApiAuthorize]   // Chỉ user đã đăng nhập mới xem được
    public class ProfilerApiController : ApiController
    {
        
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
