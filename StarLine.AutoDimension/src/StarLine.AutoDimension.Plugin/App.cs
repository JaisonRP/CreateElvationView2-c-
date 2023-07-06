using Autodesk.Revit.UI;
using Prism.DryIoc;
using Prism.Ioc;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.IO;
using StarLine.AutoDimension.Core.Utils;
using StarLine.AutoDimension.Core.Views;
using StarLine.AutoDimension.Plugin.Commands;

namespace StarLine.AutoDimension.Plugin
{
    public class App : PrismBootstrapper
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<OptionsWindow>();
        }

        protected override void OnInitialized()
        {
        }

        public void InitializeRibbon(UIControlledApplication application)
        {
            const string tabName = "Starline Windows";
            try { application.CreateRibbonTab(tabName); }
            catch
            {
                // ignored
            }

            var ribbonPanel = application.CreateRibbonPanel(tabName, "AutoDimension");
            AddCommand(ribbonPanel, "Dimension", "Dim", typeof(DimensioningCommand));
            AddCommand(ribbonPanel, "Tag", "Tag", typeof(TagCommand));
            AddCommand(ribbonPanel, "Settings", "Settings", typeof(SettingsCommand));
        }

        private static void AddCommand(RibbonPanel ribbonPanel, string commandName, string imageName, Type commandType)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var thisAssemblyPath = assembly.Location;

            var settingsButtonData = new PushButtonData($"cmd{commandName}", commandName, thisAssemblyPath,
                commandType.UnderlyingSystemType.FullName);

            if (ribbonPanel.AddItem(settingsButtonData) is PushButton settingsPushButton)
            {
                settingsPushButton.ToolTip = commandName;
                settingsPushButton.LargeImage =
                    new BitmapImage(new Uri($"pack://application:,,,/{assembly.FullName};component/Images/{imageName}.png"));
                settingsPushButton.Image =
                    new BitmapImage(new Uri($"pack://application:,,,/{assembly.FullName};component/Images/{imageName}.png"));
            }
        }

        public void CreateAppFolder()
        {
            if (!File.Exists(CommonValues.CurrentOptionsPath))
            {
                Directory.CreateDirectory(CommonValues.LocalFilesFolder);
                File.Copy(CommonValues.DefaultOptionsPath, CommonValues.CurrentOptionsPath);
            }
        }
    }
}