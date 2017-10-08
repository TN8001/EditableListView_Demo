using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EditableListView_Demo
{
    public partial class Window1 : Window
    {
        public ObservableCollection<FileModel> Samples { get; set; } = new ObservableCollection<FileModel>();

        public Window1()
        {
            InitializeComponent();
            DataContext = this;

            AddSample(1000);
        }

        private void AddSample(int iteration)
        {
            for(var i = 0; i < iteration; i++)
                Samples.Add(new FileModel());
        }

        protected void List_DoubleClick(object sender, MouseButtonEventArgs e)
            => Console.Beep();
    }
}
