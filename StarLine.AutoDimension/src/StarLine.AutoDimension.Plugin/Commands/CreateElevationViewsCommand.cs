using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using StarLine.AutoDimension.Core.Services;
using StarLine.AutoDimension.Plugin.Utils;
using System.Windows;
using StarLine.AutoDimension.Plugin.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;



namespace StarLine.AutoDimension.Plugin.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateElevationViewsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //get uidocument 
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get document 
            Document doc = uidoc.Document;
            IList<Wall> selected_curtainwalls       = new List<Wall>();
            IList<Wall> selected_curtainwalls_valid = new List<Wall>();

            try
            {

                IList<Element> curtain_walls        = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();

                List<string> wall_name              = new List<string>();
                List<string> window_tag             = new List<string>();
                List<string> curtain_wall_level     = new List<string>();
                HashSet<string> Combined_name       = new HashSet<string>();

                //================================================================================================================================
                //collecting and filtering all the walls based on the WindowGroupID
                foreach (Wall wall in curtain_walls)
                {
                    Parameter curtainwall_windowtag         = wall.LookupParameter("WindowGroupID");
                    string tag_name = curtainwall_windowtag != null ? curtainwall_windowtag.AsString() : "No Tag";


                    string wall_level_name = wall.LookupParameter("Base Constraint").AsValueString();
                    string level_name = wall_level_name != null ? wall_level_name : "No Leve;";

                    string cn = "Window Group ID: "+tag_name + ", Level: " + level_name;
                    Combined_name.Add(cn);
                }

                List<string> commbined_names = Combined_name.ToList();

                foreach (Wall wall in curtain_walls)
                {
                    wall_name.Add(wall.Name);

                }

                //================================================================================================================================
                // UI and data collected
                CreateElevationUI ui_window = new CreateElevationUI(commbined_names);
                ui_window.ShowDialog();

                List<string> selected_curtain_walls     = ui_window.selected_wall_name;
                double elevation_offset_from_wall       = ui_window.elevation_offsetfromwall;
                double elevation_view_depth             = ui_window.elevation_viewdepth;
                double elevation_width_offset           = ui_window.elevation_widthoffset;
                double elevation_height_offset          = ui_window.elevation_heightoffset;
                //================================================================================================================================
                //selected walls
                string msg3 = "";
                //string 
                foreach(string windows_groupid in selected_curtain_walls)
                {
                    string[] parts = windows_groupid.Split(',');
                    string windowgroupid = parts[0].Split(':')[1].Trim();
                    
                    foreach (Wall wall in curtain_walls)
                    {
                        string wall_group_id = wall.LookupParameter("WindowGroupID").AsString();
                        //string curtainwall_windowtag = curtainwall.LookupParameter("WindowTag").AsString();

                        if (wall_group_id == windowgroupid)
                        {
                            msg3 += windowgroupid + "\n";
                            selected_curtainwalls.Add(wall);
                        }
                    }
                }

                ICollection<Element> Elevation_views = new FilteredElementCollector(doc).OfClass(typeof(ViewSection)).WhereElementIsNotElementType().ToElements();
                List<ViewSection> AllElevation_views = new List<ViewSection>();
                /*
                string msg10 = "";
                foreach(Element elem in Elevation_views)
                {
                    foreach (Wall wall in selected_curtainwalls)
                    {
                        if (elem is ViewSection viewsection)
                        {
                            AllElevation_views.Add(viewsection);
                            string wall_windowtag = wall.LookupParameter("WindowTag").AsString();

                            if (viewsection.Name != wall_windowtag)
                            {
                                selected_curtainwalls_valid.Add(wall);
                                msg10 += wall.Name + "\n";
                            }

                            //msg10 += viewsection.Name + "\n" ;
                            

                        }
                    }
                }

                TaskDialog.Show("Elevation View List", msg10);
                */
                //===================================================================================================================================================================
                //getting elevation view family type

                IList<Element> elevation_family_types   = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).WhereElementIsElementType().ToElements();
                Element elevation_family_type           = null;
                string elevation_type_name              = "06 ENLARGED ELEVATION";

                foreach (Element elem in elevation_family_types)
                {
                    string elevation_name = elem.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                    if (elevation_name  == elevation_type_name)
                    {
                        elevation_family_type = elem;
                        break;
                    }
                }
                if (elevation_family_type == null)
                {
                    string msg = "Elevation Family Type Not Found, \nFamily Type = " + elevation_type_name;
                    TaskDialog.Show("Create Elevation View", msg);
                }

                //===================================================================================================================================================================
                //collecting elevation view template
                ICollection<Element> views = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View)).ToElements();
                List<View> templates =new List<View>();

                foreach(Element element in views)
                {
                    View view = element as View;
                    if ( view!= null && view.IsTemplate && view.ViewType == ViewType.Elevation)
                    {
                        templates.Add(view);
                    }
                }
                View template = null;
                string  view_template_name = "06 ENLARGED ELEVATIONS";
                foreach (View temp in templates)
                {
                    if(temp.Name == view_template_name)
                    {
                        template = temp;
                        break;
                    }
                }
                if (template == null)
                {
                    string msg = "Elevation View Template Not Found, \nView Template = " + view_template_name;
                    TaskDialog.Show("Create Elevation View", msg);
                }






                //===================================================================================================================================================================
                //creating elevation views
                string msgs = "";
                string msg5 = "";
                string Elevation_viewname_exist = "";

                View current_view = null;
                string curtainwall_level_name = null;

                foreach (Wall curtainwall in selected_curtainwalls)
                {

                    //=======================================================================================================================
                    // CURTAIN WALL GEOMETRY DETAILS
                    LocationCurve curtaiwall_location   = curtainwall.Location as LocationCurve;
                    //XYZ curtainwall_center_point        = curtaiwall_location.Curve.Evaluate(0.5, true);
                    XYZ curtainwall_start_point         = curtaiwall_location.Curve.GetEndPoint(0);
                    XYZ curtainwall_start_end           = curtaiwall_location.Curve.GetEndPoint(1);

                    XYZ curtainwall_center_point        = (curtainwall_start_point + curtainwall_start_end)/2;
                    //=======================================================================================================================
                    // CURTAIN WALL orinetation
                    XYZ interior_orineation = curtainwall.Orientation;
                    XYZ wall_direction      = interior_orineation * -1;

                    double wall_perpendicular_angle         = Math.Atan2(wall_direction.Y, wall_direction.X) * -1;
                    double wall_perpendicular_angle_degree  = wall_perpendicular_angle * (180/Math.PI);


                    //=======================================================================================================================
                    // CURTAIN WALL PROPERTIES 
                    double curtainWallHeight    = curtainwall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                    double curtainWallLength    = curtainwall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                    double distance             = UnitUtils.ConvertToInternalUnits(elevation_offset_from_wall, UnitTypeId.Inches);
                    double width                = curtainWallLength;
                    double height               = curtainWallHeight;
                    double widthOffset          = UnitUtils.ConvertToInternalUnits(elevation_width_offset, UnitTypeId.Inches);
                    double heightOffset         = UnitUtils.ConvertToInternalUnits(elevation_height_offset, UnitTypeId.Inches);
                    double depth                = UnitUtils.ConvertToInternalUnits(elevation_view_depth, UnitTypeId.Inches);


                    //=======================================================================================================================
                    // New centre point 
                    XYZ new_center_point = new XYZ(
                            curtainwall_center_point.X + wall_direction.X * distance,
                            curtainwall_center_point.Y + wall_direction.Y * distance,
                            curtainwall_center_point.Z + wall_direction.Z * distance
                            );



                    msgs = curtainwall_center_point.ToString() + "\n" + distance.ToString() + 
                        "\n" + width.ToString() + "\n" + height.ToString() + "\n" + widthOffset.ToString() +
                        "\n" + heightOffset.ToString() + "\n" + wall_perpendicular_angle.ToString() + "\n" + new_center_point.ToString();

                    //=======================================================================================================================
                    try
                    {
                        curtainwall_level_name = curtainwall.LookupParameter("Base Constraint").AsValueString();
                        
                        foreach(View view in views)
                        {
                            if (view.Name.ToLower() == curtainwall_level_name.ToLower())
                            {
                                current_view = view;
                            }
                        }

                        // start transaction
                        Transaction transaction_w           = new Transaction(doc, "Create Viewsf For Windows");
                        transaction_w.Start();
                    
                        // Create Elevation Marker
                        ElevationMarker elevation_marker    = ElevationMarker.CreateElevationMarker(doc, elevation_family_type.Id, new_center_point, 1);
                    
                        // Create Elevation using elevation marker
                        ViewSection elevation_view          = elevation_marker.CreateElevation(doc, current_view.Id, 0);

                        //getting bouding box and setting min and max point 
                        BoundingBoxXYZ bbox = elevation_view.get_BoundingBox(null);
                        bbox.Min            = new XYZ(new_center_point.Y - width/2 - widthOffset, new_center_point.Z - heightOffset, 0);
                        bbox.Max            = new XYZ(new_center_point.Y + width/2 + widthOffset, new_center_point.Z + height + heightOffset, 0);

                        elevation_view.CropBoxActive    = true;
                        elevation_view.CropBoxVisible   = true;
                        elevation_view.CropBox          = bbox;


                        //rotate the elavrion view 
                        XYZ rotation_point1     = new_center_point;
                        XYZ rotation_point2     = new XYZ(new_center_point.X, new_center_point.Y, new_center_point.Z - 3);
                        Line rotation_axis      = Line.CreateBound(rotation_point1, rotation_point2);

                        ElementTransformUtils.RotateElement(doc, elevation_marker.Id, rotation_axis, wall_perpendicular_angle);

                        //set farclip offset 
                        Parameter farclipoffset         = elevation_view.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
                        farclipoffset.Set(depth);

                        //set view template 
                        elevation_view.ViewTemplateId   = template.Id;
                        //set view name

                        //setting view name
                        string curtainwall_windowtag = curtainwall.LookupParameter("WindowTag").AsString();
                        try
                        {

                            Parameter elevation_view_name   = elevation_view.LookupParameter("View Name");
                            elevation_view_name.Set(curtainwall_windowtag);

                            //setting level details
                            //string level_name               = curtainwall.LookupParameter("Base Constraint").AsValueString();
                            Parameter elevation_view_level  = elevation_view.LookupParameter("View Level");
                            elevation_view_level.Set(curtainwall_level_name); 
                        }

                        catch
                        {
                            Elevation_viewname_exist += curtainwall_windowtag + "\n";
                        }

                        transaction_w.Commit();
                    }
                    catch (Exception ex) 
                    {
                        //string msg = ex.Message;    
                    }

                    if (current_view == null)
                    {
                        msg5 = "View Not Found: " + curtainwall_level_name + "(Level)";
                        TaskDialog.Show("Create Elevation", msg5);
                    }

                    //TaskDialog.Show("Create Elevation View", msgs);

                }
                if (Elevation_viewname_exist != "")
                {
                    TaskDialog.Show("Eelevation View Name Already Exist", Elevation_viewname_exist);
                }

                return Result.Succeeded;

            }
            catch (System.Exception ex)
            {
                //MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error");
                return Result.Failed;
            }
        }
    }
}