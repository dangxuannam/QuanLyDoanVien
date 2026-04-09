using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using QuanLyDoanVien.Helpers;

namespace QuanLyDoanVien.Helpers
{
    /// <summary>
    /// EF6 Interceptor tự động đo thời gian của MỌI câu SQL query
    /// mà Entity Framework sinh ra, KHÔNG cần thay đổi code trong Controllers.
    ///
    /// Cơ chế hoạt động:
    ///   EF6 chuẩn bị SQL → NonQueryExecuting() / ReaderExecuting() → [SQL chạy trên DB]
    ///   → NonQueryExecuted() / ReaderExecuted() → tính elapsed, ghi vào ProfileStore
    ///
    /// Đăng ký trong Global.asax.cs:
    ///   DbInterception.Add(new EfTimingInterceptor());
    /// </summary>
    public class EfTimingInterceptor : DbCommandInterceptor
    {
        // ─── SELECT queries (DbDataReader) ───────────────────────────────────

        public override void ReaderExecuting(DbCommand command,
            DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            StartTimer(command);
            base.ReaderExecuting(command, interceptionContext);
        }

        public override void ReaderExecuted(DbCommand command,
            DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            base.ReaderExecuted(command, interceptionContext);
            StopTimer(command, interceptionContext.Exception);
        }

        // ─── INSERT / UPDATE / DELETE queries (NonQuery) ─────────────────────

        public override void NonQueryExecuting(DbCommand command,
            DbCommandInterceptionContext<int> interceptionContext)
        {
            StartTimer(command);
            base.NonQueryExecuting(command, interceptionContext);
        }

        public override void NonQueryExecuted(DbCommand command,
            DbCommandInterceptionContext<int> interceptionContext)
        {
            base.NonQueryExecuted(command, interceptionContext);
            StopTimer(command, interceptionContext.Exception);
        }

        // ─── Scalar queries (COUNT, SUM, ...) ────────────────────────────────

        public override void ScalarExecuting(DbCommand command,
            DbCommandInterceptionContext<object> interceptionContext)
        {
            StartTimer(command);
            base.ScalarExecuting(command, interceptionContext);
        }

        public override void ScalarExecuted(DbCommand command,
            DbCommandInterceptionContext<object> interceptionContext)
        {
            base.ScalarExecuted(command, interceptionContext);
            StopTimer(command, interceptionContext.Exception);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Lưu Stopwatch vào ExtendedProperties của DbCommand để dùng sau.
        /// (Không thể dùng biến instance vì Interceptor là singleton, nhiều
        ///  thread có thể gọi cùng lúc)
        /// </summary>
        private static void StartTimer(DbCommand command)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                command.GetHashCode(); // đảm bảo command tồn tại
                // Lưu tạm vào Dictionary thread-safe per-command
                TimerStore.Start(command, sw);
            }
            catch { /* không bao giờ để interceptor làm crash app */ }
        }

        private static void StopTimer(DbCommand command, Exception exception)
        {
            try
            {
                var sw = TimerStore.Stop(command);
                if (sw == null) return;

                sw.Stop();
                var elapsed = sw.Elapsed.TotalMilliseconds;
                var sql     = command.CommandText ?? "";

                ProfileStore.RecordSql(sql, elapsed, isError: exception != null);
            }
            catch { /* không bao giờ để interceptor làm crash app */ }
        }
    }

    /// <summary>
    /// Lưu trữ Stopwatch theo từng DbCommand (theo HashCode của object).
    /// Thread-safe vì dùng ConcurrentDictionary.
    /// </summary>
    internal static class TimerStore
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, Stopwatch>
            _timers = new System.Collections.Concurrent.ConcurrentDictionary<int, Stopwatch>();

        public static void Start(DbCommand command, Stopwatch sw)
        {
            _timers[RuntimeHelpers.GetHashCode(command)] = sw;
        }

        public static Stopwatch Stop(DbCommand command)
        {
            Stopwatch sw;
            _timers.TryRemove(RuntimeHelpers.GetHashCode(command), out sw);
            return sw;
        }
    }

    // Alias tránh phải thêm using
    internal static class RuntimeHelpers
    {
        public static int GetHashCode(object obj)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
