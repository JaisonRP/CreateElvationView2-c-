using Newtonsoft.Json;
using StarLine.AutoDimension.Core.Domain;
using System.IO;
using StarLine.AutoDimension.Core.Utils;

namespace StarLine.AutoDimension.Core.Services
{
    public static class OptionSerializer
    {
        public static void Serialize(Options options, string path)
        {
            var json = JsonConvert.SerializeObject(options, Formatting.Indented);
            File.WriteAllText(path, json);
        }  

        public static Options Deserialize(string path)
        {
            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                var options = (Options)serializer.Deserialize(file, typeof(Options));
                return options;
            }
        }    

        public static void WriteCurrent(Options options)
        {
            Serialize(options, CommonValues.CurrentOptionsPath);
        }

        public static Options ReadCurrent()
        {
            return Deserialize(CommonValues.CurrentOptionsPath);
        }

        public static Options ResetToDefault()
        {
            File.Copy(CommonValues.DefaultOptionsPath, CommonValues.CurrentOptionsPath, true);
            return ReadCurrent();
        }
    }
}
