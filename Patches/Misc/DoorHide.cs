using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(OfficeButtons), nameof(OfficeButtons.Update))]
	static class OfficeButtons_Update_Patch
	{
		public static void Postfix(OfficeButtons __instance)
		{
			var show = __instance.doorJammed || __instance.isLightOn;
			var scale = show ? new Vector3(1, 0.785f, 1) : Vector3.zero;

			switch (__instance.name)
			{
				case "EastButton":
					GameObject.Find("Office/Animatronics In Doors/Bonnie/Doors_Bonnie In East").transform.localScale = scale;
					GameObject.Find("Office/Animatronics In Doors/Chica/Doors_Chica In East").transform.localScale = scale;
					break;
				case "WestButton":
					GameObject.Find("Office/Animatronics In Doors/Bonnie/Doors_Bonnie In West").transform.localScale = scale;
					GameObject.Find("Office/Animatronics In Doors/Chica/Doors_Chica In West").transform.localScale = scale;
					break;
			}
		}
	}
}
