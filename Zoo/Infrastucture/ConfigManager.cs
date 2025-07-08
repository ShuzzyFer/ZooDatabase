using System.Configuration;

namespace Zoo.Infrastructure
{
    public static class ConfigManager
    {
        // Приватное поле для пароля, чтобы он не "светился" везде
        private static string DbPassword => ConfigurationManager.AppSettings["DbPassword"];

        // Свойство, которое "на лету" собирает полную строку подключения
        public static string ConnectionString => 
            string.Format(ConfigurationManager.AppSettings["ConnectionStringTemplate"], DbPassword);

        // Пароль для утилит pg_dump/pg_restore
        public static string PgPassword => DbPassword;

        public static string PgDumpPath => ConfigurationManager.AppSettings["PgDumpPath"];
        public static string PgRestorePath => ConfigurationManager.AppSettings["PgRestorePath"];
        public static string SavedQueriesFilePath => ConfigurationManager.AppSettings["SavedQueriesFilePath"];
    }
}