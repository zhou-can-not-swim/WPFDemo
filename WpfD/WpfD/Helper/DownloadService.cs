using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;

namespace WpfD.Helper
{
    public class DownloadService
    {
        private static readonly HttpClient _httpClient;

        static DownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromHours(5); // 设置更长的超时时间
                                                         // 增加默认连接限制
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        public static async Task DownloadFileAsync(string url, string filePath,
    IProgress<(double, long, long)> progress, CancellationToken cancellationToken, string downloadId)
        {
            using (var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                    FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    var totalBytesRead = 0L;
                    var bytesRead = 0;
                    var lastUpdateTime = DateTime.MinValue;

                    //循环读取网络流数据到缓冲区
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        //将缓冲区中的数据写入本地文件
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                        totalBytesRead += bytesRead;//共下载了多少字节

                        if (canReportProgress && (DateTime.Now - lastUpdateTime).TotalMilliseconds > 100)
                        {
                            var progressPercentage = (double)totalBytesRead / totalBytes * 100;

                            // 报告进度，更新进度的值，里面是元组
                            progress?.Report((progressPercentage, totalBytesRead, totalBytes));

                            // 保存进度到应用程序级别
                            if (App.DownloadProgresses.ContainsKey(downloadId))
                            {
                                App.DownloadProgresses[downloadId] = (progressPercentage, totalBytesRead, totalBytes);
                            }
                            else
                            {
                                App.DownloadProgresses.Add(downloadId, (progressPercentage, totalBytesRead, totalBytes));
                            }

                            lastUpdateTime = DateTime.Now;
                        }
                    }

                    // 下载完成后移除进度信息
                    App.DownloadProgresses.Remove(downloadId);
                    App.DownloadIdToSoftwareIdMap.Remove(downloadId);
                }
            }
        }

        // 添加辅助方法生成唯一的下载ID
        private static string GetDownloadId(string url, string filePath)
        {
            return $"{url}_{filePath}".GetHashCode().ToString();
        }

        ////原来的版本
        //private async Task DownloadFileAsync(string url, ProgressBar progressBar, TextBlock textBlock, string filePath, CancellationToken cancellationToken)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        // 设置超时时间
        //        httpClient.Timeout = TimeSpan.FromMinutes(30);

        //        //得到的响应数据
        //        using (var response = await httpClient.GetAsync(
        //            url,
        //            HttpCompletionOption.ResponseHeadersRead,
        //            cancellationToken))
        //        {
        //            response.EnsureSuccessStatusCode();// 确保响应成功

        //            var totalBytes = response.Content.Headers.ContentLength ?? -1L;//获取文件总大小，如果未知则为-1
        //            var canReportProgress = totalBytes != -1; //是否可以报告进度（只有在知道文件大小时才能计算百分比）

        //            using (var contentStream = await response.Content.ReadAsStreamAsync())
        //            //FileMode.Create: 创建新文件，如果存在则覆盖
        //            //FileAccess.Write: 只写访问
        //            //FileShare.None: 不允许其他进程访问
        //            //8192: 缓冲区大小
        //            //true: 使用异步I / O
        //            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
        //            {
        //                var buffer = new byte[8192];// 8KB缓冲区
        //                var totalBytesRead = 0L; // 总共已读取的字节数
        //                var bytesRead = 0;      // 每次读取的字节数

        //                //从buffer数组的第0个位置开始存放数据,最多读取buffer.Length大小（装满整个水桶）
        //                //Stream流会自动保留当前读取的位置，下次读取的时候会从上次读取的位置开始读取
        //                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        //                {
        //                    //从读取的buffer数组的第0个位置开始来写入
        //                    //bytesRead: 实际要写入的字节数,就是读取的字节数
        //                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

        //                    totalBytesRead += bytesRead;//累计计数: 记录总共读取的字节数

        //                    if (canReportProgress)//判断是否可以报告进度
        //                    {
        //                        var progressPercentage = (double)totalBytesRead / totalBytes * 100;//计算进度百分比
        //                        // 通过名称查找控件

        //                        if (progressBar != null)
        //                        {
        //                            progressBar.Value = progressPercentage;
        //                            textBlock.Text = progressPercentage.ToString("F1") + "%";
        //                        }

        //                    }
        //                    else
        //                    {
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
