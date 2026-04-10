using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using QuanLyDoanVien.Helpers;

namespace QuanLyDoanVien.Helpers
{

    public class EfTimingInterceptor : DbCommandInterceptor
    {
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

        private static void StartTimer(DbCommand command)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                command.GetHashCode(); 
                // Lưu tạm vào Dictionary thread-safe per-command
                TimerStore.Start(command, sw);
            }
            catch {  }
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
            catch {  }
        }
    }
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

    internal static class RuntimeHelpers
    {
        public static int GetHashCode(object obj)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
