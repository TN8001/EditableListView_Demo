using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EditableListView_Demo
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileModel> Samples { get; set; } = new ObservableCollection<FileModel>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            AddSample(100);
        }

        private void AddSample(int iteration)
        {
            for(var i = 0; i < iteration; i++)
                Samples.Add(new FileModel());
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(checkBox.IsChecked == true) return;

            var listViewItem = sender as ListViewItem;
            var fileModel = listViewItem?.DataContext as FileModel;
            MessageBox.Show(fileModel?.Name, "DoubleClick");
        }
    }
}
