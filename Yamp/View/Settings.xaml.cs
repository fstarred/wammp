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
using Yemp.ViewModel;

namespace Yemp.View
{
    /// <summary>
    /// Interaction logic for ViewSettings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void aboutbtn_Click(object sender, RoutedEventArgs e)
        {
            Yamp.View.About view = new Yamp.View.About();
            view.Owner = this;
            view.ShowDialog();
        }
    }
}
