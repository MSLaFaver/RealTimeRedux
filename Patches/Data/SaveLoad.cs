using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(Global), nameof(Global.SaveGameToFile))]
	static class Global_SaveGameToFile_Patch
	{
		public static bool Prefix(object __instance)
		{
			try
			{
				string path = Global.GetSaveDataPath();
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				string savePath = Path.Combine(path, "redux.json");
				var wrapper = new { redux = new { Redux.night, Redux.beatMaxMode } };
				System.IO.File.WriteAllText(savePath, JsonConvert.SerializeObject(wrapper, Formatting.Indented));
			}
			catch (Exception e)
			{
				MelonLogger.Msg($"Exception while saving: {e}");
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Global), nameof(Global.LoadGameFromFile))]
	static class Global_LoadGameFromFile_Patch
	{
		public static bool Prefix(object __instance, ref bool __result)
		{
			try
			{
				string path = Path.Combine(Global.GetSaveDataPath(), "redux.json");

				if (File.Exists(path))
				{
					string json = System.IO.File.ReadAllText(path);
					dynamic wrapper = JsonConvert.DeserializeObject<dynamic>(json);

					var night = (int)wrapper.redux.night;
					var beatMaxMode = (bool)wrapper.redux.beatMaxMode && night >= 7;
					Redux.night = night;
					Redux.beatMaxMode = beatMaxMode;
					Global.data = new SaveData
					{
						night = night,
						firstTime = false
					};

					__result = true;
				}
			}
			catch (Exception e)
			{
				MelonLogger.Msg($"Exception while loading: {e}");
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Global), nameof(Global.DeleteSaveFile))]
	static class Global_DeleteSaveFile_Patch
	{
		public static bool Prefix(object __instance)
		{
			try
			{
				string path = Path.Combine(Global.GetSaveDataPath(), "redux.json");

				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception e)
			{
				MelonLogger.Msg($"Exception while deleting save: {e}");
			}

			Redux.night = 1;
			Redux.beatMaxMode = false;

			GameObject.Find("Title Video").gameObject.active = false;
			GameObject.Find("UI Canvas").gameObject.active = false;
			GameObject.Find("InterruptCanvas").GetComponent<InterruptMenu>().Resume();

			SceneTransitoner.LoadScene("Title");
			return false;
		}
	}
}
