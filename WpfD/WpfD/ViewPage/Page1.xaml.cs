using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfD.Dialog;
using WpfD.Helper;
using WpfD.Model;
using WpfD.ViewModel;

namespace WpfD.ViewPage
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class Page1 : Page
    {
        private CancellationTokenSource _cancellationTokenSource;

        // 1在类级别添加字典
        private Dictionary<string, (ProgressBar ProgressBar, TextBlock TextBlock)> _progressBars = 
            new Dictionary<string, (ProgressBar, TextBlock)>();


        // 在类级别添加已加载标记
        private bool _isPageLoaded = false;
        private List<string> _activeDownloadIds = new List<string>();

        public Page1()
        {
            InitializeComponent();
            var viewModel = new SoftWareViewModel();//设置数据源,会有命令的绑定
            this.DataContext = viewModel;
            // 订阅搜索完成事件
            viewModel.SearchCompleted += OnSearchCompleted;

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = false;
            // 注意：不要在这里取消下载，只移除UI引用
            _activeDownloadIds.Clear();
        }
        // 辅助方法：从下载ID提取软件ID
        private string GetSoftwareIdFromDownloadId(string downloadId)
        {
            // 根据你的ID生成逻辑实现
            // 例如，如果你使用软件ID作为下载ID的一部分
            return downloadId.Split('_')[0]; // 假设格式为 "softwareId_urlHash"
        }


        //搜索完成事件处理程序
        private void OnSearchCompleted(List<SoftWare> softWares)
        {
            // 确保在 UI 线程上执行
            Dispatcher.Invoke(() =>
            {
                // 先清空现有内容（如果需要）
                ImageContainer.Children.Clear();
                _progressBars.Clear();
                // 添加新的搜索结果
                foreach (var item in softWares)
                {
                    AddImage(item);
                }
            });
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //得到所有的software信息
            foreach (var item in App.Database.GetAllSoftWares())
            {
                AddImage(item);
            }
        }

        private void AddImage(SoftWare softWare)
        {
            // 检查是否已经添加过这个软件
            if (_progressBars.ContainsKey(softWare.Id.ToString()))
            {
                return;
            }
            // 创建边框容器
            Border border = new Border//设置边框
            {
                Margin = new Thickness(5),//厚度为5
                Background = Brushes.White, //背景颜色为白色
                CornerRadius = new CornerRadius(5), //圆角为5
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,    //模糊半径为10,设置过大会很卡
                    Color = Colors.Pink, //阴影颜色
                    Opacity = 0.3,      //透明度为0.3
                    ShadowDepth = 2     //阴影深度为2
                }
            };
            ProgressBar progressBar = new ProgressBar()
            {
                Name = "ProgressBar",
                Tag = "ProgressBar"+ softWare.Id,
                Margin = new Thickness(5),
                Height = 2,
                Value = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch // 让进度条填充整个单元格
            };
            TextBlock progressText = new TextBlock()
            {
                Name = "ProgressText",
                Tag = "ProgressText" + softWare.Id,
                Text = "0%",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right // 文本右对齐
            };
            //progressBar.Visibility = Visibility.Hidden;
            //progressBar.Visibility = Visibility.Hidden;

            // 2添加到字典

            _progressBars.Add(softWare.Id.ToString(), (progressBar, progressText));

            // 创建网格布局
            Grid grid = new Grid()
            {
                Name = "Grid",
                RowDefinitions =
                {
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },//1: 比例系数（权重）GridUnitType.Star: 单位类型，表示比例分配
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) }
                }
            };

            // 创建图片
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(softWare.IconUrl)),//设置图片源
                Stretch = Stretch.Uniform,//图片填充方式 均匀填充
                Style = this.TryFindResource("HoverImageStyle") as Style//应用悬停样式
            };

            // 创建遮罩层
            Border overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), // 设置阴影的背景色
                Opacity = 0,                                                    // 初始完全透明（不可见）
                VerticalAlignment = VerticalAlignment.Bottom,                   // 垂直对齐到底部
                Height = 0                                                      // 初始高度为0
            };

            // 创建按钮容器
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // 创建下载按钮
            Button downloadButton = new Button
            {
                Content = "下载",
                Style = this.TryFindResource("ImageButtonStyle") as Style,  // 尝试从资源中查找并应用名为"ImageButtonStyle"的样式
                Tag = new
                {
                    Url = softWare.DownloadUrl,
                    Id = softWare.Id
                }                                    // 将imageUrl存储在按钮的Tag属性中（通常用于存储关联数据）
            };
            downloadButton.Click += DownloadButton_Click;                   // 为按钮的Click事件添加事件处理程序

            // 创建打开按钮
            Button openButton = new Button
            {
                Content = "打开网页",
                Style = this.TryFindResource("ImageButtonStyle") as Style,
                Tag = softWare.Detail
            };
            openButton.Click += OpenButton_Click;

            // 添加按钮到面板
            buttonPanel.Children.Add(downloadButton);
            buttonPanel.Children.Add(openButton);

            // 将按钮面板添加到遮罩层
            overlay.Child = buttonPanel;

            // 添加到网格
            // 设置Grid位置
            Grid.SetRow(progressBar, 0);
            Grid.SetColumn(progressBar, 0);
            Grid.SetRow(progressText, 0);
            Grid.SetColumn(progressText, 1);

            // 添加到网格
            grid.Children.Add(progressBar);
            grid.Children.Add(progressText);

            Grid.SetRow(image, 1);
            Grid.SetRow(overlay, 1);
            grid.Children.Add(image);
            grid.Children.Add(overlay);

            // 添加到边框
            border.Child = grid;

            // 添加鼠标事件  += 是 C# 中的事件订阅操作符，用于将方法或事件处理程序附加到事件上。
            border.MouseEnter += (s, e) => ShowButtons(overlay);
            border.MouseLeave += (s, e) => HideButtons(overlay);

            // 添加到WrapPanel
            ImageContainer.Children.Add(border);
        }

        private void ShowButtons(Border overlay)
        {
            // 创建动画显示按钮创建一个高度动画：从当前高度动画变化到100像素动画时长300毫秒
            DoubleAnimation heightAnimation = new DoubleAnimation(100, new Duration(TimeSpan.FromMilliseconds(300)));
            //创建一个透明度动画：从当前透明度动画变化到完全不透明(1) 动画时长300毫秒
            DoubleAnimation opacityAnimation = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(300)));

            //将高度动画应用到Border控件的Height属性
            overlay.BeginAnimation(Border.HeightProperty, heightAnimation);
            overlay.BeginAnimation(Border.OpacityProperty, opacityAnimation);
        }

        private void HideButtons(Border overlay)
        {
            // 创建动画隐藏按钮
            DoubleAnimation heightAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));
            DoubleAnimation opacityAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300)));

            overlay.BeginAnimation(Border.HeightProperty, heightAnimation);
            overlay.BeginAnimation(Border.OpacityProperty, opacityAnimation);
        }

        //下载按钮点击事件
        //private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Button button = sender as Button;

        //    // Tag使用的是匿名对象
        //    var data = button.Tag as dynamic; // 使用 dynamic
        //    string downloadUrl = data.Url;
        //    string id = data.Id.ToString();
        //    //MessageBox.Show($"开始下载图片: {iconUrl}", "下载", MessageBoxButton.OK, MessageBoxImage.Information);

        //    //OpenFolderDialog dialog = new();//打开文件夹

        //    var dialog = new SaveFileDialog();
        //    dialog.Filter = "可执行文件|*.exe|所有文件|*.*";
        //    if (dialog.ShowDialog() == true)
        //    {
        //        //dialog.Multiselect = false;//允许多选   false
        //        dialog.Title = "Select a folder";//标题

        //        string fullPathToFolder = dialog.FileName;//可以下载到这个路径下

        //        // 初始化取消令牌
        //        _cancellationTokenSource = new CancellationTokenSource();
        //        var cancellationToken = _cancellationTokenSource.Token;

        //        // 更新UI状态
        //        button.IsEnabled = false;
        //        //找到进度条控件的名字


        //        // 通过名称查找控件
        //        //ProgressBar progressBar = ImageContainer.Children
        //        //.OfType<ProgressBar>()
        //        //.FirstOrDefault(pb => pb.Tag?.ToString() == "ProgressBar" + button.Content.ToString().Split("-")[1]);

        //        // 3在DownloadButton_Click中直接使用
        //        string index = id;
        //        ProgressBar progressBar = _progressBars[index].ProgressBar;
        //        TextBlock textBlock = _progressBars[index].TextBlock;

        //        if (progressBar != null)
        //        {
        //            progressBar.Value = 0;
        //        }
        //        button.Content = "开始下载...";
        //        //展示进度条
        //        progressBar.Visibility= Visibility.Visible;
        //        try
        //        {
        //            await DownloadFileAsync(downloadUrl, progressBar, textBlock,fullPathToFolder, cancellationToken);
        //            MessageBox.Show("下载完成！");
        //            button.Content = "下载";
        //        }
        //        catch (OperationCanceledException)//取消操作
        //        {
        //            MessageBox.Show("下载已取消,需手动删除已下载的部分文件才能继续下载");
        //            // 删除部分下载的文件
        //            if (File.Exists(fullPathToFolder))
        //            {
        //                File.Delete(fullPathToFolder);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("下载失败");
        //        }
        //        finally
        //        {
        //            // 恢复UI状态
        //            button.IsEnabled = true;
        //            _cancellationTokenSource?.Dispose();//确保释放取消令牌
        //            _cancellationTokenSource = null;
        //        }

        //    }

        //}

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string Detail = button.Tag as string;


            // 打开默认浏览器并导航到指定URL
            Process.Start(new ProcessStartInfo
            {
                FileName = Detail,
                UseShellExecute = true
            });
        }


        private async Task DownloadFileAsync(string url, ProgressBar progressBar,TextBlock textBlock, string filePath, CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                // 设置超时时间
                httpClient.Timeout = TimeSpan.FromMinutes(30);

                //得到的响应数据
                using (var response = await httpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken))
                {
                    response.EnsureSuccessStatusCode();// 确保响应成功

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;//获取文件总大小，如果未知则为-1
                    var canReportProgress = totalBytes != -1; //是否可以报告进度（只有在知道文件大小时才能计算百分比）

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    //FileMode.Create: 创建新文件，如果存在则覆盖
                    //FileAccess.Write: 只写访问
                    //FileShare.None: 不允许其他进程访问
                    //8192: 缓冲区大小
                    //true: 使用异步I / O
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];// 8KB缓冲区
                        var totalBytesRead = 0L; // 总共已读取的字节数
                        var bytesRead = 0;      // 每次读取的字节数

                        //从buffer数组的第0个位置开始存放数据,最多读取buffer.Length大小（装满整个水桶）
                        //Stream流会自动保留当前读取的位置，下次读取的时候会从上次读取的位置开始读取
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            //从读取的buffer数组的第0个位置开始来写入
                            //bytesRead: 实际要写入的字节数,就是读取的字节数
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                            totalBytesRead += bytesRead;//累计计数: 记录总共读取的字节数

                            if (canReportProgress)//判断是否可以报告进度
                            {
                                var progressPercentage = (double)totalBytesRead / totalBytes * 100;//计算进度百分比
                                // 通过名称查找控件

                                if (progressBar != null)
                                {
                                    progressBar.Value = progressPercentage;
                                    textBlock.Text = progressPercentage.ToString("F1")+"%";
                                }

                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
        }

        private void AddDownloadUrl(object sender, RoutedEventArgs e)
        {
            new AddDownloadUrlWindow().ShowDialog();
        }

        //private async void CancelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Button? button = sender as Button;
        //    _cancellationTokenSource?.Cancel();
        //    button.Content = "正在取消下载...";
        //}

        //优化之后的下载方案：
        // 刷新进度条显示 - 关键修改
        private void RefreshProgressBars()
        {
            // 检查所有活跃下载
            foreach (var downloadId in App.ActiveDownloads.Keys.ToList())
            {
                // 如果这个下载属于当前页面
                if (App.DownloadIdToSoftwareIdMap.TryGetValue(downloadId, out var softwareId))
                {
                    // 添加到本页面的活跃下载列表
                    if (!_activeDownloadIds.Contains(downloadId))
                    {
                        _activeDownloadIds.Add(downloadId);
                    }

                    // 更新进度显示
                    if (App.DownloadProgresses.TryGetValue(downloadId, out var progressInfo))
                    {
                        if (_progressBars.TryGetValue(softwareId, out var progressControls))
                        {
                            progressControls.ProgressBar.Value = progressInfo.Percentage;
                            progressControls.TextBlock.Text = $"{progressInfo.Percentage:F1}%";
                            progressControls.ProgressBar.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        // 下载正在进行但没有进度信息（刚开始下载）
                        if (_progressBars.TryGetValue(softwareId, out var progressControls))
                        {
                            progressControls.ProgressBar.Value = 0;
                            progressControls.TextBlock.Text = "0%";
                            progressControls.ProgressBar.Visibility = Visibility.Visible;
                        }
                    }
                }
            }

            // 清理已完成或取消的下载
            foreach (var downloadId in _activeDownloadIds.ToList())
            {
                if (!App.ActiveDownloads.ContainsKey(downloadId))
                {
                    _activeDownloadIds.Remove(downloadId);
                }
            }
        }

        // 优化下载按钮点击事件
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var data = button.Tag as dynamic;
            string downloadUrl = data.Url;
            string softwareId = data.Id.ToString();

            var dialog = new SaveFileDialog();
            dialog.Filter = "可执行文件|*.exe|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                string fullPathToFolder = dialog.FileName;//fullPathToFolder(自己写的路径如)=C:\\Users\\qiyu.zhou\\Downloads\\jojoba.exe"

                // 生成唯一的下载ID（使用GUID确保唯一性）
                string downloadId = Guid.NewGuid().ToString();

                //用于管理和控制异步下载任务的取消操作。
                var cts = new CancellationTokenSource();
                App.ActiveDownloads[downloadId] = cts;

                // 建立下载ID到软件ID的映射
                App.DownloadIdToSoftwareIdMap[downloadId] = softwareId;//也就是哪个软件正在下载（如id(git)=3<---->"fad7eafa-5d14-4407-b409-16823864c293"）

                _activeDownloadIds.Add(downloadId);//正在运行的下载ID放到本页面的集合中去,也就是正在下载的软件

                button.IsEnabled = false;

                //尝试从 _progressBars 字典中查找 softwareId 对应的值
                //如果找到：将找到的值通过 out 参数赋给 progressControls 变量，并返回 true
                //如果没找到：返回 false，progressControls 变量会被赋为默认值（null）
                //其实也就是对全局的字典_progressBars进行赋值
                if (_progressBars.TryGetValue(softwareId, out var progressControls))
                {
                    progressControls.ProgressBar.Value = 0;
                    progressControls.ProgressBar.Visibility = Visibility.Visible;
                    progressControls.TextBlock.Text = "0%";
                }

                button.Content = "开始下载...";

                try
                {
                    //Progress<T> 和 元组 +Lambda 表达式
                    var progress = new Progress<(double Percentage, long BytesReceived, long TotalBytes)>(value =>
                    {
                        if (!_isPageLoaded) return;

                        if (_progressBars.TryGetValue(softwareId, out var controls))
                        {
                            controls.ProgressBar.Value = value.Percentage;
                            controls.TextBlock.Text = $"{value.Percentage:F1}%";
                        }
                    });

                    // 传递downloadId参数
                    await DownloadService.DownloadFileAsync(downloadUrl, fullPathToFolder, progress, cts.Token, downloadId);

                    if (_isPageLoaded)
                    {
                        MessageBox.Show("下载完成！");
                        button.Content = "下载";
                        button.IsEnabled = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (_isPageLoaded)
                    {
                        MessageBox.Show("下载已取消");
                        if (File.Exists(fullPathToFolder))
                        {
                            File.Delete(fullPathToFolder);
                        }
                        button.Content = "下载";
                        button.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    if (_isPageLoaded)
                    {
                        MessageBox.Show($"下载失败: {ex.Message}");
                        button.Content = "下载";
                        button.IsEnabled = true;
                    }
                }
                finally
                {
                    App.ActiveDownloads.Remove(downloadId);
                    App.DownloadIdToSoftwareIdMap.Remove(downloadId);
                    _activeDownloadIds.Remove(downloadId);
                    cts.Dispose();
                }
            }
        }

        // 修改页面加载事件
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = true;

            // 恢复页面时重新绑定进度显示
            RefreshProgressBars();

            // 只在第一次加载时添加图片
            if (ImageContainer.Children.Count == 0)
            {
                foreach (var item in App.Database.GetAllSoftWares())
                {
                    AddImage(item);
                }
            }
        }


        // 添加取消下载的方法
        private void CancelDownload(string id)
        {
            if (App.ActiveDownloads.TryGetValue(id, out var cts))
            {
                cts.Cancel();
            }
        }


        // 计算下载速度的辅助方法（可选）
        private string CalculateSpeed(long bytesReceived, DateTime startTime)
        {
            var elapsed = DateTime.Now - startTime;
            if (elapsed.TotalSeconds < 0.1) return "计算中...";

            double bytesPerSecond = bytesReceived / elapsed.TotalSeconds;

            if (bytesPerSecond > 1024 * 1024)
                return $"{(bytesPerSecond / (1024 * 1024)):F1} MB/s";
            else if (bytesPerSecond > 1024)
                return $"{(bytesPerSecond / 1024):F1} KB/s";
            else
                return $"{bytesPerSecond:F1} B/s";
        }
    }
}
