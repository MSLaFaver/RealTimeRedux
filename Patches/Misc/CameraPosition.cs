using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(CamAnimationSelector), nameof(CamAnimationSelector.Start))]
	static class CamAnimationSelector_Start_Patch
	{
		public static void Postfix(CamAnimationSelector __instance)
		{
			__instance.animator.enabled = false;
		}
	}

	[HarmonyPatch(typeof(CamAnimationSelector), nameof(CamAnimationSelector.Update))]
	static class CamAnimationSelector_Update_Patch
	{
		public static void Postfix(CamAnimationSelector __instance)
		{
			if (__instance.camManager.isCamUp)
			{
				var pos = Redux.GetCamPosition();
				__instance.animator.transform.position = new Vector3((float)pos, 0f, -10f);
				Redux.DoCamTimer();

			}
		}
	}
}
