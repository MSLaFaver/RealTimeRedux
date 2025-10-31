using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(AnimatronicStateMachine), nameof(AnimatronicStateMachine.ChangeState))]
	static class AnimatronicStateMachine_ChangeState_Patch
	{
		public static bool Prefix(AnimatronicStateMachine __instance, string stateName)
		{
			if (__instance.name == "Foxy State Machine" && stateName == "Basic AI State")
			{
				Redux.foxyBangs++;
				var nightManager = GameObject.Find("Night1Manager(Clone)").GetComponent<Night1>();
				nightManager.power -= Mathf.Min(nightManager.power, Redux.foxyBangs * 50 - 40);
			}
			return true;
		}
	}
}
