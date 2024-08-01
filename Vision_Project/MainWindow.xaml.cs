using Emgu.CV;
using System.Windows;

namespace Vision_Project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

        }

        public Image CameraImage { get; set; }
    }
}
