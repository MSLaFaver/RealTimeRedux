using Il2Cpp;
using MelonLoader;
using NVorbis;
using System.Reflection;
using UnityEngine;

namespace Redux
{
	enum C  // clanker
	{
		Freddy = 0,
		Bonnie = 1,
		Chica = 2,
		Foxy = 3
	}

	public class Redux : MelonMod
	{
		public static int night = 1;
		public static bool beatMaxMode = false;

		public static bool custom = false;
		public static bool togglingCustomNight = false;
		public static bool[] enabled = { false, false, false, false };
		public static int[] customLevels = { 0, 0, 0, 0 };
		public static bool playFired = false;
		public static bool enableFired = false;
		public static double camTimer;
		public static int foxyBangs = 0;

		public static string GetVersion()
		{
			return System.Reflection.Assembly.GetExecutingAssembly()
				.GetCustomAttribute<MelonLoader.MelonInfoAttribute>()?.Version;
		}

		public static string GetAssetsPath()
		{
			var melonInfo = System.Reflection.Assembly.GetExecutingAssembly()
				.GetCustomAttribute<MelonLoader.MelonInfoAttribute>();
			return "Mods/" + melonInfo?.Author + "-Redux/Assets";
		}

		public static void ChangeLevel(int clanker, bool increase)
		{
			if (increase)
			{
				if (customLevels[clanker] < 20)
				{
					customLevels[clanker]++;
				}
			}
			else
			{
				if (customLevels[clanker] > 0)
				{
					customLevels[clanker]--;
				}
			}
		}

		public static int GetIdxFromName(string name)
		{
			C idx = C.Freddy;

			switch (name.ToLower())
			{
				case "bonnie":
					idx = C.Bonnie;
					break;
				case "chica":
					idx = C.Chica;
					break;
				case "foxy":
					idx = C.Foxy;
					break;
			}

			return (int)idx;
		}

		public static void ResetMenu()
		{
			custom = false;
			togglingCustomNight = false;
			playFired = false;
			enableFired = false;
			camTimer = 0.0;
		}

		public static void ResetMain()
		{
			foxyBangs = 0;
		}

		public static AudioClip LoadOgg(string path)
		{
			AudioClip clip = null;

			try
			{
				var vorbis = new VorbisReader(path);
				int channels = vorbis.Channels;
				int sampleRate = vorbis.SampleRate;
				float[] buffer = new float[vorbis.TotalSamples * channels];
				vorbis.ReadSamples(buffer, 0, buffer.Length);

				clip = AudioClip.Create(
					System.IO.Path.GetFileNameWithoutExtension(path),
					buffer.Length / channels,
					channels,
					sampleRate,
					false);
				clip.SetData(buffer, 0);
			}
			catch { }

			return clip;
		}

		public static double GetCamPosition()
		{
			var pos = 2.0;

			if (camTimer < 5.0)
			{
				pos = 2.0 + (camTimer / 5.0) * (-2.4 - 2.0);
			}
			else if (camTimer < 10.0)
			{
				pos = -2.4;
			}
			else if (camTimer < 15.0)
			{
				pos = -2.4 + ((camTimer - 10.0) / 5.0) * (2.0 + 2.4);
			}

			return pos;
		}

		public static void DoCamTimer()
		{
			camTimer += Time.deltaTime;
			if (camTimer >= 20)
			{
				camTimer -= 20;
			}
		}

		public static void Shuffle<T>(T[] array)
		{
			System.Random rng = new System.Random();
			for (int n = array.Length - 1; n > 0; n--)
			{
				int k = rng.Next(n + 1);
				(array[n], array[k]) = (array[k], array[n]);
			}
		}

		public static int[] AI()
		{
			System.Random rng = new System.Random();

			int[][] levels = {
				new[] { 0, 0, 0, 0 },
				new[] { 0, 3, 1, 1 },
				new[] { 1, 0, 5, 2 },
				new[] { rng.Next(1) + 1, 2, 4, 6 },
				new[] { 3, 5, 7, 5 },
				new[] { 4, 10, 12, 16 },
				customLevels
			};

			return levels[night - Sub() - 1];
		}

		public static int Sub()
		{
			return night < 7 || custom ? 0 : 1;
		}

		public static void LoadNight(TitleManager __instance)
		{
			int forcedNightIndex = 0;
			__instance.continueButton?.SetActive(false);

			if (__instance.nights != null && forcedNightIndex < __instance.nights.Length)
			{
				NightData forcedNight = __instance.nights[forcedNightIndex];

				if (forcedNight != null)
				{
					Global.nightData = forcedNight;
					SceneTransitoner.LoadScene("ShowNight");
				}
			}
		}
	}
}