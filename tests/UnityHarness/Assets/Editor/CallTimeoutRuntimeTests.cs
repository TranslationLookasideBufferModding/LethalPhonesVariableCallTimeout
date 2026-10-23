using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class CallTimeoutRuntimeTests
{
    private static Type settingsType,modeType,patches;
    private static int checks;
    private static readonly List<AudioClip> created=new List<AudioClip>();
    public static void Run()
    {
        string[] args=Environment.GetCommandLineArgs();
        string path=args[Array.IndexOf(args,"-pluginPath")+1];
        string core=args[Array.IndexOf(args,"-corePath")+1];
        AppDomain.CurrentDomain.AssemblyResolve+=(sender,eventArgs)=>
        {
            string local=Path.Combine(core,new AssemblyName(eventArgs.Name).Name+".dll");
            return File.Exists(local)?Assembly.LoadFrom(local):null;
        };
        Assembly assembly=Assembly.LoadFrom(path);
        patches=assembly.GetType("LethalPhonesCallTimeout.CallTimeoutPatches",true);
        settingsType=assembly.GetType("LethalPhonesCallTimeout.TimeoutSettings",true);
        modeType=assembly.GetType("LethalPhonesCallTimeout.TimeoutMode",true);
        Type runtime=assembly.GetType("LethalPhonesCallTimeout.LethalPhonesRuntime",true);
        runtime.GetMethod("Initialize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        Type harmonyType=Assembly.LoadFrom(Path.Combine(core,"0Harmony.dll")).GetType("HarmonyLib.Harmony",true);
        object harmony=Activator.CreateInstance(harmonyType,new object[]{"lethalphones.calltimeout.runtime.tests"});
        try
        {
            patches.GetMethod("Install",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{harmony});
            var caller=new Scoops.misc.PlayerPhone{phoneNumber=1,phoneRingtoneId="caller"};
            var receiver=new Scoops.misc.PlayerPhone{phoneNumber=2,phoneRingtoneId="receiver"};
            var third=new Scoops.misc.PlayerPhone{phoneNumber=3,phoneRingtoneId="third"};
            var board=new Scoops.gameobjects.SwitchboardPhone{phoneNumber=1111};
            var masked=new Scoops.misc.MaskedPhone{phoneNumber=4};
            var list=new List<Scoops.misc.PhoneBehavior>{caller,receiver,third,board,masked};
            Scoops.service.PhoneNetworkHandler.allPhoneBehaviors=list;
            var clips=Scoops.customization.CustomizationManager.ringtoneCustomizations;
            clips["caller"]=Clip("caller",7f);clips["receiver"]=Clip("receiver",40f);
            clips["third"]=Clip("third",17.25f);clips["default"]=Clip("default",3.5f);
            Scoops.service.PhoneAssetManager.switchboardRing=Clip("switchboard",6.75f);
            Settings("Fixed",45f);
            Wait(45f,caller.Call(2),"fixed player");Wait(45f,board.Call(2),"fixed switchboard");
            Settings("Fixed",90f);Wait(90f,caller.Call(2),"fixed above previous cap");
            Noise(23,caller,"long player noise");Noise(23,board,"long switchboard noise");Noise(4,masked,"enemy noise unchanged");
            Settings("RingtoneLength",45f);
            Wait(40f,caller.Call(2),"receiving ringtone, not caller");
            Wait(7f,receiver.Call(1),"opposite direction");
            Wait(17.25f,caller.Call(3),"fractional clip length");
            Wait(40f,board.Call(2),"switchboard caller");
            Wait(6.75f,caller.Call(1111),"switchboard receiver");
            Noise(10,receiver,"ringtone-based noise");
            Wait(45f,caller.Call(999),"unknown phone fallback");
            receiver.phoneRingtoneId="missing";Wait(3.5f,caller.Call(2),"default ringtone fallback");
            clips.Remove("default");Wait(45f,caller.Call(2),"missing default falls back fixed");
            clips["default"]=Clip("default-restored",3.5f);receiver.phoneRingtoneId="receiver";
            clips["destroyed"]=Clip("destroyed",12f);UnityEngine.Object.DestroyImmediate(clips["destroyed"]);
            receiver.phoneRingtoneId="destroyed";Wait(3.5f,caller.Call(2),"destroyed clip uses default");receiver.phoneRingtoneId="receiver";
            Scoops.service.PhoneNetworkHandler.allPhoneBehaviors=null;
            Wait(45f,caller.Call(2),"missing registry fallback");
            Scoops.service.PhoneNetworkHandler.allPhoneBehaviors=list;
            GameObject sceneObject=new GameObject("Isolated phone lookup test");
            try
            {
                ScenePhone scenePhone=sceneObject.AddComponent<ScenePhone>();scenePhone.phoneNumber=55;scenePhone.phoneRingtoneId="third";
                runtime.GetField("behaviorType",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,typeof(ScenePhone));
                runtime.GetField("number",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,typeof(ScenePhone).GetField("phoneNumber"));
                runtime.GetField("ringtoneId",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,typeof(ScenePhone).GetField("phoneRingtoneId"));
                Scoops.service.PhoneNetworkHandler.allPhoneBehaviors=null;
                Wait(17.25f,caller.Call(55),"live scene phone when contact list missing");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sceneObject);
                runtime.GetMethod("Initialize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
                Scoops.service.PhoneNetworkHandler.allPhoneBehaviors=list;
            }
            Settings("RingtoneLength",45f,20f);Wait(20f,caller.Call(2),"configured cap");
            Settings("RingtoneLength",45f,300f,.5f);Wait(17.75f,caller.Call(3),"configured grace");
            Settings("Fixed",45f);Wait(14f,masked.Call(2),"enemy timer unchanged");
            IEnumerator completed=caller.Call(2);completed.MoveNext();int before=caller.Completed;
            if(completed.MoveNext() || caller.Completed!=before+1)throw new Exception("Completion altered");checks++;
            IEnumerator accepted=caller.Call(2);accepted.MoveNext();before=caller.Completed;caller.Accept();
            if(accepted.MoveNext() || caller.Completed!=before)throw new Exception("Accepted call later timed out");checks++;
            IEnumerator cancelled=caller.Call(2);cancelled.MoveNext();before=caller.Completed;int disposed=caller.Disposed;
            ((IDisposable)cancelled).Dispose();
            if(caller.Completed!=before || caller.Disposed!=disposed+1)throw new Exception("Cancellation altered");checks++;
            Debug.Log("RUNTIME_TESTS_OK "+checks+" (real Harmony; receiving clip lengths, fallbacks, noise, cancellation)");
        }
        finally
        {
            harmonyType.GetMethod("UnpatchSelf").Invoke(harmony,null);
            foreach(AudioClip clip in created)UnityEngine.Object.DestroyImmediate(clip);
        }
    }
    private static AudioClip Clip(string name,float seconds)
    {
        AudioClip clip=AudioClip.Create(name,Mathf.RoundToInt(seconds*8000),1,8000,false);created.Add(clip);return clip;
    }
    private static void Settings(string mode,float fixedSeconds,float maximum=300f,float grace=0f)
    {
        object value=Activator.CreateInstance(settingsType,true);
        foreach(var entry in new Dictionary<string,object>{{"Mode",Enum.Parse(modeType,mode)},{"FixedSeconds",fixedSeconds},{"MaximumRingtoneSeconds",maximum},{"RingtoneGraceSeconds",grace}})
            settingsType.GetField(entry.Key,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(value,entry.Value);
        Type function=typeof(Func<>).MakeGenericType(settingsType);
        Delegate provider=Expression.Lambda(function,Expression.Constant(value,settingsType)).Compile();
        patches.GetField("SettingsProvider",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,provider);
    }
    private static void Wait(float expected,IEnumerator call,string name)
    {
        if(!call.MoveNext())throw new Exception(name+": no wait");
        float value=(float)typeof(WaitForSeconds).GetField("m_Seconds",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(call.Current);
        if(Mathf.Abs(expected-value)>.001f)throw new Exception(name+": expected "+expected+", got "+value);
        ((IDisposable)call).Dispose();checks++;
    }
    private static void Noise(int expected,Scoops.misc.PhoneBehavior phone,string name)
    {
        IEnumerator noise=phone.NoiseTest();noise.MoveNext();
        if((int)noise.Current!=expected)throw new Exception(name+": got "+noise.Current);
        ((IDisposable)noise).Dispose();checks++;
    }
}
public class ScenePhone:MonoBehaviour{public short phoneNumber;public string phoneRingtoneId;}
namespace Scoops.misc
{
    public class PhoneBehavior
    {
        public short phoneNumber;public string phoneRingtoneId;
        protected virtual IEnumerator PhoneRingCoroutine(int repeats){yield return repeats;GC.KeepAlive(this);}
        public IEnumerator NoiseTest(){return PhoneRingCoroutine(4);}
    }
    public class PlayerPhone:PhoneBehavior
    {
        public int Completed,Disposed;private short outgoing;
        private IEnumerator CallTimeoutCoroutine(short number)
        {
            try{yield return new WaitForSeconds(15f);if(outgoing==number)Completed++;}
            finally{Disposed++;}
        }
        public IEnumerator Call(short number){outgoing=number;return CallTimeoutCoroutine(number);}
        public void Accept(){outgoing=-1;}
        protected override IEnumerator PhoneRingCoroutine(int repeats){yield return repeats;GC.KeepAlive(this);}
    }
    public class MaskedPhone:PhoneBehavior
    {
        private IEnumerator CallTimeoutCoroutine(short number){yield return new WaitForSeconds(14f);GC.KeepAlive(number);}
        public IEnumerator Call(short number){return CallTimeoutCoroutine(number);}
    }
}
namespace Scoops.gameobjects
{
    public class SwitchboardPhone:Scoops.misc.PhoneBehavior
    {
        private IEnumerator CallTimeoutCoroutine(short number){yield return new WaitForSeconds(15f);GC.KeepAlive(this);GC.KeepAlive(number);}
        public IEnumerator Call(short number){return CallTimeoutCoroutine(number);}
    }
}
namespace Scoops.service
{
    public static class PhoneNetworkHandler{public static List<Scoops.misc.PhoneBehavior> allPhoneBehaviors;}
    public static class PhoneAssetManager{public static AudioClip switchboardRing;}
}
namespace Scoops.customization
{
    public static class CustomizationManager
    {
        public const string DEFAULT_RINGTONE="default";
        public static Dictionary<string,AudioClip> ringtoneCustomizations=new Dictionary<string,AudioClip>();
    }
}
