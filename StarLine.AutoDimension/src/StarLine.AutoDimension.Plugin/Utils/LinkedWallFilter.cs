using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class LinkedWallFilter : ISelectionFilter
    {
        private RevitLinkInstance _mCurrentInstance;

        public bool AllowElement(Element e)
        {
            // Accept any link instance, and save the handle for use in AllowReference()
            _mCurrentInstance = e as RevitLinkInstance;
            return (_mCurrentInstance != null);
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            if (_mCurrentInstance == null)
            {
                return false;
            }

            // Get the handle to the element in the link
            var linkedDoc = _mCurrentInstance.GetLinkDocument();
            var e = linkedDoc.GetElement(refer.LinkedElementId);

            // Accept the selection if the element exists and is of the correct type
            return e.Category != null && e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Walls);
        }
    }
}