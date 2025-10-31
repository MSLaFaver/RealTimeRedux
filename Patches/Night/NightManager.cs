using HarmonyLib;
using Il2Cpp;
using System.Linq;

namespace Redux
{
	[HarmonyPatch(typeof(Night1), nameof(Night1.Start))]
	static class Night1_Start_Patch
	{
		public static bool Prefix(NightManager __instance)
		{
			var levels = Redux.AI();

			__instance.freddyAI.aiLevel = levels[(int)C.Freddy];
			__instance.bonnieAI.aiLevel = levels[(int)C.Bonnie];
			__instance.chicaAI.aiLevel = levels[(int)C.Chica];
			__instance.foxyAI.aiLevel = levels[(int)C.Foxy];

			if (Redux.night <= 5)
			{
				var events = EventsManager.Instance;
				events?.Night2PhoneCall();
			}

			__instance.StartCoroutine("RemoveUsageFromPower");
			__instance.StartCoroutine("IncrementHour");

			return false;
		}
	}

	[HarmonyPatch(typeof(Night1), nameof(Night1.Exit))]
	static class Night1_Exit_Patch
	{
		public static bool Prefix(NightManager __instance)
		{
			if (Redux.night <= 5)
			{
				SceneTransitoner.LoadScene("ShowNight");
			}
			else
			{
				SceneTransitoner.LoadScene("Title");
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(NightManager), nameof(NightManager.IncrementHour))]
	static class NightManager_IncrementHour_Patch
	{
		public static void Postfix(NightManager __instance)
		{
			__instance.CheckFor6AM();

			if (new[] { 2, 3, 4 }.Contains(__instance.hour))
			{
				__instance.bonnieAI.aiLevel++;

				if (__instance.hour != 2)
				{
					__instance.chicaAI.aiLevel++;
					__instance.foxyAI.aiLevel++;
				}
			}
		}
	}
}
