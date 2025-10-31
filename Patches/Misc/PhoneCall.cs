using HarmonyLib;
using Il2Cpp;

namespace Redux
{
	[HarmonyPatch(typeof(TranslatedAudio), nameof(TranslatedAudio.PlayVoiceLine))]
	static class TranslatedAudio_PlayVoiceLine_Patch
	{
		public static bool Prefix(TranslatedAudio __instance)
		{
			bool gotClip = false;

			if (Redux.night <= 5)
			{
				var path = $"{Redux.GetAssetsPath()}night{Redux.night}.ogg";
				var clip = Redux.LoadOgg(path);
				if (clip != null)
				{
					gotClip = true;
					__instance.audioSource.clip = clip;
					__instance.audioSource.Play();
				}
			}

			if (!gotClip)
			{
				__instance.audioSource.Stop();
			}

			return false;
		}
	}
}
