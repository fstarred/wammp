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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YempPluginContracts
{
    /// <summary>
    /// Interaction logic for UserControlBase.xaml
    /// </summary>
    public partial class UserControlBase : UserControl
    {
        public UserControlBase()
        {
            InitializeComponent();
            this.DataContextChanged += UserControlBase_DataContextChanged;            
        }

        private void UserControlBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IPlugin)
            {
                ctx = (IPlugin)DataContext;

                if (!ctx.CanLoadSave())
                {
                    this.LoadBtn.IsEnabled = false;
                    this.SaveBtn.IsEnabled = false;
                }
            }
        }
        
        
        IPlugin ctx;
        
        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ctx.Save();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            ctx.Load();
        }

        private void Version_Click(object sender, RoutedEventArgs e)
        {
            ctx.IsVersionUptoDate();
        }
    }
}
