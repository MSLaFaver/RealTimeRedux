using HarmonyLib;
using Il2Cpp;
using System.Collections.Generic;
using UnityEngine.Video;

namespace Redux
{
	[HarmonyPatch(typeof(VideoPlayer), "get_frame")]
	static class VideoPlayer_GetFrame_Patch
	{
		public static void Postfix(VideoPlayer __instance)
		{
			var cams = new Dictionary<string, CamManager.CAMS>{
				{ "CAM 2A - FOXY - RUN DOWN - OFFICE AUDIO", CamManager.CAMS.CAM2A },
				{ "CAM 4A - FOXY - RUN - OFFICE AUDIO (1)", CamManager.CAMS.CAM4A }
			};

			if (cams.TryGetValue(__instance.clip.name, out var cam))
			{
				bool viewingCam = CamManager.instance.isCamUp && CamManager.instance.currentCam == cam;
				__instance.SetDirectAudioMute(0, !viewingCam);
			}
		}
	}
}
