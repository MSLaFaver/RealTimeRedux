using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(SettingsMenuManager), nameof(SettingsMenuManager.Start))]
	static class SettingsMenuManager_Start_Patch
	{
		public static bool Prefix(Animatronic __instance)
		{
			__instance.transform.Find("TopButtons/GJBtn").gameObject.active = false;
			__instance.transform.Find("AccessibilityMenu/ContentCreatorMode").gameObject.active = false;

			return true;
		}
	}
}
