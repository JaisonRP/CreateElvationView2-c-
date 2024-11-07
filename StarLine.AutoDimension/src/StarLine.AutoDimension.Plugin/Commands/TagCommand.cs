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
    // Get the active UI document and document from the command data
    var uiDoc = commandData.Application.ActiveUIDocument;
    var doc = commandData.Application.ActiveUIDocument.Document;


    try
    {
        // Start a new transaction
        using (var trans = new Transaction(doc))
        {
            // Start the transaction with a specific name
            trans.Start("StarLine_AutoDimension_Tag");

            // Get the active view from the document
            var activeView = doc.ActiveView;

            // Check if the active view is a 3D view or a table view
            if (activeView is View3D || activeView is TableView)
            {
                // Display an error message if tags cannot be added to this view
                MessageBox.Show("Tags cannot be added to this view.", "Error");
                return Result.Succeeded;
            }

            // Create a RevitSelector instance for selecting elements
            var selector = new RevitSelector(uiDoc);

            // Get the current selection result
            var result = selector.GetCurrentSelection();

            // Check if the selected element is valid
            if (result.Id == ElementId.InvalidElementId)
            {
                // Display a message if no valid element is selected
                MessageBox.Show("Please select a wall or panel before running the command.");
                return Result.Cancelled;
            }

            // Check if the selected element is a Wall
            if (result.Document.GetElement(result.Id) is Wall wall)
            {
                // Create a WallTagger instance for tagging walls
                var tagger = new WallTagger(doc, wall, result);

                // Read current tagging options from a serialized file
                var options = OptionSerializer.ReadCurrent();

                // Tag the selected wall with the specified options
                tagger.Tag(options);

                // Collect levels associated with the selected wall
                foreach (var id in selector.CollectLevels())
                {
                    // Create a LevelWrapper instance for handling levels
                    var level = new LevelWrapper(id, doc);

                    // Add notes to the collected levels
                    level.AddNote();
                }
            }

            // Commit the transaction
            trans.Commit();
        }

        // Return a success result
        return Result.Succeeded;
    }
    catch (System.Exception ex)
    {
        // Display an error message if an exception occurs
        MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error");
        return Result.Failed;
    }
}

    } //
}
