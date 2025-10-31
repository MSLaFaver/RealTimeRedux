using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;

namespace Redux
{
	[HarmonyPatch(typeof(SetTransitionNightText), nameof(SetTransitionNightText.Awake))]
	static class SetTransitionNightText_Awake_Patch
	{
		public static bool Prefix(SetTransitionNightText __instance)
		{
			Redux.ResetMain();

			var sub = Redux.custom ? 0 : 1;
			var nightName = Language.GetString($"show_night.night_{Redux.night - sub}");
			__instance.GetComponent<TextMeshProUGUI>().text = nightName;
			return false;
		}
	}

	[HarmonyPatch(typeof(SetNightText), nameof(SetNightText.Start))]
	static class SetNightText_Start_Patch
	{
		public static bool Prefix(SetNightText __instance)
		{
			var sub = Redux.custom ? 0 : 1;

			__instance.m_TextMeshPro = __instance.GetComponent<TextMeshProUGUI>();
			var trans = Language.GetString("game.night_display");
			__instance.m_TextMeshPro.text = string.Format(trans, Redux.night - sub);
			return false;
		}
	}

	[HarmonyPatch(typeof(SetNightText), nameof(SetNightText.UpdateLocalization))]
	static class SetNightText_UpdateLocalization_Patch
	{
		public static bool Prefix(SetNightText __instance)
		{
			var sub = Redux.custom ? 0 : 1;

			var trans = Language.GetString("game.night_display");
			__instance.m_TextMeshPro.text = string.Format(trans, Redux.night - sub);
			return false;
		}
	}
}
