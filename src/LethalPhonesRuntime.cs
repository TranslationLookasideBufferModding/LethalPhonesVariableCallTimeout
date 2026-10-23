using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace LethalPhonesCallTimeout
{
    internal static class LethalPhonesRuntime
    {
        private static FieldInfo phones;
        private static Type behaviorType;
        private static FieldInfo number;
        private static FieldInfo ringtoneId;
        private static FieldInfo ringtones;
        private static FieldInfo switchboardRingtone;
        private static Type switchboardType;
        private static string defaultRingtone;

        internal static void Initialize()
        {
            Type behavior = RequireType("Scoops.misc.PhoneBehavior");
            behaviorType = behavior;
            phones = RequireField(RequireType("Scoops.service.PhoneNetworkHandler"), "allPhoneBehaviors");
            number = RequireField(behavior, "phoneNumber");
            ringtoneId = RequireField(behavior, "phoneRingtoneId");
            Type customizations = RequireType("Scoops.customization.CustomizationManager");
            ringtones = RequireField(customizations, "ringtoneCustomizations");
            defaultRingtone = (string)RequireField(customizations, "DEFAULT_RINGTONE").GetValue(null);
            switchboardType = RequireType("Scoops.gameobjects.SwitchboardPhone");
            switchboardRingtone = RequireField(RequireType("Scoops.service.PhoneAssetManager"), "switchboardRing");
        }

        internal static float? OutgoingRingtoneLength(short targetNumber)
        {
            IEnumerable all = phones.GetValue(null) as IEnumerable;
            float? result = FindLength(all, targetNumber);
            if (result.HasValue) return result;
            // The contact-list cache can lag behind spawning/joining. Resolve
            // live phone components before falling back to the fixed timeout.
            if (typeof(UnityEngine.Object).IsAssignableFrom(behaviorType))
                return FindLength(UnityEngine.Object.FindObjectsOfType(behaviorType), targetNumber);
            return null;
        }

        private static float? FindLength(IEnumerable all, short targetNumber)
        {
            if (all == null) return null;
            foreach (object phone in all)
            {
                if (IsMissing(phone)) continue;
                if ((short)number.GetValue(phone) == targetNumber) return RingtoneLength(phone);
            }
            return null;
        }

        internal static float? RingtoneLength(object phone)
        {
            if (IsMissing(phone)) return null;
            if (switchboardType.IsInstanceOfType(phone)) return ClipLength(switchboardRingtone.GetValue(null));
            IDictionary clips = ringtones.GetValue(null) as IDictionary;
            if (clips == null) return null;
            string id = ringtoneId.GetValue(phone) as string;
            object clip = id != null && clips.Contains(id) ? clips[id] : null;
            if (IsMissing(clip) && defaultRingtone != null && clips.Contains(defaultRingtone)) clip = clips[defaultRingtone];
            return ClipLength(clip);
        }

        private static float? ClipLength(object value)
        {
            AudioClip clip = value as AudioClip;
            if (clip == null) return null;
            float length = clip.length;
            return TimeoutPolicy.ValidPositive(length) ? (float?)length : null;
        }

        private static bool IsMissing(object value)
        {
            if (value == null) return true;
            UnityEngine.Object unityObject = value as UnityEngine.Object;
            return !object.ReferenceEquals(unityObject, null) && unityObject == null;
        }

        private static Type RequireType(string name)
        {
            Type type = AccessTools.TypeByName(name);
            if (type == null) throw new TypeLoadException(name);
            return type;
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            FieldInfo field = AccessTools.Field(type, name);
            if (field == null) throw new MissingFieldException(type.FullName, name);
            return field;
        }
    }
}
