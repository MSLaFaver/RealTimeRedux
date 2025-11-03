using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(Animatronic), nameof(Animatronic.StateUpdate))]
	static class Animatronic_StateUpdate_Patch
	{
		public static void Postfix(Animatronic __instance)
		{
			var foxyName = "Foxy State Machine";
			if (__instance.transform.parent.name == foxyName && CamManager.instance.isCamUp)
			{
				__instance.stallTimer = 3f;
			}
		}
	}
}
