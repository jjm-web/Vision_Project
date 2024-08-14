using System.Windows;
using Vision_Project.ViewModels;
namespace Vision_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

    }
}
