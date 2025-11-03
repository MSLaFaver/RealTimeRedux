using HarmonyLib;
using Il2Cpp;
using System.Linq;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(NightEndTransition), nameof(NightEndTransition.Start))]
	static class NightEndTransition_Start_Patch
	{
		public static bool Prefix(NightEndTransition __instance)
		{
			if (Redux.night < 7)
			{
				Redux.night++;
			}
			else if (Redux.custom && Redux.customLevels.Sum() >= 80 && Redux.enabled.All(b => b))
			{
				Redux.beatMaxMode = true;
			}
			Global.SaveGameToFile();

			__instance.cashText.color = new Color(0, 0, 0, 0);
			return true;
		}
	}
}
