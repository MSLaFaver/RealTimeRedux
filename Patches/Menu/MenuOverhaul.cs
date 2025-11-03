using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem.IO;
using Il2CppTMPro;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

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
			{
				__instance.titleMusic.enabled = true;
			}

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

			TitleManager_Update_Patch.Reset();

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

			var nightStart = mainTitle.Find("NewGame-Continue Button");
			nightStart.GetComponent<Button>().onClick.RemoveAllListeners();
			nightStart.GetComponent<Button>().onClick.AddListener((UnityAction)NightStart);
			nightStart.gameObject.active = false;

			mainTitle.Find("Continue Night Number Text").gameObject.active = false;
			mainTitle.Find("TAB info text").gameObject.active = false;

			var customNight = mainTitle.Find("Minimap Button");
			customNight.GetComponent<Button>().onClick.RemoveAllListeners();
			customNight.GetComponent<Button>().onClick.AddListener((UnityAction)CustomNight);
			var customNightColors = customNight.GetComponent<Button>().colors;
			customNightColors.disabledColor = Color.white;
			customNight.GetComponent<Button>().colors = customNightColors;
			customNight.gameObject.active = false;

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

			var sizeDelta = customNight.GetComponent<RectTransform>().sizeDelta + new Vector2(180, 0);
			customNight.GetComponent<RectTransform>().sizeDelta = sizeDelta;
			var offsetMin = customNight.GetComponent<RectTransform>().offsetMin += new Vector2(90, 0);
			customNight.GetComponent<RectTransform>().offsetMin = offsetMin;

			var versionNum = mainTitle.Find("VersionNum");
			versionNum.localScale = new Vector3(0.5f, 0.5f, 1);
			versionNum.GetComponent<TextMeshProUGUI>().text = $"1.2.0 (Redux {Redux.GetVersion()})";
			versionNum.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 56.74f);

			var music = GameObject.Find("Menu Music");
			music.GetComponent<AudioSource>().clip = Redux.LoadOgg($"{Redux.GetAssetsPath()}/Audio/menu.ogg");
			music.GetComponent<AudioSource>().Play();

			var customNightReady = GameObject.Instantiate(customNight.gameObject, mainTitle);
			customNightReady.name = "CustomNightReady";
			var customNightReadyRect = customNightReady.GetComponent<RectTransform>();
			customNightReadyRect.sizeDelta -= new Vector2(200, 0);
			customNightReadyRect.localPosition += new Vector3(750, 0, 0);
			customNightReady.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopRight;
			customNightReady.GetComponent<Button>().onClick.RemoveAllListeners();
			customNightReady.GetComponent<Button>().onClick.AddListener((UnityAction)CustomNightStart);

			var parent = GameObject.Find("UI Canvas").transform;

			var container = new GameObject("customNightCharacters");
			container.AddComponent(Il2CppType.Of<RectTransform>());
			container.AddComponent(Il2CppType.Of<CanvasRenderer>());

			var rect = container.GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.localPosition = new Vector2(220, 0);
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);

			float width = (130 * 2) + 10;
			float height = (172 * 2) + 10;
			rect.sizeDelta = new Vector2(width, height);

			float xOffset = 130 / 2f + 5;
			float yOffset = 172 / 2f + 5;

			AddCharacter(rect, "freddy", new Vector2(-xOffset - 30, yOffset + 30));
			AddCharacter(rect, "bonnie", new Vector2(xOffset + 30, yOffset + 30));
			AddCharacter(rect, "chica", new Vector2(-xOffset - 30, -yOffset - 30));
			AddCharacter(rect, "foxy", new Vector2(xOffset + 30, -yOffset - 30));

			return false;
		}

		private static void AddCharacter(RectTransform parent, string name, Vector2 anchoredPos)
		{
			var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			ImageConversion.LoadImage(tex, System.IO.File.ReadAllBytes($"{Redux.GetAssetsPath()}/Image/{name}.png"));

			var container = new GameObject($"{name}Portrait");
			container.AddComponent(Il2CppType.Of<RectTransform>());
			container.AddComponent(Il2CppType.Of<CanvasRenderer>());
			var containerRT = container.GetComponent<RectTransform>();
			containerRT.SetParent(parent, false);
			containerRT.sizeDelta = new Vector2(130, 172);
			containerRT.anchoredPosition = anchoredPos;
			
			var nameObj = new GameObject("name");
			nameObj.AddComponent(Il2CppType.Of<RectTransform>());
			nameObj.AddComponent(Il2CppType.Of<TextMeshProUGUI>());
			var nameText = nameObj.GetComponent<TextMeshProUGUI>();
			nameText.text = name[0].ToString().ToUpper() + name.Substring(1);
			nameText.fontSize = 30;
			nameText.alignment = TextAlignmentOptions.Center;
			nameText.color = new Color(1, 1, 1, 0);

			var refTextName = GameObject.Find("UI Canvas/MainTitle/NewGame-Continue Button")
				.GetComponent<TextMeshProUGUI>();
			nameText.font = refTextName.font;
			nameText.fontSharedMaterial = refTextName.fontSharedMaterial;

			var nameRT = nameObj.GetComponent<RectTransform>();
			nameRT.SetParent(containerRT, false);
			nameRT.anchorMin = nameRT.anchorMax = nameRT.pivot = new Vector2(0.5f, 0);
			nameRT.anchoredPosition = new Vector2(0, containerRT.sizeDelta.y / 2 + 90);
			nameRT.sizeDelta = new Vector2(130, 30);

			var imgObj = new GameObject("portrait");
			imgObj.AddComponent(Il2CppType.Of<RectTransform>());
			imgObj.AddComponent(Il2CppType.Of<CanvasRenderer>());
			imgObj.AddComponent(Il2CppType.Of<Image>());
			var img = imgObj.GetComponent<Image>();
			img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			img.color = new Color(1, 1, 1, 0);
			var rt = imgObj.GetComponent<RectTransform>();
			rt.SetParent(containerRT, false);
			rt.sizeDelta = new Vector2(128, 170);
			rt.anchoredPosition = Vector2.zero;

			Vector2[] outerPos = {
				new Vector2(0, 85), new Vector2(0, -85),
				new Vector2(-64, 0), new Vector2(64, 0)
			};
			Vector2[] outerSize = {
				new Vector2(128, 1), new Vector2(128, 1),
				new Vector2(1, 170), new Vector2(1, 170)
			};

			float inset = 5f;
			var hoverBorder = new GameObject("HoverBorder");
			hoverBorder.AddComponent(Il2CppType.Of<RectTransform>());
			hoverBorder.AddComponent(Il2CppType.Of<CanvasRenderer>());
			var hoverRT = hoverBorder.GetComponent<RectTransform>();
			hoverRT.SetParent(containerRT, false);
			hoverRT.sizeDelta = new Vector2(86, 173);

			Vector2[] innerPos = {
				new Vector2(0, rt.sizeDelta.y / 2 - inset / 2),
				new Vector2(0, -rt.sizeDelta.y / 2 + inset / 2),
				new Vector2(-rt.sizeDelta.x / 2 + inset / 2, 0),
				new Vector2(rt.sizeDelta.x / 2 - inset / 2, 0)
			};
			Vector2[] innerSize = {
				new Vector2(rt.sizeDelta.x, 1),
				new Vector2(rt.sizeDelta.x, 1),
				new Vector2(1, rt.sizeDelta.y - inset * 2),
				new Vector2(1, rt.sizeDelta.y - inset * 2)
			};

			for (int i = 0; i < 4; i++)
			{
				var outerLine = new GameObject("outerBorder");
				outerLine.AddComponent(Il2CppType.Of<RectTransform>());
				outerLine.AddComponent(Il2CppType.Of<CanvasRenderer>());
				outerLine.AddComponent(Il2CppType.Of<Image>());
				var outerRT = outerLine.GetComponent<RectTransform>();
				outerRT.SetParent(containerRT, false);
				outerRT.sizeDelta = outerSize[i];
				outerRT.anchoredPosition = outerPos[i];
				outerLine.GetComponent<Image>().color = new Color(1, 1, 1, 0);

				var innerLine = new GameObject("innerBorder");
				innerLine.AddComponent(Il2CppType.Of<RectTransform>());
				innerLine.AddComponent(Il2CppType.Of<CanvasRenderer>());
				innerLine.AddComponent(Il2CppType.Of<Image>());
				var innerRT = innerLine.GetComponent<RectTransform>();
				innerRT.SetParent(hoverRT, false);
				innerRT.sizeDelta = innerSize[i];
				innerRT.anchoredPosition = innerPos[i];
				innerLine.GetComponent<Image>().color = new Color(1, 1, 1, 0);
				innerLine.transform.localScale = new Vector3(i >= 2 ? inset : 1, i < 2 ? inset : 1, 1);
			}

			hoverBorder.SetActive(false);

			var numberObj = new GameObject("level");
			numberObj.AddComponent(Il2CppType.Of<RectTransform>());
			numberObj.AddComponent(Il2CppType.Of<TextMeshProUGUI>());
			var text = numberObj.GetComponent<TextMeshProUGUI>();
			text.text = Redux.customLevels[Redux.GetIdxFromName(name)].ToString();
			text.fontSize = 24;
			text.alignment = TextAlignmentOptions.BottomRight;
			text.color = new Color(1, 1, 1, 0);
			var refText = GameObject.Find("InterruptCanvas")
				.transform.Find("InterruptToggle/Interrupt/ButtonsArea/Resume/Text (TMP)")
				.GetComponent<TextMeshProUGUI>();
			text.font = refText.font;
			text.fontSharedMaterial = refText.fontSharedMaterial;
			var textRT = numberObj.GetComponent<RectTransform>();
			textRT.SetParent(containerRT, false);
			textRT.anchorMin = textRT.anchorMax = textRT.pivot = new Vector2(1, 0);
			textRT.anchoredPosition = new Vector2(-8, 5);
			textRT.sizeDelta = new Vector2(40, 30);

			AddArrow(containerRT, new Vector2(8, 8), false);
			AddArrow(containerRT, new Vector2(8, 27), true);
		}

		private static void AddArrow(RectTransform parent, Vector2 anchoredPos, bool isUp = false)
		{
			string fileName = isUp ? "up.png" : "down.png";
			byte[] arrowBytes = System.IO.File.ReadAllBytes($"{Redux.GetAssetsPath()}/Image/{fileName}");
			Texture2D arrowTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			ImageConversion.LoadImage(arrowTex, arrowBytes);

			var arrowObj = new GameObject(isUp ? "arrowUp" : "arrowDown");
			var rt = arrowObj.AddComponent<RectTransform>();
			arrowObj.AddComponent<CanvasRenderer>();
			var img = arrowObj.AddComponent<Image>();
			img.sprite = Sprite.Create(arrowTex, new Rect(0, 0, arrowTex.width, arrowTex.height), new Vector2(0.5f, 0.5f));

			rt.SetParent(parent, false);
			rt.anchorMin = new Vector2(0, 0);
			rt.anchorMax = new Vector2(0, 0);
			rt.pivot = new Vector2(0, 0);
			rt.sizeDelta = new Vector2(32, 16);
			rt.anchoredPosition = anchoredPos;
			rt.gameObject.active = false;

			// Overlay arrow
			byte[] overlayBytes = System.IO.File.ReadAllBytes($"{Redux.GetAssetsPath()}/Image/arrow.png");
			Texture2D overlayTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			ImageConversion.LoadImage(overlayTex, overlayBytes);

			var overlayObj = new GameObject("overlay");
			var overlayRT = overlayObj.AddComponent<RectTransform>();
			overlayObj.AddComponent<CanvasRenderer>();
			var overlayImg = overlayObj.AddComponent<Image>();
			overlayImg.sprite = Sprite.Create(overlayTex, new Rect(0, 0, overlayTex.width, overlayTex.height), new Vector2(0.5f, 0.5f));

			if (isUp)
			{
				overlayRT.localScale = new Vector3(1, -1, 1);
			}

			overlayRT.SetParent(rt, false);
			overlayRT.anchorMin = new Vector2(0, 0);
			overlayRT.anchorMax = new Vector2(1, 1);
			overlayRT.pivot = new Vector2(0.5f, 0.5f);
			overlayRT.sizeDelta = Vector2.zero;
			overlayRT.anchoredPosition = isUp ? new Vector2(0, 1) : Vector2.zero;
		}

		private static void NightStart()
		{
			Redux.custom = false;
			Redux.LoadNight(GameObject.Find("TitleManager").GetComponent<TitleManager>());
		}

		private static void CustomNight()
		{
			Redux.togglingCustomNight = true;
		}

		private static void CustomNightStart()
		{
			Redux.custom = true;
			Redux.LoadNight(GameObject.Find("TitleManager").GetComponent<TitleManager>());
		}
	}

	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.ContinuePressed))]
	static class TitleManager_ContinuePressed_Patch
	{
		public static bool Prefix(TitleManager __instance)
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.MinimapPressed))]
	static class TitleManager_MinimapPressed_Patch
	{
		public static bool Prefix(TitleManager __instance)
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(TitleManager), nameof(TitleManager.Update))]
	static class TitleManager_Update_Patch
	{
		private static readonly double[] timecodes = { 0.9, 7.90, 13.6, 19.6, 24.85 };

		private static C[] clankerArray = { C.Freddy };
		private static int clanker = 0;
		private static double prevTime = 0;
		private static double menuTimer = 0;
		private static double customNightReadyTimer = 0;
		private static bool customNightMenuVisible = false;
		private static double arrowTimer = 0;
		private static double levelChangeTimer = 0;
		private static double levelChangeTarget = 0.25;
		private static bool pressingUp = false;
		private static bool pressingDown = false;

		public static void Postfix(TitleManager __instance)
		{
			Global.data.night = Redux.night;

			var video = __instance.titleVideo;
			HandleMenuIntro(video);
			HandleVideoSequence(video);
			HandleCustomNightMenu(__instance);
			HandlePortraitInteractions();
			UpdateArrowTimer();
		}

		public static void Reset()	// I don't like this, change it later
		{
			clankerArray = new C[] { C.Freddy };
			clanker = 0;
			prevTime = 0;
			menuTimer = 0;
			customNightReadyTimer = 0;
			customNightMenuVisible = false;
			arrowTimer = 0;
			levelChangeTimer = 0;
			levelChangeTarget = 0.25;
			pressingUp = false;
			pressingDown = false;
		}

		private static void HandleMenuIntro(VideoPlayer video)
		{
			if (menuTimer >= 0.0)
			{
				if (menuTimer < 1.5)
				{
					menuTimer += Time.deltaTime;
					return;
				}

				if (!Redux.playFired)
				{
					Redux.playFired = true;
					video.Play();
				}

				if (menuTimer >= 3.75)
				{
					var realTimeText = GameObject.Find("UI Canvas/MainTitle/LogoMask/RealTimeText");
					if (!Redux.enableFired)
					{
						Redux.enableFired = true;
						realTimeText.gameObject.active = true;
					}

					float fade = (float)(menuTimer - 3.75);
					realTimeText.GetComponent<TextMeshProUGUI>().color = new Color(0.549f, 0, 0, fade);
					GameObject.Find("Static Source").GetComponent<AudioSource>().volume = (float)((5.75 - menuTimer) / 2.0);
				}

				if (menuTimer >= 4.75)
				{
					string path = "UI Canvas/MainTitle/";
					var names = new[] { "NewGame-Continue Button", "Continue Night Number Text", "Minimap Button", "TAB info text" };
					var fadeColor = new Color(1, 1, 1, (float)(menuTimer - 4.75));

					foreach (var name in names)
					{
						var obj = GameObject.Find(path + name);
						var text = obj.GetComponent<TextMeshProUGUI>();
						text.color = fadeColor;
						obj.gameObject.active = name != "Minimap Button" || Redux.night > 6;
					}
				}

				menuTimer += Time.deltaTime;
				if (menuTimer >= 5.75)
				{
					menuTimer = -1.0;
				}
			}
		}

		private static void HandleVideoSequence(VideoPlayer video)
		{
			if (clankerArray.Length == 1)
			{
				C[] clankerSubArray = { C.Bonnie, C.Chica, C.Foxy };
				Redux.Shuffle(clankerSubArray);
				clankerArray = clankerArray.Concat(clankerSubArray).ToArray();
			}

			if (video.time != prevTime)
			{
				prevTime = 0;

				if (video.isPlaying && video.time > timecodes[(int)clankerArray[clanker] + 1])
				{
					video.Pause();
					clanker++;
					if (clanker >= 4)
					{
						clanker = 0;
						C last = clankerArray.Last();
						do Redux.Shuffle(clankerArray);
						while (clankerArray.First() == last);
					}

					prevTime = video.time;
					video.time = timecodes[(int)clankerArray[clanker]];
					video.Play();
				}
			}
		}

		private static void HandleCustomNightMenu(TitleManager instance)
		{
			if (Redux.togglingCustomNight)
			{
				var customNight = instance.mainUI.transform.Find("MainTitle/Minimap Button");
				var customNightReady = instance.mainUI.transform.Find("MainTitle/CustomNightReady");
				var customNightMenu = instance.mainUI.transform.Find("customNightCharacters").GetComponent<RectTransform>();
				var customNightMenuImages = customNightMenu.transform.GetComponentsInChildren<Image>(true);

				if (customNightReadyTimer < 2)
				{
					customNight.GetComponent<Button>().interactable = false;

					var videoColor = instance.titleVideo.GetComponent<SpriteRenderer>().color;
					var customNightReadyColor = customNightReady.GetComponent<TextMeshProUGUI>().color;

					if (!customNightMenuVisible)
					{
						FadeInCustomNightMenu(customNightReady, customNightMenuImages, ref videoColor, ref customNightReadyColor);
					}
					else
					{
						FadeOutCustomNightMenu(customNightReady, customNightMenuImages, ref videoColor, ref customNightReadyColor);
					}

					instance.titleVideo.GetComponent<SpriteRenderer>().color = videoColor;
					customNightReady.GetComponent<TextMeshProUGUI>().color = customNightReadyColor;
					customNightReadyTimer += Time.deltaTime;
				}
				else
				{
					customNight.GetComponent<Button>().interactable = true;
					customNightMenuVisible = !customNightMenuVisible;
					customNightReadyTimer = 0;
					Redux.togglingCustomNight = false;
				}
			}
		}

		private static void FadeInCustomNightMenu(Transform startText, Image[] images, ref Color videoColor, ref Color textColor)
		{
			if (customNightReadyTimer < 1)
			{
				videoColor.a = 1 - (float)customNightReadyTimer;
				return;
			}

			videoColor.a = 0;
			textColor.a = (float)customNightReadyTimer - 1;

			foreach (var item in images)
			{
				float mult = 1f;

				if (!(item.name == "arrowUp" || item.name == "arrowDown" || item.name == "button"))
				{
					if (item.name == "portrait")
					{
						var name = item.transform.parent.name
							.Split(new[] { "Portrait" }, StringSplitOptions.None)
							.FirstOrDefault();

						if (!Redux.enabled[Redux.GetIdxFromName(name)])
						{
							mult = 0.2f;
						}

						item.transform.parent.Find("name")
							.GetComponent<TextMeshProUGUI>().color =
							new Color(1, 1, 1, (float)customNightReadyTimer - 1);

						var level = item.transform.parent.Find("level").GetComponent<TextMeshProUGUI>();
						level.color = new Color(1, 1, 1, ((float)customNightReadyTimer - 1) * mult);
						level.outlineWidth = 0.1f;
					}
				}

				item.color = new Color(1, 1, 1, ((float)customNightReadyTimer - 1) * mult);
			}

			if (!startText.gameObject.active)
			{
				startText.gameObject.active = true;
			}
		}

		private static void FadeOutCustomNightMenu(Transform startText, Image[] images, ref Color videoColor, ref Color textColor)
		{
			if (customNightReadyTimer < 1)
			{
				textColor.a = 1 - (float)customNightReadyTimer;

				foreach (var item in images)
				{
					float mult = 1f;
					if (!(item.name == "arrowUp" || item.name == "arrowDown" || item.name == "button"))
					{
						if (item.name == "portrait")
						{
							var name = item.transform.parent.name
								.Split(new[] { "Portrait" }, StringSplitOptions.None)
								.FirstOrDefault();

							if (!Redux.enabled[Redux.GetIdxFromName(name)])
							{
								mult = 0.2f;
							}

							item.transform.parent.Find("name")
								.GetComponent<TextMeshProUGUI>().color =
								new Color(1, 1, 1, 1 - (float)customNightReadyTimer);
							item.transform.parent.Find("level")
								.GetComponent<TextMeshProUGUI>().color =
								new Color(1, 1, 1, (1 - (float)customNightReadyTimer) * mult);
						}
					}

					item.color = new Color(1, 1, 1, (1 - (float)customNightReadyTimer) * mult);
				}
			}
			else
			{
				textColor.a = 0;

				foreach (var item in images)
				{
					item.color = new Color(1, 1, 1, 0);
					if (item.name == "portrait")
					{
						item.transform.parent.Find("name").GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);

						var level = item.transform.parent.Find("level").GetComponent<TextMeshProUGUI>();
						level.color = new Color(1, 1, 1, 0);
						level.outlineWidth = 0;
					}
				}

				videoColor.a = (float)customNightReadyTimer - 1;
				if (startText.gameObject.active)
				{
					startText.gameObject.active = false;
				}
			}
		}

		private static void HandlePortraitInteractions()
		{
			var customNightMenu = GameObject.Find("UI Canvas/customNightCharacters").GetComponent<RectTransform>();
			Vector2 mousePos = Vector2.Scale(Mouse.current.position.ReadValue() -
				new Vector2(Screen.width / 2f, Screen.height / 2f),
				new Vector2(0.66f, 0.66f));

			var containers = GameObject.FindObjectsOfType<RectTransform>()
				.Where(rt => rt.name.EndsWith("Portrait"));

			foreach (var container in containers)
				UpdatePortrait(container, customNightMenu, mousePos);
		}

		private static void UpdatePortrait(RectTransform container, RectTransform menu, Vector2 mousePos)
		{
			var hoverBorder = container.Find("HoverBorder").gameObject;
			Rect rect = container.rect;

			float left = container.anchoredPosition.x - rect.width / 2 + menu.localPosition.x;
			float right = container.anchoredPosition.x + rect.width / 2 + menu.localPosition.x;
			float top = container.anchoredPosition.y + rect.height / 2 + menu.localPosition.y;
			float bottom = container.anchoredPosition.y - rect.height / 2 + menu.localPosition.y;

			var name = container.name.Split(new string[] { "Portrait" }, System.StringSplitOptions.None).FirstOrDefault();
			var arrowUp = hoverBorder.transform.parent.Find("arrowUp");
			var arrowDown = hoverBorder.transform.parent.Find("arrowDown");

			if (!Redux.togglingCustomNight && customNightMenuVisible &&
				mousePos.x >= left && mousePos.x <= right && mousePos.y >= bottom && mousePos.y <= top)
			{
				HandleHover(container, hoverBorder, arrowUp, arrowDown, name, mousePos, left, bottom);
			}
			else
			{
				hoverBorder.SetActive(false);
				arrowUp.gameObject.SetActive(false);
				arrowDown.gameObject.SetActive(false);
			}

			var alpha = arrowTimer < 1 ? 1 - arrowTimer : arrowTimer - 1;
			arrowUp.Find("overlay").GetComponent<Image>().color = new Color(1, 1, 1, (float)alpha);
			arrowDown.Find("overlay").GetComponent<Image>().color = new Color(1, 1, 1, (float)alpha);
		}

		private static void HandleHover(RectTransform container, GameObject hoverBorder, Transform arrowUp, Transform arrowDown,
			string name, Vector2 mousePos, float left, float bottom)
		{
			hoverBorder.SetActive(true);
			var level = hoverBorder.transform.parent.Find("level");
			bool enabled = Redux.enabled[Redux.GetIdxFromName(name)];

			arrowUp.gameObject.SetActive(enabled);
			arrowDown.gameObject.SetActive(enabled);

			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				HandleClick(hoverBorder, arrowUp, arrowDown, name, mousePos, left, bottom, enabled, level);
			}

			UpdateHoldToChangeLevel(name);
			level.GetComponent<TextMeshProUGUI>().text = Redux.customLevels[Redux.GetIdxFromName(name)].ToString();
		}

		private static void HandleClick(GameObject hoverBorder, Transform arrowUp, Transform arrowDown,
			string name, Vector2 mousePos, float left, float bottom, bool enabled, Transform level)
		{
			var upRT = arrowUp.GetComponent<RectTransform>();
			var downRT = arrowDown.GetComponent<RectTransform>();
			int idx = Redux.GetIdxFromName(name);
			bool clicked = false;

			void HandleArrow(bool up)
			{
				if (enabled)
				{
					Redux.ChangeLevel(idx, up);
					pressingUp = up;
					pressingDown = !up;
				}
				else ActivateCharacter(hoverBorder, name, level);
			}

			bool clickedUp = Inside(mousePos, left, bottom, upRT);
			bool clickedDown = !clickedUp && Inside(mousePos, left, bottom, downRT);

			if (clickedUp || clickedDown)
			{
				HandleArrow(clickedUp);
				clicked = true;
			}

			if (!clicked)
			{
				Redux.enabled[idx] = !enabled;
				float alpha = enabled ? 0.2f : 1f;
				var portrait = hoverBorder.transform.parent.Find("portrait").GetComponent<Image>();
				portrait.color = new Color(1, 1, 1, alpha);
				level.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, alpha);
			}
		}

		private static bool Inside(Vector2 pos, float left, float bottom, RectTransform rt)
		{
			var p = rt.anchoredPosition;
			var s = rt.sizeDelta;
			float x1 = left + p.x, y1 = bottom + p.y;
			return pos.x >= x1 && pos.x <= x1 + s.x && pos.y >= y1 && pos.y <= y1 + s.y;
		}

		private static void ActivateCharacter(GameObject hoverBorder, string name, Transform level)
		{
			Redux.enabled[Redux.GetIdxFromName(name)] = true;
			var portrait = hoverBorder.transform.parent.Find("portrait");
			portrait.GetComponent<Image>().color = Color.white;
			level.GetComponent<TextMeshProUGUI>().color = Color.white;
		}

		private static void UpdateHoldToChangeLevel(string name)
		{
			if (Mouse.current.leftButton.isPressed)
			{
				levelChangeTimer += (pressingUp ? 1 : pressingDown ? -1 : 0) * Time.deltaTime;
			}
			else
			{
				levelChangeTimer = 0;
				pressingUp = pressingDown = false;
			}

			double absTimer = Mathf.Abs((float)levelChangeTimer);
			if (absTimer >= 0.25 && absTimer >= levelChangeTarget)
			{
				bool increase = levelChangeTimer > 0;
				Redux.ChangeLevel(Redux.GetIdxFromName(name), increase);
				levelChangeTarget += 0.08;
			}
			else if (absTimer < 0.25)
			{
				levelChangeTarget = 0.25;
			}
		}

		private static void UpdateArrowTimer()
		{
			arrowTimer += Time.deltaTime;
			if (arrowTimer >= 2)
			{
				arrowTimer -= 2;
			}
		}
	}
}
