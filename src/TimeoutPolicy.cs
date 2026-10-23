using System;

namespace LethalPhonesCallTimeout
{
    internal enum TimeoutMode { Fixed, RingtoneLength }

    internal sealed class TimeoutSettings
    {
        internal TimeoutMode Mode = TimeoutMode.Fixed;
        internal float FixedSeconds = 45f;
        internal float MaximumRingtoneSeconds = 300f;
        internal float RingtoneGraceSeconds = 0f;
    }

    internal static class TimeoutPolicy
    {
        internal const float MinimumSeconds = 0.1f;
        internal const float MaximumSeconds = 3600f;

        internal static bool ValidPositive(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }

        internal static float Resolve(TimeoutSettings settings, float? ringtoneLength)
        {
            if (settings == null) settings = new TimeoutSettings();
            float fixedSeconds = Sanitize(settings.FixedSeconds, 45f);
            if (settings.Mode != TimeoutMode.RingtoneLength || !ringtoneLength.HasValue || !ValidPositive(ringtoneLength.Value))
                return fixedSeconds;

            float maximum = Sanitize(settings.MaximumRingtoneSeconds, 300f);
            float grace = settings.RingtoneGraceSeconds;
            if (float.IsNaN(grace) || float.IsInfinity(grace) || grace < 0f) grace = 0f;
            grace = Math.Min(grace, 10f);
            return Math.Max(MinimumSeconds, Math.Min(maximum, ringtoneLength.Value + grace));
        }

        private static float Sanitize(float value, float fallback)
        {
            if (!ValidPositive(value)) value = fallback;
            return Math.Max(MinimumSeconds, Math.Min(MaximumSeconds, value));
        }
    }
}
