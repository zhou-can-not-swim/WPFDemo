using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfD
{
    //一些导航的行为封装
    public static class NavigationService
    {
        public static Frame MainFrame { get; set; }

        public static void NavigateTo<T>() where T : Page, new()
        {
            MainFrame?.Navigate(new T());
        }

        public static bool CanGoBack => MainFrame?.CanGoBack ?? false;
        public static void GoBack() => MainFrame?.GoBack();
    }
}
