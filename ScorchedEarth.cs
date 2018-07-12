using BattleTech.Rendering;
using Harmony;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TranspilerTest
{
    public class ScorchedEarth
    {
        internal static string ModDirectory;
        internal static Settings settings;
        public static void Init(string directory, string settingsJSON)
        {
            var harmony = HarmonyInstance.Create("ca.gnivler.ScorchedEarth");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            ModDirectory = directory;
        }

        public class Settings
        {
            bool Debug = true;
        }
    }

    [HarmonyPatch(typeof(FootstepManager))]
    [HarmonyPatch(new[] { typeof(int), typeof(float), typeof(float), typeof(float) })]
    public static class Patch
    {

        // .maxstack 8
        // 
        // IL_0000: ldc.i4.s     125 // 0x7d
        // IL_0002: stsfld       int32 BattleTech.Rendering.FootstepManager::maxDecals
        // IL_0007: ldc.r4       20
        // IL_000c: stsfld       float32 BattleTech.Rendering.FootstepManager::footstepLife
        // IL_0011: ldc.r4       30
        // IL_0016: stsfld       float32 BattleTech.Rendering.FootstepManager::scorchLife
        // IL_001b: ldc.r4       4
        // IL_0020: stsfld       float32 BattleTech.Rendering.FootstepManager::decalFadeTime
        // IL_0025: ret          

        // yield return new CodeInstruction(OpCodes.Ldarg_1);//First argument for both our method and its own
        // yield return new CodeInstruction(OpCodes.Ldarg_S, (byte)4);//Second argument for our method, fourth argument for its own: Thing thing
        // yield return new CodeInstruction(OpCodes.Call, typeof(Patch_Shadows).GetMethod("SatisfiesNoShadow"));//Injected code
        // yield return new CodeInstruction(OpCodes.Brtrue, instruction.operand);//If true, break to exactly where the original instruction went


        // strategy:  replace the ldc codes with higher numbers.  ldc.i4.s is a byte cast to int32 the rest are floats
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < instructions.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S)
                {
                    codes.Remove(codes[i]);
                    codes.Add(new CodeInstruction(OpCodes.Ldc_I4, 1000));
                }

                if (codes[i].opcode == OpCodes.Ldc_I4)
                {
                    codes.Remove(codes[i]);
                    codes.Add(new CodeInstruction(OpCodes.Ldc_R4, float.MaxValue));
                }
            }
            return codes.AsEnumerable();
        }
    }
}