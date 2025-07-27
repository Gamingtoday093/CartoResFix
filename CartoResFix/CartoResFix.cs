using HarmonyLib;
using SDG.Framework.Modules;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CartoResFix
{
    public class CartoResFix : IModuleNexus
    {
        private Harmony Harmony { get; set; }

        public void initialize()
        {
            Harmony = new Harmony("CartoResFix");
            Harmony.PatchAll();

            CommandWindow.Log($"CartoResFix v{Assembly.GetExecutingAssembly().GetName().Version} by Gamingtoday093 has been Loaded!");
        }

        public void shutdown()
        {
            Harmony.UnpatchAll("CartoResFix");

            CommandWindow.Log("CartoResFix has been Unloaded!");
        }
    }
}
