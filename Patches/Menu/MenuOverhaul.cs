using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.IO;
using Il2CppTMPro;
using System.Linq;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.Start))]
	static class TitleManager_Start_Patch
	{
		public static bool Prefix(TitleManager __instance)
		{
			PlayerPrefs.SetInt("PlayedBefore", 0);
			PlayerPrefs.Save();

			Global.SaveGameToFile();

			DeviceColorManager.SetEffect(new StaticDeviceColorEffect(__instance.keyColor));

			var data = Global.data;

			__instance.staticSource?.SetActive(true);

			__instance.sfxSource = __instance.GetComponent<AudioSource>();

			if (__instance.titleMusic != null)
				__instance.titleMusic.enabled = true;

			var prefix = "title";

			if (data.night != 1)
			{
				var night = Mathf.Min(Redux.night, 6);
				__instance.continueText.text = Language.GetString($"{prefix}.continue");
				__instance.nightNumberText.text = string.Format($"{Language.GetString($"{prefix}.night")}", night);
			}
			else
			{
				__instance.continueText.text = Language.GetString($"{prefix}.new_game");
			}

			// REMOVED SUBSCRIBE, CHECK LATER

			return false;
		}
	}

	[HarmonyPatch(typeof(TitleShortener), nameof(TitleShortener.Start))]
	static class TitleShortener_Start_Patch
	{
		public static bool Prefix(TitleShortener __instance)
		{
			Redux.ResetMenu();

			__instance.GetComponent<Animation>().Play("LaunchSequenceShortened");

			var mainTitle = __instance.transform.Find("MainTitle");

			mainTitle.Find("NewGame-Continue Button").gameObject.active = false;
			mainTitle.Find("Continue Night Number Text").gameObject.active = false;
			mainTitle.Find("Minimap Button").gameObject.active = false;
			mainTitle.Find("TAB info text").gameObject.active = false;

			mainTitle.Find("TAB info text").GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 20);

			var logoMask = mainTitle.Find("LogoMask");
			logoMask.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 100);

			var realTimeText = logoMask.Find("RealTimeText");

			var newText = GameObject.Instantiate(realTimeText.gameObject, logoMask);
			newText.name = "RealTimeTextNew";
			var newRect = newText.GetComponent<RectTransform>();
			newRect.anchoredPosition += new Vector2(0, 15);

			realTimeText.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 65);
			realTimeText.GetComponent<TextMeshProUGUI>().text = ">Redux";
			realTimeText.gameObject.active = false;

			var customNight = mainTitle.Find("Minimap Button");
			var sizeDelta = customNight.GetComponent<RectTransform>().sizeDelta + new Vector2(180, 0);
			customNight.GetComponent<RectTransform>().sizeDelta = sizeDelta;
			var offsetMin = customNight.GetComponent<RectTransform>().offsetMin + new Vector2(90, 0);
			customNight.GetComponent<RectTransform>().offsetMin = offsetMin;

			var versionNum = mainTitle.Find("VersionNum");
			versionNum.localScale = new Vector3(0.5f, 0.5f, 1);
			versionNum.GetComponent<TextMeshProUGUI>().text = $"1.2.0 (Redux {Redux.GetVersion()})";
			versionNum.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 56.74f);

			var music = GameObject.Find("Menu Music");
			music.GetComponent<AudioSource>().clip = Redux.LoadOgg($"{Redux.GetAssetsPath()}menu.ogg");
			music.GetComponent<AudioSource>().Play();

			return false;
		}
	}

	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.Update))]
	static class TitleManager_Update_Patch
	{
		private static C[] clankerArray = { C.Freddy };
		private static int i = 0;
		private static double prevTime = 0;
		private static readonly double[] timecodes = { 0.9, 7.85, 13.6, 19.6, 24.85 };

		public static void Postfix(TitleManager __instance)
		{
			Global.data.night = Redux.night;

			var video = __instance.titleVideo;

			if (Redux.menuTimer >= 0.0)
			{
				if (Redux.menuTimer < 5.75)
				{
					if (Redux.menuTimer >= 1.5)
					{
						if (!Redux.playFired)
						{
							Redux.playFired = true;
							video.Play();
						}
					}

					if (Redux.menuTimer >= 3.75)
					{
						var realTimeText = GameObject.Find("UI Canvas/MainTitle/LogoMask/RealTimeText");

						if (!Redux.enableFired)
						{
							Redux.enableFired = true;
							realTimeText.gameObject.active = true;
						}

						realTimeText.GetComponent<TextMeshProUGUI>().color =
							new Color(0.549f, 0f, 0f, (float)(Redux.menuTimer - 3.75));

						var staticSource = GameObject.Find("Static Source");
						staticSource.GetComponent<AudioSource>().volume = (float)((5.75 - Redux.menuTimer) / 2.0);
					}

					if (Redux.menuTimer >= 4.75)
					{
						var cButton = GameObject.Find("UI Canvas/MainTitle/NewGame-Continue Button");
						var cText = GameObject.Find("UI Canvas/MainTitle/Continue Night Number Text");
						var mButton = GameObject.Find("UI Canvas/MainTitle/Minimap Button");
						var mText = GameObject.Find("UI Canvas/MainTitle/TAB info text");

						Color fadeColor = new Color(1f, 1f, 1f, (float)(Redux.menuTimer - 4.75));

						cButton.GetComponent<TextMeshProUGUI>().color = fadeColor;
						cText.GetComponent<TextMeshProUGUI>().color = fadeColor;
						mButton.GetComponent<TextMeshProUGUI>().color = fadeColor;
						mText.GetComponent<TextMeshProUGUI>().color = fadeColor;

						cButton.gameObject.active = true;
						cText.gameObject.active = true;
						mButton.gameObject.active = Redux.night > 6;
						mText.gameObject.active = true;
					}


					Redux.menuTimer += Time.deltaTime;
				}
				else
				{
					Redux.menuTimer = -1.0;
				}
			}

			if (clankerArray?.Length == 1)
			{
				C[] clankerSubArray = { C.Bonnie, C.Chica, C.Foxy };
				Redux.Shuffle(clankerSubArray);
				clankerArray = clankerArray.Concat(clankerSubArray).ToArray();
			}

			if (video.time != prevTime)
			{
				prevTime = 0;

				if (video.isPlaying && video.time > timecodes[(int)clankerArray[i] + 1])
				{
					video.Pause();
					i++;
					if (i >= 4)
					{
						i = 0;
						C last = clankerArray.Last();
						do
						{
							Redux.Shuffle(clankerArray);
						} while (clankerArray.First() == last);

					}

					prevTime = video.time;
					video.time = timecodes[(int)clankerArray[i]];
					video.Play();
				}
			}
		}
	}
}
