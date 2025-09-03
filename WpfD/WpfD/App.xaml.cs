using System.Configuration;
using System.Data;
using System.Windows;
using WpfD.Helper;

namespace WpfD
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DatabaseHelper Database { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化数据库
            InitializeDatabase();

            // 其他启动逻辑...
        }

        private void InitializeDatabase()
        {
            try
            {
                Database = new DatabaseHelper();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库初始化失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 清理资源
            base.OnExit(e);
        }
    }

}
