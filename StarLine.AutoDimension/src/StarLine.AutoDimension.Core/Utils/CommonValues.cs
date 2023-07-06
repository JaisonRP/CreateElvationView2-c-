using System;
using System.IO;
using System.Reflection;

namespace StarLine.AutoDimension.Core.Utils
{
    public static class CommonValues
    {
        public static string LocalFilesFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StarLine", "AutoDimension");

        public static string DefaultOptionsPath
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var thisAssemblyPath = assembly.Location;
                var folder = Path.GetDirectoryName(thisAssemblyPath);
                return Path.Combine(folder, "default.json");
            }
        }

        public static string CurrentOptionsPath => Path.Combine(LocalFilesFolder, "current.json");
    }
}
