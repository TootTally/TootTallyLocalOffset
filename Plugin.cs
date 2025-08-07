using BaboonAPI.Hooks.Initializer;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using TootTallyCore.Utils.TootTallyModules;
using TootTallySettings;
using UnityEngine;
using UnityEngine.UIElements.UIR;

namespace TootTallyLocalOffset
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("TootTallyCore", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("TootTallySettings", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin, ITootTallyModule
    {
        public static Plugin Instance;

        public const string FILE_OFFSET_NAME = "TootTallyLocalOffsets.json";

        private Harmony _harmony;
        public ConfigEntry<bool> ModuleConfigEnabled { get; set; }
        public bool IsConfigInitialized { get; set; }

        //Change this name to whatever you want
        public string Name { get => PluginInfo.PLUGIN_NAME; set => Name = value; }

        public static TootTallySettingPage settingPage;

        public static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        public static void LogError(string msg) => Instance.Logger.LogError(msg);

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            _harmony = new Harmony(Info.Metadata.GUID);

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            ModuleConfigEnabled = TootTallyCore.Plugin.Instance.Config.Bind("Modules", "LocalOffets", true, "Allows offset per charts.");
            TootTallyModuleManager.AddModule(this);
            TootTallySettings.Plugin.Instance.AddModuleToSettingPage(this);
        }

        public void LoadModule()
        {
            string configPath = Path.Combine(Paths.BepInExRootPath, "config/");
            OffsetIncreaseKeybind = Config.Bind("General", "OffsetIncreaseKeybind", KeyCode.RightBracket, "Increase the local offset of the current chart");
            OffsetDecreaseKeybind = Config.Bind("General", "OffsetDecreaseKeybind", KeyCode.LeftBracket, "Decrease the local offset of the current chart");
            OffsetIncrements = Config.Bind("General", "OffsetIncrements", 1f, "Offset Increments per key presses.");

            settingPage = TootTallySettingsManager.AddNewPage("Local Offset", "Local Offset", 40f, new Color(0,0,0,0));
            if (settingPage != null) {
                settingPage.AddLabel("Increase Offset Keybind");
                settingPage.AddDropdown("Increase Offset Keybind", OffsetIncreaseKeybind);
                settingPage.AddLabel("Decrease Offset Keybind");
                settingPage.AddDropdown("Decrease Offset Keybind", OffsetDecreaseKeybind);
                settingPage.AddSlider("Increments (ms)", 1, 50, OffsetIncrements, true);
            }

            TootTallySettings.Plugin.TryAddThunderstoreIconToPageButton(Instance.Info.Location, Name, settingPage);

            LocalOffsetPatches.LoadOffsetsFromFile();

            _harmony.PatchAll(typeof(LocalOffsetPatches));
            LogInfo($"Module loaded!");
        }

        public void UnloadModule()
        {
            _harmony.UnpatchSelf();
            settingPage.Remove();
            LogInfo($"Module unloaded!");
        }

        public ConfigEntry<KeyCode> OffsetIncreaseKeybind { get; set; }
        public ConfigEntry<KeyCode> OffsetDecreaseKeybind { get; set; }
        public ConfigEntry<float> OffsetIncrements { get; set; }
    }
}