using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.ContinuePressed))]
	static class TitleManager_ContinuePressed_Patch
	{
		public static bool Prefix(TitleManager __instance)
		{
			Redux.custom = false;

			Redux.LoadNight(__instance);

			return false;
		}
	}

	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.MinimapPressed))]
	static class TitleManager_MinimapPressed_Patch
	{
		public static bool Prefix(TitleManager __instance)
		{
			Redux.night = 7;

			Redux.custom = true;

			Redux.LoadNight(__instance);

			return false;
		}
	}
}
