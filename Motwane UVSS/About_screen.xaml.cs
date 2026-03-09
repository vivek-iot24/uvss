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

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for About_screen.xaml
    /// </summary>
    public partial class About_screen : Window
    {
        public About_screen()
        {
            InitializeComponent();
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            this.Close();
        }
    }
}
