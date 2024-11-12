using System;
using System.IO;
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
using Newtonsoft.Json.Linq;
using System.Reflection;



namespace StarLine.AutoDimension.Core.Services
{
    /// <summary>
    /// Interaction logic for
    /// </summary>
    public partial class CreateElevationUI : Window
    {
        private List<string> wall_name_ui;

        public List<string> selected_wall_name {  get; private set; } = new List<string>();

        public double elevation_offsetfromwall { get; set; } = new double();
        public double elevation_viewdepth { get; set; } = new double();
        public double elevation_widthoffset { get; set; } = new double();
        public double elevation_heightoffset { get; set; } = new double();


        public CreateElevationUI (List<string> wall_name)
        {

            InitializeComponent();
            wall_name_ui = wall_name;
            AddWallListBox();
            LoadJsonData();          
        }


        private void LoadJsonData()
        {
            try
            {
                string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string jsonFilePath = System.IO.Path.Combine(assemblyPath, "default.json");

                string jsonData = File.ReadAllText(jsonFilePath);
                JObject jsonObject = JObject.Parse(jsonData);

                //setting default value from Json to XAML UI
                textboxeleoffset.Text   = jsonObject["CreateElevationViews"][0]["ElevationOffsetFromWall"].ToString();
                textboxeledepth.Text    = jsonObject["CreateElevationViews"][0]["ElevationViewDepth"].ToString();
                textboxelewidth.Text    = jsonObject["CreateElevationViews"][0]["ElevationViewWidthOffset"].ToString();
                textboxeleheight.Text   = jsonObject["CreateElevationViews"][0]["ElevationViewHeightOffset"].ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error Loading Json: ", ex.Message);
            }

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
            elevation_offsetfromwall    = double.Parse(textboxeleoffset.Text);
            elevation_viewdepth         = double.Parse(textboxeledepth.Text);
            elevation_widthoffset       = double.Parse(textboxelewidth.Text);
            elevation_heightoffset      = double.Parse(textboxeleheight.Text);

            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
