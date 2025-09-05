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
        //下载优化方案
        public static Dictionary<string, CancellationTokenSource> ActiveDownloads { get; }
     = new Dictionary<string, CancellationTokenSource>();

        // 新增：存储下载进度信息
        public static Dictionary<string, (double Percentage, long BytesReceived, long TotalBytes)> DownloadProgresses { get; } =
            new Dictionary<string, (double, long, long)>();

        // 新增：存储下载ID到软件ID的映射
        public static Dictionary<string, string> DownloadIdToSoftwareIdMap { get; } =
            new Dictionary<string, string>();

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
