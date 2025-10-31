using HarmonyLib;
using Il2Cpp;
using System;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(Animator), nameof(Animator.Play), new Type[] { typeof(string) })]
	static class Animator_Play_Patch
	{
		private static bool skippedWarning = false;

		public static bool Prefix(Animator __instance, string stateName)
		{
			var normal = true;

			if (__instance.transform.name == "IntroCanvas")
			{
				if (stateName == "FadeOut")
				{
					if (!skippedWarning)
					{
						Time.timeScale = 20f;
						normal = false;
					}
					else if (__instance.transform.Find("Subtitle Prompt").gameObject.active)
					{
						__instance.transform.Find("FlashingLights").gameObject.SetActive(false);
					}
				}
				else if (stateName == "FadeIn")
				{
					normal = skippedWarning;
					Time.timeScale = 1f;
					skippedWarning = true;
				}
			}

			return normal;
		}
	}

	[HarmonyPatch(typeof(WarningScreen), nameof(WarningScreen.Start))]
	static class WarningScreen_Start_Patch
	{
		public static bool Prefix(WarningScreen __instance)
		{
			__instance.gameJoltLogo.SetDirectAudioVolume(0, 0.1f);
			__instance.BGMusic.enabled = false;
			return true;
		}
	}

	[HarmonyPatch(typeof(WarningScreen), nameof(WarningScreen.SelectOption))]
	static class WarningScreen_SelectOption_Patch
	{
		public static bool Prefix(WarningScreen __instance, bool Yes)
		{
			if (__instance.canSelect)
			{
				__instance.canSelect = false;

				int opt = 3;

				switch (__instance.option)
				{
					case 0:
						PlayerPrefs.SetInt("FlashingLights", Yes ? 1 : 0);
						PlayerPrefs.SetInt("StreamerMode", 0);
						opt = 2;
						__instance.option = 2;
						break;
					case 2:
						PlayerPrefs.SetInt("Subtitles", Yes ? 1 : 0);
						Global.subtitles = Yes;
						opt = 3;
						break;
				}

				__instance.StartCoroutine("RegularOptionEnum", opt);
			}

			return false;
		}
	}
}
