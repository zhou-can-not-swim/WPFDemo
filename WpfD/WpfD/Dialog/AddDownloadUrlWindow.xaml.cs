using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfD.ViewModel;

namespace WpfD.Dialog
{
    /// <summary>
    /// AddDownloadUrlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddDownloadUrlWindow : Window
    {
        public AddDownloadUrlWindow()
        {
            InitializeComponent();
            SoftWareViewModel softWareViewModel = new SoftWareViewModel();
            this.DataContext = softWareViewModel;
        }
    }
}
