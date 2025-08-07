using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyNotifs;
using UnityEngine;

namespace TootTallyLocalOffset
{
    public static class LocalOffsetPatches
    {
        private static float _saveToFileTimer;
        private static Dictionary<string, int> _trackRefToOffsetDict = new Dictionary<string, int>();

        [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
        [HarmonyPostfix]
        public static void OnGameControllerStart(GameController __instance)
        {
            var key = GlobalVariables.chosen_track;
            if (_trackRefToOffsetDict.TryGetValue(key, out int offset) && offset != 0)
            {
                Plugin.LogInfo($"Latency adjusted by local offset by {offset}.");
                __instance.latency_offset += offset * 0.001f;
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
        [HarmonyPrefix]
        public static void OnGameControllerUpdate(GameController __instance)
        {
            if (Input.GetKeyDown(Plugin.Instance.OffsetIncreaseKeybind.Value))
                AddOffsetToDict(__instance, GlobalVariables.chosen_track, Plugin.Instance.OffsetIncrements.Value);
            if (Input.GetKeyDown(Plugin.Instance.OffsetDecreaseKeybind.Value))
                AddOffsetToDict(__instance, GlobalVariables.chosen_track, -Plugin.Instance.OffsetIncrements.Value);


            if (_saveToFileTimer > 0)
            {
                _saveToFileTimer -= Time.fixedUnscaledDeltaTime;
                if (_saveToFileTimer <= 0)
                {
                    //FileHelper.SaveNewOffsetsToFile(); // Maybe if this end up causing lag
                }
            }
        }

        public static void AddOffsetToDict(GameController __instance, string key, float value)
        {
            if (!_trackRefToOffsetDict.ContainsKey(key))
                _trackRefToOffsetDict.Add(key, 0);
            _trackRefToOffsetDict[key] += (int)value;


            __instance.latency_offset += value * 0.001f;
            TootTallyNotifManager.DisplayNotif($"New Local Offset: {_trackRefToOffsetDict[key]}ms");
            FileHelper.SaveToTootTallyAppData(Plugin.FILE_OFFSET_NAME, _trackRefToOffsetDict);
            //FileHelperOffset.SaveOffetFile(_trackRefToOffsetDict);
            //_saveToFileTimer = .5f;
        }

        public static void LoadOffsetsFromFile()
        {
            _trackRefToOffsetDict = FileHelper.LoadFromTootTallyAppData<Dictionary<string, int>>(Plugin.FILE_OFFSET_NAME);
            if (_trackRefToOffsetDict == default)
            {
                Plugin.LogInfo($"Couldn't find offset file, creating new one!");
                _trackRefToOffsetDict = new Dictionary<string, int>();
                FileHelper.SaveToTootTallyAppData(Plugin.FILE_OFFSET_NAME, _trackRefToOffsetDict, true);
            }
        }

    }
}
