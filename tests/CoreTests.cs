using System;
using LethalPhonesCallTimeout;

internal static class CoreTests
{
    private static int count;
    private static void Equal(float expected, float actual, string name)
    {
        if (Math.Abs(expected - actual) > 0.001f) throw new Exception(name + ": expected " + expected + ", got " + actual);
        count++;
    }

    public static int Main()
    {
        try
        {
            TimeoutSettings settings = new TimeoutSettings();
            Equal(45f, TimeoutPolicy.Resolve(settings, null), "default fixed");
            Equal(45f, TimeoutPolicy.Resolve(settings, 17.25f), "fixed ignores clip");
            settings.FixedSeconds = 90f;
            Equal(90f, TimeoutPolicy.Resolve(settings, 5f), "configurable above old 45s cap");
            settings.Mode = TimeoutMode.RingtoneLength;
            Equal(17.25f, TimeoutPolicy.Resolve(settings, 17.25f), "individual clip");
            Equal(40f, TimeoutPolicy.Resolve(settings, 40f), "second individual clip");
            Equal(90f, TimeoutPolicy.Resolve(settings, null), "missing clip fallback");
            Equal(90f, TimeoutPolicy.Resolve(settings, float.NaN), "NaN clip fallback");
            Equal(90f, TimeoutPolicy.Resolve(settings, float.PositiveInfinity), "infinite clip fallback");
            Equal(90f, TimeoutPolicy.Resolve(settings, -2f), "negative clip fallback");
            Equal(90f, TimeoutPolicy.Resolve(settings, 0f), "zero clip fallback");
            settings.RingtoneGraceSeconds = 0.5f;
            Equal(17.75f, TimeoutPolicy.Resolve(settings, 17.25f), "grace");
            settings.MaximumRingtoneSeconds = 20f;
            Equal(20f, TimeoutPolicy.Resolve(settings, 100f), "ringtone cap");
            settings.RingtoneGraceSeconds = -1f;
            Equal(10f, TimeoutPolicy.Resolve(settings, 10f), "invalid grace");
            settings.RingtoneGraceSeconds = 999f;
            Equal(20f, TimeoutPolicy.Resolve(settings, 15f), "grace safety bound");
            settings.RingtoneGraceSeconds = 0f;
            Equal(0.1f, TimeoutPolicy.Resolve(settings, 0.01f), "minimum wait");
            settings.FixedSeconds = float.NaN;
            Equal(45f, TimeoutPolicy.Resolve(settings, null), "invalid fallback config");
            settings.MaximumRingtoneSeconds = float.NaN;
            Equal(300f, TimeoutPolicy.Resolve(settings, 999f), "invalid maximum config");
            settings.Mode = TimeoutMode.Fixed;
            settings.FixedSeconds = 99999f;
            Equal(3600f, TimeoutPolicy.Resolve(settings, null), "fixed safety limit");
            Equal(45f, TimeoutPolicy.Resolve(null, null), "null settings");
            settings.Mode = (TimeoutMode)99;settings.FixedSeconds = 12f;
            Equal(12f, TimeoutPolicy.Resolve(settings, 7f), "invalid mode falls back fixed");
            Console.WriteLine("CORE_TESTS_OK " + count);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
