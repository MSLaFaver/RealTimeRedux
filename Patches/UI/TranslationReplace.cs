using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(TranslatedText), nameof(TranslatedText.UpdateText))]
	static class TranslatedText_UpdateText_Patch
	{
		public static bool Prefix(TranslatedText __instance)
		{
			var normal = true;

			if (__instance.key == "title.minimap")
			{
				if (__instance.text != null)
				{
					__instance.text.text = __instance.name == "CustomNightReady" ? "Ready" : "Custom Night";
				}
				normal = false;
			}

			return normal;
		}
	}
}
