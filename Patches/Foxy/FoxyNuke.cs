using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(ChooseJumpscare), nameof(ChooseJumpscare.Start))]
	static class ChooseJumpscare_Start_Patch
	{
		public static void Postfix(ChooseJumpscare __instance)
		{
			if (__instance.transform.name == "Jumpscare_Foxy")
			{
				__instance.ForceClipIndex(0);
			}
		}
	}
}
