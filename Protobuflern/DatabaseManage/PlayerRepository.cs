using MySqlConnector;
using Protobuflern.Demo;

namespace Protobuflern.Database
{
    // player 表的操作：注册/登录/改资料。以后要加新表（对战记录、邮件等）就再建一个 Repository
    internal static class PlayerRepository
    {
        // 玩家注册：账号已存在或写入失败返回 false（新玩家分数从 0 开始）
        public static bool Register(int playerId, string password, string playerName)
        {
            using var connection = Db.Open();

            // 判断玩家是否存在
            string selectQuery = "SELECT * FROM player WHERE id=@id";
            using (var selectCommand = new MySqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@id", playerId);
                using var reader = selectCommand.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine("玩家已存在，请重新输入！");
                    return false;
                }
            }

            // 插入新数据
            string insertQuery = "INSERT INTO player (id, password, name, score) VALUES (@id, @password, @name, 0)";
            using (var insertCommand = new MySqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@id", playerId);
                insertCommand.Parameters.AddWithValue("@password", password);
                insertCommand.Parameters.AddWithValue("@name", playerName);

                int rowsAffected = insertCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("玩家注册成功！");
                    return true;
                }
            }
            return false;
        }

        // 玩家登录：成功返回玩家状态（昵称/分数），账号或密码不对返回 null
        public static PlayerState? Login(int playerId, string password)
        {
            using var connection = Db.Open();

            string query = "SELECT password, name, score FROM player WHERE id=@id";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", playerId);
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    Console.WriteLine("登录失败：玩家不存在！");
                    return null;
                }

                if (reader.GetString("password") != password)
                {
                    Console.WriteLine("登录失败：密码错误！");
                    return null;
                }

                Console.WriteLine("登录成功！");
                return new PlayerState
                {
                    Name = reader.GetString("name"),
                    Score = reader.GetInt32("score")
                };
            }
        }

        // 修改玩家昵称
        public static bool UpdateName(int playerId, string newName)
        {
            using var connection = Db.Open();

            if (!PlayerExists(connection, playerId)) return false;

            string updateQuery = "UPDATE player SET name=@name WHERE id=@id";
            using (var updateCommand = new MySqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@id", playerId);
                updateCommand.Parameters.AddWithValue("@name", newName);

                int rowsAffected = updateCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("修改成功！");
                    return true;
                }
            }
            return false;
        }

        // 修改玩家分数（正数加分，负数减分）
        public static bool UpdateScore(int playerId, int scoreDelta)
        {
            using var connection = Db.Open();

            if (!PlayerExists(connection, playerId)) return false;

            string updateQuery = "UPDATE player SET score = score + @scoreDelta WHERE id=@id";
            using (var updateCommand = new MySqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@id", playerId);
                updateCommand.Parameters.AddWithValue("@scoreDelta", scoreDelta);

                int rowsAffected = updateCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"修改成功！玩家 {playerId} 的分数 {(scoreDelta >= 0 ? "增加" : "减少")} {Math.Abs(scoreDelta)} 分");
                    return true;
                }
            }
            Console.WriteLine("修改失败：未找到该玩家！");
            return false;
        }

        // 修改玩家密码
        public static bool UpdatePassword(int playerId, string newPassword)
        {
            using var connection = Db.Open();

            if (!PlayerExists(connection, playerId)) return false;

            string updateQuery = "UPDATE player SET password=@password WHERE id=@id";
            using (var updateCommand = new MySqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@id", playerId);
                updateCommand.Parameters.AddWithValue("@password", newPassword);

                int rowsAffected = updateCommand.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("修改成功！");
                    return true;
                }
            }
            return false;
        }

        // 判断玩家是否存在
        private static bool PlayerExists(MySqlConnection connection, int playerId)
        {
            string checkQuery = "SELECT * FROM player WHERE id=@id";
            using (var command = new MySqlCommand(checkQuery, connection))
            {
                command.Parameters.AddWithValue("@id", playerId);
                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    Console.WriteLine("修改失败：玩家不存在！");
                    return false;
                }
            }
            return true;
        }
    }
}
