using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using StarLine.AutoDimension.Core.Services;
using StarLine.AutoDimension.Plugin.Domain;
using StarLine.AutoDimension.Plugin.Utils;

namespace StarLine.AutoDimension.Plugin.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TagCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            try
            {
                using (var trans = new Transaction(doc))
                {
                    trans.Start("StarLine_AutoDimension_Tag");
                    var activeView = doc.ActiveView;
                    if (activeView is View3D || activeView is TableView)
                    {
                        MessageBox.Show("Tags cannot be added to this view.", "Error");
                        return Result.Succeeded;
                    }

                    var selector = new RevitSelector(uiDoc);
                    var result = selector.GetCurrentSelection();
                    if (result.Id == ElementId.InvalidElementId)
                    {
                        MessageBox.Show("Please select a wall or panel before running the command.");
                        return Result.Cancelled;
                    }

                    if (result.Document.GetElement(result.Id) is Wall wall)
                    {
                        var tagger = new WallTagger(doc, wall, result);
                        var options = OptionSerializer.ReadCurrent();
                        tagger.Tag(options);
                    }

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error");
                return Result.Failed;
            }
        }
    }
}
