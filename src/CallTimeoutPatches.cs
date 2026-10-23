using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace LethalPhonesCallTimeout
{
    internal static class CallTimeoutPatches
    {
        internal static Func<TimeoutSettings> SettingsProvider = delegate { return new TimeoutSettings(); };
        internal static Action<string> Warning = delegate { };
        private static bool lookupWarningLogged;

        internal static void Install(Harmony patcher)
        {
            foreach (string name in new string[] { "Scoops.misc.PlayerPhone", "Scoops.gameobjects.SwitchboardPhone" })
            {
                MethodInfo next = IteratorMoveNext(name, "CallTimeoutCoroutine", typeof(short));
                RequireField(next.DeclaringType, "<>4__this");
                RequireField(next.DeclaringType, "number");
                patcher.Patch(next, transpiler: new HarmonyMethod(typeof(CallTimeoutPatches), "TimeoutTranspiler"));
            }
            foreach (string name in new string[] { "Scoops.misc.PlayerPhone", "Scoops.misc.PhoneBehavior" })
            {
                MethodInfo next = IteratorMoveNext(name, "PhoneRingCoroutine", typeof(int));
                RequireField(next.DeclaringType, "<>4__this");
                RequireField(next.DeclaringType, "repeats");
                patcher.Patch(next, prefix: new HarmonyMethod(typeof(CallTimeoutPatches), "NoisePrefix"));
            }
        }

        internal static float ResolveOutgoingTimeout(object caller, short number)
        {
            TimeoutSettings settings = SettingsProvider();
            float? length = null;
            if (settings.Mode == TimeoutMode.RingtoneLength)
            {
                try { length = LethalPhonesRuntime.OutgoingRingtoneLength(number); }
                catch (Exception error) { WarnAboutFallback(error); }
            }
            return TimeoutPolicy.Resolve(settings, length);
        }

        private static IEnumerable<CodeInstruction> TimeoutTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            List<CodeInstruction> output = new List<CodeInstruction>();
            Type state = __originalMethod.DeclaringType;
            FieldInfo owner = RequireField(state, "<>4__this");
            FieldInfo number = RequireField(state, "number");
            int matches = 0;
            for (int i = 0; i < code.Count; i++)
            {
                ConstructorInfo constructor = i + 1 < code.Count ? code[i + 1].operand as ConstructorInfo : null;
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 15f &&
                    code[i + 1].opcode == OpCodes.Newobj && constructor != null && constructor.DeclaringType == typeof(WaitForSeconds))
                {
                    CodeInstruction first = new CodeInstruction(OpCodes.Ldarg_0);
                    first.labels.AddRange(code[i].labels); first.blocks.AddRange(code[i].blocks);
                    output.Add(first);
                    output.Add(new CodeInstruction(OpCodes.Ldfld, owner));
                    output.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    output.Add(new CodeInstruction(OpCodes.Ldfld, number));
                    output.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CallTimeoutPatches), "ResolveOutgoingTimeout")));
                    matches++;
                }
                else output.Add(code[i]);
            }
            if (matches != 1) throw new InvalidOperationException("Expected exactly one 15-second call timer; found " + matches);
            return output;
        }

        private static void NoisePrefix(object __instance)
        {
            Type state = __instance.GetType();
            object phone = RequireField(state, "<>4__this").GetValue(__instance);
            if (phone == null) return;
            string name = phone.GetType().FullName;
            if (name != "Scoops.misc.PlayerPhone" && name != "Scoops.gameobjects.SwitchboardPhone") return;
            TimeoutSettings settings = SettingsProvider();
            float? length = null;
            if (settings.Mode == TimeoutMode.RingtoneLength)
            {
                try { length = LethalPhonesRuntime.RingtoneLength(phone); }
                catch (Exception error) { WarnAboutFallback(error); }
            }
            float duration = TimeoutPolicy.Resolve(settings, length);
            FieldInfo repeats = RequireField(state, "repeats");
            repeats.SetValue(__instance, Math.Max((int)repeats.GetValue(__instance), (int)Math.Ceiling(duration / 4f)));
        }

        private static MethodInfo IteratorMoveNext(string typeName, string method, Type parameter)
        {
            Type type = AccessTools.TypeByName(typeName);
            MethodInfo target = type == null ? null : AccessTools.DeclaredMethod(type, method, new Type[] { parameter });
            if (target == null || target.ReturnType != typeof(IEnumerator)) throw new MissingMethodException(typeName, method);
            MethodInfo next = AccessTools.EnumeratorMoveNext(target);
            if (next == null) throw new MissingMethodException(typeName, method + ".MoveNext");
            return next;
        }

        private static void WarnAboutFallback(Exception error)
        {
            if (lookupWarningLogged) return;
            lookupWarningLogged = true;
            Warning("Ringtone lookup failed; using FixedSeconds as fallback. Further identical warnings are suppressed. " + error.Message);
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            FieldInfo field = AccessTools.Field(type, name);
            if (field == null) throw new MissingFieldException(type.FullName, name);
            return field;
        }
    }
}
