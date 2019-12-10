using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ProjectAutomata;
using Newtonsoft.Json.Linq;
using Harmony;

namespace ModdersYLib
{
    public class ModdersYLibMod: Mod
    {
        public override void OnAllModsLoaded()
        {
            var harmony = Harmony.HarmonyInstance.Create("ModdersYLibMod");
            harmony.Patch(typeof(Harvester).GetMethod("StopWork"), null, new HarmonyMethod(typeof(Patch).GetMethod("HarvesterStopWork")));

            ScanContent();
        }

        void ScanContent()
        {
            List<ModInstance> mods = ModLoader.instance.loadedMods;
            foreach (ModInstance instance in mods)
            {
                string contentDirectory = System.IO.Path.Combine(instance.modPath, "content");
                if (Directory.Exists(contentDirectory))
                {
                    foreach (var jsonFile in Directory.GetFiles(contentDirectory, "*.json"))
                    {
                        JObject jsonObj = JObject.Parse(File.ReadAllText(jsonFile));
                        if (jsonObj.TryGetValue("object", out JToken paObj))
                            foreach (var k in ((JObject)paObj).Properties().Where(x => x.Name.StartsWith("y")))
                                if (JCommands.Instance.Commands.TryGetValue(k.Name, out JCommandDelegate func))
                                    func(jsonObj, (JObject)paObj, k.Value);
                    } 
                }
            }            
        }
    }
}
