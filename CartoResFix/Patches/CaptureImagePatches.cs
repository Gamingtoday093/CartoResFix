// #define USE_RESOLUTION

using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CartoResFix.Patches
{
    [HarmonyPatch(typeof(Level))]
    public class CaptureImagePatches
    {
        private static readonly MethodInfo BoundsGetSizeMethod = typeof(Bounds).GetProperty(nameof(Bounds.size)).GetMethod;

        private static readonly MethodInfo LevelGetSizeMethod = typeof(Level).GetProperty(nameof(Level.size)).GetMethod;

        private const int RESOLUTION = 2048;

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Level.CaptureSatelliteImage))]
        private static IEnumerable<CodeInstruction> CaptureSatelliteImageTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            if (BoundsGetSizeMethod is null)
                throw new Exception("Failed to Get MethodInfo for BoundsGetSizeMethod");

            if (LevelGetSizeMethod is null)
                throw new Exception("Failed to Get MethodInfo for LevelGetSizeMethod");

            Label coolSatElseLabel = generator.DefineLabel();
            Label coolSatEndLabel = generator.DefineLabel();

            return new CodeMatcher(instructions).MatchStartForward(
                CodeMatch.Calls(BoundsGetSizeMethod)
                )
                .Advance(2)
                .RemoveInstructions(8)
                .Insert(
                    // Vector3.x
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.x)),

                    // Vector3.z
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.z)),

                    // x < z
                    new CodeInstruction(OpCodes.Clt),

                    // if false -> coolElseLabel
                    new CodeInstruction(OpCodes.Brfalse_S, coolSatElseLabel),

                    // Level.size -> float
#if USE_RESOLUTION
                    new CodeInstruction(OpCodes.Ldc_I4, RESOLUTION),
#else
                    new CodeInstruction(OpCodes.Call, LevelGetSizeMethod),
#endif
                    new CodeInstruction(OpCodes.Conv_R4),

                    // Vector3.x
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.x)),

                    // Vector3.z
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.z)),

                    // x / z
                    new CodeInstruction(OpCodes.Div),

                    // Level.size * (x / z)
                    new CodeInstruction(OpCodes.Mul),

                    // -> int
                    new CodeInstruction(OpCodes.Conv_I4),
                    CodeInstruction.StoreLocal(1),

#if USE_RESOLUTION
                    new CodeInstruction(OpCodes.Ldc_I4, RESOLUTION),
#else
                    new CodeInstruction(OpCodes.Call, LevelGetSizeMethod),
#endif
                    CodeInstruction.StoreLocal(2),

                    // jump to End
                    new CodeInstruction(OpCodes.Br_S, coolSatEndLabel),

                    // Else Label
                    // Level.size -> float
#if USE_RESOLUTION
                    new CodeInstruction(OpCodes.Ldc_I4, RESOLUTION)
#else
                    new CodeInstruction(OpCodes.Call, LevelGetSizeMethod)
#endif
                    {
                        labels = new List<Label>() { coolSatElseLabel }
                    },
                    new CodeInstruction(OpCodes.Conv_R4),

                    // Vector3.z
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.z)),

                    // Vector3.x
                    CodeInstruction.LoadLocal(17),
                    CodeInstruction.LoadField(typeof(Vector3), nameof(Vector3.x)),

                    // z / x
                    new CodeInstruction(OpCodes.Div),

                    // Level.size * (z / x)
                    new CodeInstruction(OpCodes.Mul),

                    // -> int
                    new CodeInstruction(OpCodes.Conv_I4),
                    CodeInstruction.StoreLocal(2),

#if USE_RESOLUTION
                    new CodeInstruction(OpCodes.Ldc_I4, RESOLUTION),
#else
                    new CodeInstruction(OpCodes.Call, LevelGetSizeMethod),
#endif
                    CodeInstruction.StoreLocal(1),

                    // End Label
                    new CodeInstruction(OpCodes.Nop)
                    {
                        labels = new List<Label>() { coolSatEndLabel }
                    }

                ).InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Level.CaptureChartImage))]
        private static IEnumerable<CodeInstruction> CaptureChartImageTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // I tried to get this to work as well but the IL shows some weird
            // stfld     int32 SDG.Unturned.Level/'<>c__DisplayClass153_0'::imageWidth
            // and idk how to get that type/fieldinfo
            return instructions;
        }
    }
}
