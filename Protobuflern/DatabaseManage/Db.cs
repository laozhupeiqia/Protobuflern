using MySqlConnector;

namespace Protobuflern.Database
{
    // 数据库连接辅助：连接串和打开连接只写一遍，各 Repository 复用它
    internal static class Db
    {
        private const string ConnectionString =
            "server=localhost;user=root;database=jingziqi;port=3306;password=123456;CharSet=utf8mb4";

        // 建一条已打开的连接（连接池会自动复用物理连接），用完记得 Dispose（using 即可）
        public static MySqlConnection Open()
        {
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
