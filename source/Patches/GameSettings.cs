using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TownOfUs.CustomOption;

namespace TownOfUs
{
    [HarmonyPatch]
    public static class GameSettings
    {
        public static bool AllOptions;

        [HarmonyPatch]
        private static class GameOptionsDataPatch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(GameOptionsData).GetMethods(typeof(string), typeof(int));
            }

            private static void Postfix(ref string __result)
            {
                var builder = new StringBuilder(AllOptions ? __result : "");

                foreach (var option in CustomOption.CustomOption.AllOptions)
                {
                    if (option.Name == "Crewmate Investigative Roles")
                    {
                        builder.Append("(Scroll for all settings)");
                        builder.AppendLine("");
                        builder.Append(new StringBuilder(__result));
                    }
                    if (option.Type == CustomOptionType.Button) continue;
                    if (option.Type == CustomOptionType.Header) builder.AppendLine($"\n{option.Name}");
                    else if (option.Indent) builder.AppendLine($"     {option.Name}: {option}");
                    else builder.AppendLine($"{option.Name}: {option}");
                }

                __result = builder.ToString();
                __result = $"<size=1.25>{__result}</size>";
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        public static class Update
        {
            public static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentInParent<Scroller>().ContentYBounds.max = (__instance.Children.Length - 6.5f) / 2;
            }
        }
    }
}