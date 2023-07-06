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
    }
}
