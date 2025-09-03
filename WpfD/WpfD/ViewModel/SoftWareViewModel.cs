using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfD.Model;
using WpfD.Utils;

namespace WpfD.ViewModel
{
    //INotifyPropertyChanged接口用于实现数据绑定中的属性更改通知,没有它，扩展123没办法实现        前端---->后端
    //当绑定到UI元素的数据源中的属性值发生更改时，INotifyPropertyChanged接口可以通知UI元素更新。
    public class SoftWareViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SoftWare> SoftWares { get; set; }
        public ICommand AddSoftWareCommand { get; set; }

        public SoftWareViewModel()
        {
            AddSoftWareCommand = new RelayCommand(AddSoftWare, CanAddUser);

        }

        private bool CanAddUser(object obj)
        {
            return true;
        }

        //添加软件的方法
        private void AddSoftWare(object obj)
        {
            try
            {
                SoftWare softWare = new SoftWare();
                softWare.Name = Name;
                softWare.IconUrl = IconUrl;
                softWare.DownloadUrl = DownloadUrl;
                softWare.Detail = Detail;
                App.Database.AddSoftWare(softWare);
            }
            catch(Exception ex)
            {
                MessageBox.Show("添加软件失败，多找找自己的问题！！！");
            }

            
        }

        //和INotifyPropertyChanged配套使用
        //还需要添加Name和Email,这是和前端一致的
        private string? _name;
        public string? Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string? _iconUrl;
        public string? IconUrl
        {
            get { return _iconUrl; }
            set
            {
                if (_iconUrl != value)
                {
                    _iconUrl = value;
                    OnPropertyChanged(nameof(IconUrl));
                }
            }
        }

        private string? _downloadUrl;
        public string? DownloadUrl
        {
            get { return _downloadUrl; }
            set
            {
                if (_downloadUrl != value)
                {
                    _downloadUrl = value;
                    OnPropertyChanged(nameof(DownloadUrl));
                }
            }
        }

        private string? _detail;
        public string? Detail
        {
            get { return _detail; }
            set
            {
                if (_detail != value)
                {
                    _detail = value;
                    OnPropertyChanged(nameof(Detail));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
