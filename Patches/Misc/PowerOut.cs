using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Redux
{
	[HarmonyPatch(typeof(PowerOutSequence), "Update")]
	public static class PowerOutSequence_Update_Patch
	{
		private static float rngTimer = 0f;
		private static float stageTimer = 0f;
		private static int flashBuffer = 0;

		private enum PowerOutState
		{
			PowerOut,
			Freddy,
			Flicker,
			LightsOut,
			Jumpscare
		}

		private static PowerOutState state;
		private static System.Random rng = new System.Random();
		private static GameObject player;
		private static AudioClip clip;
		private static AudioClip powerOutAudio = null;

		public static bool Prefix(PowerOutSequence __instance)
		{
			if (powerOutAudio == null)
			{
				powerOutAudio = Redux.LoadOgg($"{Redux.GetAssetsPath()}powerOut.ogg");

				byte[] bytes = System.IO.File.ReadAllBytes($"{Redux.GetAssetsPath()}freddy.png");
				Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				ImageConversion.LoadImage(tex, bytes);

				MeshRenderer renderer = __instance.poweroutVideo.GetComponent<MeshRenderer>();
				Material mat = new Material(renderer.material)
				{
					mainTexture = tex
				};
				renderer.material = mat;
			}

			var nightManager = NightManager.Instance;
			if (!nightManager.isNightOver && nightManager.isPowerOut)
			{
				rngTimer += Time.deltaTime;
				stageTimer += Time.deltaTime;

				switch (state)
				{
					case PowerOutState.PowerOut:
						if (rngTimer >= 5f)
						{
							rngTimer = 0f;
							if (rng.Next(5) == 0 || stageTimer >= 20f)
							{
								state = PowerOutState.Freddy;
								stageTimer = 0f;
								player = GameObject.Find("Office/West Light/LightSound");
								clip = player.GetComponent<AudioSource>().clip;
								player.GetComponent<AudioSource>().loop = false;
								player.GetComponent<AudioSource>().clip = powerOutAudio;
								player.GetComponent<AudioSource>().volume = 0.4f;
								player.GetComponent<AudioSource>().enabled = true;
								player.GetComponent<AudioSource>().Play();
								__instance.poweroutVideo.enabled = false;
							}
						}
						break;

					case PowerOutState.Freddy:
						if (flashBuffer >= 6)
						{
							flashBuffer = 0;
							__instance.poweroutVideo.GetComponent<MeshRenderer>().enabled = rng.Next(3) != 0;
						}
						flashBuffer++;

						if (rngTimer >= 5f)
						{
							rngTimer = 0f;
							if (rng.Next(5) == 0 || stageTimer >= 20f)
							{
								state = PowerOutState.Flicker;
								stageTimer = 0f;
								__instance.primedToAttack = true;
								player.GetComponent<AudioSource>().Stop();
								__instance.powerOutSound.clip = clip;
								__instance.powerOutSound.Play();
								__instance.lightFlicker.flickerAmount = 40;
								__instance.poweroutVideo.GetComponent<MeshRenderer>().enabled = true;
							}
						}
						break;

					case PowerOutState.Flicker:
						__instance.powerOutSound.volume = (rng.Next(2) == 0) ? 1 : 0;

						if (rngTimer >= 0.5f)
						{
							state = PowerOutState.LightsOut;
							rngTimer = 0f;
							stageTimer = 0f;
							__instance.powerOutSound.Stop();
							__instance.lightFlicker.flickerAmount = 100;
							__instance.poweroutVideo.GetComponent<MeshRenderer>().enabled = false;
						}
						break;

					case PowerOutState.LightsOut:
						if (rngTimer >= 2f)
						{
							rngTimer = 0f;
							if (rng.Next(5) == 0)
							{
								state = PowerOutState.Jumpscare;
								__instance.jumpscareSource.clip = __instance.freddyJump;
								__instance.jumpscareSource.gameObject.SetActive(true);
								__instance.jumpscaring = true;
							}
						}
						break;

					case PowerOutState.Jumpscare:
						if (rngTimer >= 0.55f)
						{
							SceneTransitoner.LoadScene("Game Over");
						}
						break;
				}
			}

			return false;
		}
	}
}
