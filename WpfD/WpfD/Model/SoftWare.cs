using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfD.Model
{
    // software数据模型类
    public class SoftWare
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string Detail { get; set; }
    }
}
