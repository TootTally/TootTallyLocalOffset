using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTallyLocalOffset
{
    public static class FileHelper
    {
        private static readonly string FILE_PATH = Path.Combine(Paths.BepInExRootPath, "config",  "TootTallyLocalOffsets.json");

        public static Dictionary<string, int> LoadOffetFile()
        {
            Dictionary<string, int> offsetDict;
            if (!File.Exists(FILE_PATH))
            {
                offsetDict = new Dictionary<string, int>();
                SaveOffetFile(offsetDict);
                Plugin.LogInfo($"Couldn't find offset file, creating new one!");
                return offsetDict;
            }
            try
            {
                offsetDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(FILE_PATH));
            } catch (Exception ex)
            {
                Plugin.LogError($"Couldn't parse local offset file: {ex.Message}");
                File.Move(FILE_PATH, FILE_PATH + ".old" );
                offsetDict = new Dictionary<string, int>();
            }

            return offsetDict;
                
        }

        public static void SaveOffetFile(Dictionary<string, int> dict)
        {
            File.WriteAllText(FILE_PATH, JsonConvert.SerializeObject(dict));
        }
    }
}
