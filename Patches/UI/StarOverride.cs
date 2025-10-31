using HarmonyLib;
using Il2Cpp;
using UnityEngine.UI;

namespace Redux
{
	[HarmonyPatch(typeof(MenuStar), nameof(MenuStar.Update))]
	static class MenuStar_Update_Patch
	{
		public static bool Prefix(MenuStar __instance)
		{
			var greaterThan = __instance.unlockRule.nightGreaterThan;

			var enable = (greaterThan != 0 && (Redux.night > greaterThan)) ||
				(Redux.beatMaxMode && __instance.unlockRule.watchedFinalCutscene);

			__instance.GetComponent<Image>().enabled = enable;

			return false;
		}
	}
}
