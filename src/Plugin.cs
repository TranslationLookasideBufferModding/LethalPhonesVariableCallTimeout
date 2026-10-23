using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace LethalPhonesCallTimeout
{
    [BepInPlugin(Id, "LethalPhones Call Timeout", "1.0.0")]
    [BepInDependency("LethalPhones", BepInDependency.DependencyFlags.HardDependency)]
    [BepInIncompatibility("private.phones.calltimeout")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Id = "lethalphones.calltimeout";
        private ConfigEntry<TimeoutMode> mode;
        private ConfigEntry<float> fixedSeconds;
        private ConfigEntry<float> maximumRingtoneSeconds;
        private ConfigEntry<float> ringtoneGraceSeconds;
        private Harmony harmony;

        private void Awake()
        {
            bool enabled = Config.Bind("General", "Enabled", true, "Enable the unanswered-call timer patches.").Value;
            mode = Config.Bind("Calls", "Mode", TimeoutMode.Fixed,
                "Fixed: use FixedSeconds. RingtoneLength: use the called phone's ringtone length.");
            fixedSeconds = Config.Bind("Calls", "FixedSeconds", 45f,
                new ConfigDescription("Fixed timeout and fallback when a ringtone cannot be resolved (seconds).",
                    new AcceptableValueRange<float>(0.1f, 3600f)));
            maximumRingtoneSeconds = Config.Bind("Calls", "MaximumRingtoneSeconds", 300f,
                new ConfigDescription("Upper limit for ringtone-based timeouts (seconds).",
                    new AcceptableValueRange<float>(0.1f, 3600f)));
            ringtoneGraceSeconds = Config.Bind("Calls", "RingtoneGraceSeconds", 0f,
                new ConfigDescription("Optional extra seconds after the ringtone length to allow for network delay.",
                    new AcceptableValueRange<float>(0f, 10f)));
            if (!enabled) { Logger.LogInfo("Call timeout patches disabled by configuration."); return; }
            harmony = new Harmony(Id);
            try
            {
                CallTimeoutPatches.SettingsProvider = ReadSettings;
                CallTimeoutPatches.Warning = delegate(string message) { Logger.LogWarning(message); };
                LethalPhonesRuntime.Initialize();
                CallTimeoutPatches.Install(harmony);
                Logger.LogInfo("Call timeout patches active. Mode=" + mode.Value + ", fixed/fallback=" + fixedSeconds.Value + "s.");
            }
            catch (Exception error)
            {
                harmony.UnpatchSelf();
                Logger.LogError("Call timeout patches disabled because the expected LethalPhones layout was not found: " + error);
            }
        }

        private TimeoutSettings ReadSettings()
        {
            return new TimeoutSettings { Mode = mode.Value, FixedSeconds = fixedSeconds.Value,
                MaximumRingtoneSeconds = maximumRingtoneSeconds.Value, RingtoneGraceSeconds = ringtoneGraceSeconds.Value };
        }

        private void OnDestroy()
        {
            if (harmony != null) harmony.UnpatchSelf();
        }
    }
}
