using StarLine.AutoDimension.Core.ViewModels;

namespace StarLine.AutoDimension.Core.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        public OptionsWindow(OptionsWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        public OptionsWindowViewModel GetViewModel()
        {
            return (OptionsWindowViewModel)DataContext;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void ReferenceView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ReferenceView_Loaded_1(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
