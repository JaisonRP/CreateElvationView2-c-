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


namespace StarLine.AutoDimension.Core.Services
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private List<string> wall_name_ui;

        public List<string> selected_wall_name {  get; private set; } = new List<string>(); 

        public Window1(List<string> wall_name)
        {
            InitializeComponent();
            wall_name_ui = wall_name;
            AddWallListBox();
        }

        private void AddWallListBox()
        {
            foreach(string wall in wall_name_ui)
            {
                listboxui.Items.Add(wall);
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            selected_wall_name.Clear();

            foreach(var item in listboxui.SelectedItems)
            {
                selected_wall_name.Add(item.ToString());
            }
            
            Close();
        }
    }
}
