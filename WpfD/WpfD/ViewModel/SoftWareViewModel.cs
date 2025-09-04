using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public event PropertyChangedEventHandler? PropertyChanged;
        //控制页面关闭的命令
        public event EventHandler RequestClose;

        public ObservableCollection<SoftWare> SoftWares { get; set; }
        public ICommand AddSoftWareCommand { get; set; }
        //模糊搜索
        public ICommand DoSearch { get; set; }

        // 在 搜索完成事件
        public event Action<List<SoftWare>> SearchCompleted;

        public SoftWareViewModel()
        {
            AddSoftWareCommand = new RelayCommand(AddSoftWare, CanAddUser);
            DoSearch = new RelayCommand(DoSearchMethod, CanDoSearchMethod);

        }
        private bool CanDoSearchMethod(object obj)
        {
            return true;
        }

        //查找软件的方法
        private void DoSearchMethod(object obj)
        {
            try
            { 
                List<SoftWare> softWares = App.Database.SearchSoftWares(_search);
                SearchCompleted?.Invoke(softWares);
            }
            catch (Exception ex)
            {
                MessageBox.Show("查找软件失败，多找找自己的问题！！！");
            }


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

                // 触发关闭事件
                RequestClose?.Invoke(this, EventArgs.Empty);

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

        //模糊搜索
        private string? _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    _search = value;
                    OnPropertyChanged(nameof(Search));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
