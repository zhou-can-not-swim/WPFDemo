using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using WpfD.Model;

namespace WpfD.Helper
{
    public class DatabaseHelper
    {
        private string databasePath;
        private string connectionString;

        public DatabaseHelper()
        {
            databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.sqlite");
            connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS SoftWares(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    IconUrl TEXT NOT NULL,
                    DownloadUrl TEXT NOT NULL,
                    Details TEXT NOT NULL
                );";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // 添加软件链接
        public void AddSoftWare(SoftWare softWare)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO SoftWares (Name,IconUrl,DownloadUrl,Detail) VALUES (@Name, @IconUrl,@DownloadUrl,@Detail)";

                try
                {
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", softWare.Name);
                        command.Parameters.AddWithValue("@IconUrl", softWare.IconUrl);
                        command.Parameters.AddWithValue("@DownloadUrl", softWare.DownloadUrl);
                        command.Parameters.AddWithValue("@Detail", softWare.Detail);
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("添加成功！");
                }
                catch(SQLiteException ex)
                {
                    MessageBox.Show("添加失败！");
                }
                
            }
        }

        // 获取所有软件
        public List<SoftWare> GetAllSoftWares()
        {
            var softWares = new List<SoftWare>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM SoftWares";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        softWares.Add(new SoftWare
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            IconUrl = reader["IconUrl"].ToString(),
                            DownloadUrl = reader["DownloadUrl"].ToString(),
                            Detail = reader["Detail"].ToString(),
                        });
                    }
                }
            }
            return softWares;
        }

        // 更新软件
        public void UpdateSoftWare(SoftWare softWare)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE SoftWares SET Name = @Name, IconUrl = @IconUrl, DownloadUrl = @DownloadUrl WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", softWare.Id);
                    command.Parameters.AddWithValue("@Name", softWare.Name);
                    command.Parameters.AddWithValue("@IconUrl", softWare.IconUrl);
                    command.Parameters.AddWithValue("@DownloadUrl", softWare.DownloadUrl);
                    command.Parameters.AddWithValue("@Detail", softWare.DownloadUrl);
                    command.ExecuteNonQuery();
                }
            }
        }

        // 删除软件
        public void DeleteSoftWare(int id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM SoftWares WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // 根据ID获取用户
        public SoftWare GetSoftWareById(int id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM SoftWares WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SoftWare
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                IconUrl = reader["IconUrl"].ToString(),
                                DownloadUrl = reader["DownloadUrl"].ToString(),
                                Detail = reader["Detail"].ToString(),
                            };
                        }
                    }
                }
            }
            return null;
        }

        // 搜索软件（返回列表）
        public List<SoftWare> SearchSoftWares(string search)
        {
            var results = new List<SoftWare>();

            // 如果搜索条件为空，返回空列表或所有记录（根据需求）
            if (string.IsNullOrWhiteSpace(search))
            {
                return results; // 或者返回所有软件
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM SoftWares WHERE Name LIKE @Name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", "%" + search + "%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // 使用 while 读取所有结果
                        {
                            results.Add(new SoftWare
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                IconUrl = reader["IconUrl"]?.ToString(), // 空值安全
                                DownloadUrl = reader["DownloadUrl"]?.ToString(),
                                Detail = reader["Detail"]?.ToString(),
                            });
                        }
                    }
                }
            }
            return results;
        }
    }
}
