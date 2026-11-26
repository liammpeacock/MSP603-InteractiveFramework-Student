using UnityEngine;

namespace MSP603.TowerDefense
{
    public static class AudioControlState
    {
        public const string MusicEnabledPreference = "MSP603_MusicEnabled";
        public const string SfxEnabledPreference = "MSP603_SFXEnabled";

        public static float MasterVolume { get; private set; } = 1f;
        public static float MusicVolume { get; private set; } = 1f;
        public static float SfxVolume { get; private set; } = 1f;
        public static bool MusicEnabled { get; private set; } = LoadEnabled(MusicEnabledPreference);
        public static bool SfxEnabled { get; private set; } = LoadEnabled(SfxEnabledPreference);

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            GameplayAudioEvents.SetParameter("MasterVolume", MasterVolume);
        }

        public static void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            GameplayAudioEvents.SetParameter("MusicVolume", MusicVolume);
        }

        public static void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            GameplayAudioEvents.SetParameter("SfxVolume", SfxVolume);
        }

        public static void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            PlayerPrefs.SetInt(MusicEnabledPreference, enabled ? 1 : 0);
            PlayerPrefs.Save();
            GameplayAudioEvents.SetParameter("MusicEnabled", enabled ? 1f : 0f);
        }

        public static void SetSfxEnabled(bool enabled)
        {
            SfxEnabled = enabled;
            PlayerPrefs.SetInt(SfxEnabledPreference, enabled ? 1 : 0);
            PlayerPrefs.Save();
            GameplayAudioEvents.SetParameter("SfxEnabled", enabled ? 1f : 0f);
        }

        private static bool LoadEnabled(string preference)
        {
            return PlayerPrefs.GetInt(preference, 1) != 0;
        }

        public static void PublishCurrentState()
        {
            GameplayAudioEvents.SetParameter("MasterVolume", MasterVolume);
            GameplayAudioEvents.SetParameter("MusicVolume", MusicVolume);
            GameplayAudioEvents.SetParameter("SfxVolume", SfxVolume);
            GameplayAudioEvents.SetParameter("MusicEnabled", MusicEnabled ? 1f : 0f);
            GameplayAudioEvents.SetParameter("SfxEnabled", SfxEnabled ? 1f : 0f);
        }
    }
}
