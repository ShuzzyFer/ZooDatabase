using System;
using System.Windows.Forms;
using Zoo.DataAccess;
using Zoo.Infrastructure;
using Zoo.Services;

namespace Zoo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // --- Сборка зависимостей (Dependency Injection) ---
            try
            {
                // 1. Создаем экземпляры низкоуровневых классов
                var repository = new PostgresRepository();
                var fileStorage = new FileStorage();

                // 2. Создаем сервисы, передавая им зависимости
                var databaseService = new DatabaseService(repository);
                var backupService = new BackupService();
                var queryService = new QueryService(fileStorage);

                // 3. Создаем главную форму, передавая ей сервисы
                var mainForm = new Form1(databaseService, backupService, queryService);

                Application.Run(mainForm);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}