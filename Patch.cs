using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectAutomata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace ModdersYLib
{
    public static class Patch
    {
        public class HarvesterProductHook
        {
            public string HookedProductName;
            public ProjectAutomata.ProductDefinition ExtraProduct;
            public int Frequency;
            public int Amount;
            public string WaitForUnlock = String.Empty;
            public int Counter = 0;

            TechTreeUnlock TechTreeUnlock;

            public HarvesterProductHook(JToken jtoken)
            {
                JsonSerializer.CreateDefault().Populate(jtoken.CreateReader(), this);
                TechTreeUnlock = GameData.instance.GetAsset<TechTreeUnlock>(WaitForUnlock);
            }

            public bool IsTechTreeUnlocked()
            {
                if (TechTreeUnlock == null) return true;
                //else if (Player.localPlayer.techTreeAgent.IsUnlocked(TechTreeUnlock)) return true;
                else if (Player.localPlayer.techTree.IsUnlocked(TechTreeUnlock)) return true;
                else return false;
            }
        }

        public static List<HarvesterProductHook> HarvesterProductHookList = new List<HarvesterProductHook>();


        public static void HarvesterStopWork(ProjectAutomata.Harvester __instance)
        {
            foreach(var hph in HarvesterProductHookList)
                if(hph.HookedProductName == __instance.hub.currentOutput.definition.name)
                    if(hph.IsTechTreeUnlocked())
                        if(++hph.Counter >= hph.Frequency)
                        {
                            UnityEngine.Debug.Log($"Hooked harvester product: Delivering {hph.Amount} {hph.ExtraProduct.name} now.");
                            if (__instance.storage != null)
                            {
                                hph.Counter = 0;
                                __instance.hub.storage.Put(hph.ExtraProduct, hph.Amount, null);
                            }
                            else UnityEngine.Debug.Log("Storage is null!");
                        }
        }
    }
}
