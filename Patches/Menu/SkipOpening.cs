using HarmonyLib;
using Il2Cpp;
using System;

namespace Redux
{
	[HarmonyPatch(typeof(SceneTransitoner), nameof(SceneTransitoner.LoadScene), new Type[] { typeof(string) })]
	static class SceneTransitioner_LoadScene_Patch
	{
		public static bool Prefix(object __instance, string scene)
		{
			var normal = scene != "OpeningCutscene";

			if (!normal)
			{
				Global.SaveGameToFile();
				SceneTransitoner.LoadScene("Title");
			}

			return normal;
		}
	}
}
