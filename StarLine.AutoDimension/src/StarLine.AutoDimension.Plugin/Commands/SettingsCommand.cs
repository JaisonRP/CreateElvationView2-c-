using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Ioc;
using StarLine.AutoDimension.Core.Services;
using StarLine.AutoDimension.Core.Views;
using StarLine.AutoDimension.Plugin.Utils;
using System.Diagnostics;
using System.Windows.Interop;

namespace StarLine.AutoDimension.Plugin.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            using (var trans = new Transaction(doc))
            {
                trans.Start("StarLine_AutoDimension_Settings");
                var win = Init.Container.Resolve<OptionsWindow>();
                var _ = new WindowInteropHelper(win)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                var options = OptionSerializer.ReadCurrent();
                var vm = win.GetViewModel();

                var revitUtil = new RevitUtils(doc);
                var lengthSymbol = revitUtil.GetLengthUnit();
                var factor = revitUtil.GetLengthConversion();
                vm.SetLengthSymbol(lengthSymbol, factor);

                var collector = new RevitFilter(doc);
                var panelSymbols = collector.GetPanelTagSymbols();
                vm.SetPanelTags(panelSymbols);

                var doorSymbols = collector.GetDoorTagSymbols();
                vm.SetDoorTags(doorSymbols);

                var materialSymbols = collector.GetMaterialTagSymbols();
                vm.SetMaterialTags(materialSymbols);

                var genericSymbols = collector.GetGenericModelTagSymbols();
                vm.SetGenericTags(genericSymbols);

                var annotationSymbols = collector.GetAnnotationSymbols();
                vm.SetAnnotationSymbols(annotationSymbols);

                var dimensionStyles = collector.GetDimensionStyles(DimensionStyleType.Linear);
                vm.SetDimensionStyles(dimensionStyles);

                vm.ReadFromOptions(options);
                win.Show();
                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}