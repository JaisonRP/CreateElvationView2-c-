using Autodesk.Revit.UI;
using Prism.Ioc;
using System.Text;
using System;

namespace StarLine.AutoDimension.Plugin
{
    public class Init : IExternalApplication
    {
        private static App _bs;

        public static IContainerProvider Container => _bs.Container;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                _bs = new App();
                _bs.Run();
                _bs.InitializeRibbon(application);
                _bs.CreateAppFolder();
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append($"{ex.Message}\r\n{ex.StackTrace}\r\n");
                if (ex.InnerException != null)
                {
                    sb.Append($"{ex.InnerException.Message}\r\n{ex.InnerException.StackTrace}\r\n");
                }

                var assembly = GetType().Assembly;
                var assemblyName = assembly.GetName();
                var version = assemblyName.Version.ToString();

                var mainDialog = new TaskDialog($"Star Line AutoDimension {version}")
                {
                    MainInstruction = "Error Initializing Star Line AutoDimension",
                    MainContent = sb.ToString(),
                };

                mainDialog.Show();
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}