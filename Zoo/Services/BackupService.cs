using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Zoo.Infrastructure;

namespace Zoo.Services
{
    public class BackupService : IBackupService
    {
        public Task BackupDatabaseAsync(string filePath)
        {
            return Task.Run(() =>
            {
                string arguments = $"-U postgres -F c -b -v -f \"{filePath}\" Zoo";
                ExecuteProcess(ConfigManager.PgDumpPath, arguments);
            });
        }

        public Task RestoreDatabaseAsync(string filePath)
        {
            // Эта логика сложная и требует закрытия соединений, DROP/CREATE DATABASE
            // Для упрощения, предположим, что она выполняется pg_restore
            return Task.Run(() =>
            {
                // Закрываем все соединения
                // ...
                // Удаляем и создаем базу
                // ...
                string arguments = $"-U postgres -d postgres --clean --create \"{filePath}\"";
                ExecuteProcess(ConfigManager.PgRestorePath, arguments);
            });
        }

        private void ExecuteProcess(string executablePath, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                EnvironmentVariables = { ["PGPASSWORD"] = ConfigManager.PgPassword }
            };

            using (var process = Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new System.Exception($"Ошибка выполнения процесса: {executablePath}\n{error}");
                }
            }
        }
    }
}