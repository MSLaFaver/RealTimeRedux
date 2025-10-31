using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(SetPowerPercent), nameof(SetPowerPercent.Update))]
	static class SetPowerPercent_Update_Patch
	{
		public static void Postfix(SetPowerPercent __instance)
		{
			string displayText = Language.GetString("game.power_display");
			int powerDisplay = Mathf.RoundToInt(NightManager.Instance.power / 10f);
			__instance._textMeshPro.text = string.Format(displayText, powerDisplay);
		}
	}
}
