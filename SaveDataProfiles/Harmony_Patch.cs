using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WhiteNightSpace;

namespace SaveDataProfiles
{
    public class Harmony_Patch
    {
        public static string GetCurrentSavePath()
        {
            return SaveDataProfilesManager.instance.currentSavePath;
        }

        public Harmony_Patch()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create($"Lobotomy.{nameof(SaveDataProfiles)}");
                Prefix(harmony, typeof(GlobalGameManager), "Awake", nameof(GlobalGameManager_Awake));
                Prefix(harmony, typeof(GlobalGameManager), "GetLogSrc", nameof(GlobalGameManager_GetLogSrc));
                Prefix(harmony, typeof(GlobalGameManager), "get_stateSrc", nameof(GlobalGameManager_get_stateSrc));
                Prefix(harmony, typeof(GlobalGameManager), "LoadStateData", nameof(GlobalGameManager_LoadStateData));
                Prefix(harmony, typeof(GlobalGameManager), "SaveLogs", nameof(GlobalGameManager_SaveLogs));
                Prefix(harmony, typeof(GlobalGameManager), "SaveStateData", nameof(GlobalGameManager_SaveStateData));
                Prefix(harmony, typeof(AgentManager), "get_AgentDataSrc", nameof(AgentManager_get_AgentDataSrc));
                Prefix(harmony, typeof(AgentManager), "get_CustomAgentData", nameof(AgentManager_get_CustomAgentData));
                Prefix(harmony, typeof(AgentManager), "get_DeletedAgentData", nameof(AgentManager_get_DeletedAgentData));
                Prefix(harmony, typeof(CreatureBase), "get_GetSaveSrc", nameof(CreatureBase_get_GetSaveSrc));
                Prefix(harmony, typeof(CreatureBase), "LoadScriptData", nameof(CreatureBase_LoadScriptData));
                Prefix(harmony, typeof(CreatureBase), "SaveScriptData", nameof(CreatureBase_SaveScriptData));
                Prefix(harmony, typeof(CreatureManager), "RemoveSriptSaveData", nameof(CreatureManager_RemoveSriptSaveData));
                Prefix(harmony, typeof(StoryMemoryManager), "get_saveFileName", nameof(StoryMemoryManager_get_saveFileName));
                Prefix(harmony, typeof(DeathAngel), "ActivateQliphothCounter", nameof(DeathAngel_ActivateQliphothCounter));
                Prefix(harmony, typeof(PlagueDoctor), "CheckAdvent", nameof(PlagueDoctor_CheckAdvent));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void GlobalGameManager_Awake()
        {
            if (SaveDataProfilesManager.instance == null)
            {
                var managerObj = new GameObject(nameof(SaveDataProfilesManager));
                managerObj.AddComponent<SaveDataProfilesManager>();
            }
        }

        public static bool GlobalGameManager_GetLogSrc(GlobalGameManager __instance, ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/Log/170808_{__instance.GetField<int>("logCount")}.txt";
            return false;
        }

        public static bool GlobalGameManager_get_stateSrc(ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/170808state.dat";
            return false;
        }

        public static void GlobalGameManager_LoadStateData()
        {
            Directory.CreateDirectory(GetCurrentSavePath());
        }

        public static void GlobalGameManager_SaveLogs()
        {
            Directory.CreateDirectory($"{GetCurrentSavePath()}/Log/");
        }

        public static void GlobalGameManager_SaveStateData()
        {
            Directory.CreateDirectory(GetCurrentSavePath());
        }

        public static bool AgentManager_get_AgentDataSrc(ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/agentData";
            return false;
        }

        public static bool AgentManager_get_CustomAgentData(ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/agentData/170808custom.dat";
            return false;
        }

        public static bool AgentManager_get_DeletedAgentData(ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/agentData/170808.dat";
            return false;
        }

        public static bool CreatureBase_get_GetSaveSrc(CreatureBase __instance, ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/creatureData/{CreatureTypeList.instance.GetModId(__instance.model.metaInfo)}{__instance.model.metadataId}.dat";
            return false;
        }

        public static void CreatureBase_LoadScriptData()
        {
            Directory.CreateDirectory($"{GetCurrentSavePath()}/creatureData");
        }

        public static void CreatureBase_SaveScriptData()
        {
            Directory.CreateDirectory($"{GetCurrentSavePath()}/creatureData");
        }

        public static bool CreatureManager_RemoveSriptSaveData()
        {
            string path = $"{GetCurrentSavePath()}/creatureData";
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }
            return false;
        }

        public static bool StoryMemoryManager_get_saveFileName(ref string __result)
        {
            __result = $"{GetCurrentSavePath()}/saveStory.dat";
            return false;
        }

        public static bool DeathAngel_ActivateQliphothCounter(DeathAngel __instance)
        {
            PlaySpeedSettingUI.instance.SetNormalSpeedForcely();
            __instance.AnimScript.AdventClockUI.SetAdventEffectEndEvent(new AdventClockUI.EndEvent(__instance.Escape));
            bool flag = __instance.GetField<List<ApostleData>>("apostleData").Count == 0;
            if (flag)
            {
                string path = string.Concat(new object[]
                {
                    GetCurrentSavePath(),
                    "/creatureData/",
                    "100014",
                    ".dat"
                });
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = File.Open(path, FileMode.Open);
                Dictionary<string, object> dic = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
                __instance.LoadData(dic);
            }
            bool flag2 = __instance.GetField<List<DeathAngelApostle>>("apostles").Count == 0;
            List<ApostleGenData> adeventTargets;
            if (flag2)
            {
                adeventTargets = DeathAngel.GetAdeventTargets(__instance.GetField<List<ApostleData>>("apostleData"));
            }
            else
            {
                adeventTargets = __instance.GetField<List<ApostleGenData>>("genDataSave");
            }
            __instance.GenApostle(adeventTargets);
            __instance.AnimScript.AdventClockUI.StartSimpleAdventEvent();
            __instance.AnimScript.AdventClockUI.SimpleAdventStart(adeventTargets);
            return false;
        }

        public static bool PlagueDoctor_CheckAdvent(ref bool __result)
        {
            string text = GetCurrentSavePath() + "/creatureData/100014.dat";
            string name = "apostleListCount";
            DirectoryInfo directoryInfo = new DirectoryInfo(GetCurrentSavePath() + "/creatureData");
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            if (!File.Exists(text))
            {
                Debug.Log(text + " doesn't exist");
                __result = false;
                return false;
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(text, FileMode.Open);
            Dictionary<string, object> dic = (Dictionary<string, object>)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
            int num = -1;
            __result = GameUtil.TryGetValue<int>(dic, name, ref num) && num == 12;
            return false;
        }

        static void Prefix(HarmonyInstance harmony, Type srcType, string srcMethodName, string dstMethodName, Type[] overload = null)
        {
            var src = GetPatchMethod(srcType, srcMethodName, overload);
            var dst = new HarmonyMethod(typeof(Harmony_Patch).GetMethod(dstMethodName));
            harmony.Patch(src, dst, null);
        }

        static void Postfix(HarmonyInstance harmony, Type srcType, string srcMethodName, string dstMethodName, Type[] overload = null)
        {
            var src = GetPatchMethod(srcType, srcMethodName, overload);
            var dst = new HarmonyMethod(typeof(Harmony_Patch).GetMethod(dstMethodName));
            harmony.Patch(src, null, dst);
        }

        static MethodInfo GetPatchMethod(Type srcType, string srcMethodName, Type[] overload = null)
        {
            var methods = srcType.GetMethods(AccessTools.all);
            MethodInfo result = null;
            foreach (var method in methods)
            {
                if (method.Name != srcMethodName) continue;
                if (overload != null)
                {
                    var parameters = method.GetParameters();
                    if (overload.Length != parameters.Length) continue;
                    var parametersMatch = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (overload[i] != parameters[i].ParameterType)
                        {
                            parametersMatch = false;
                            break;
                        }
                    }
                    if (!parametersMatch) continue;
                }
                if (result != null)
                {
                    Debug.LogError($"Ambiguous method patch {srcType.Name}.{srcMethodName}, defaulting to first match");
                    break;
                }
                result = method;
            }
            if (result == null)
            {
                Debug.LogError($"No match found for patch method {srcType.Name}.{srcMethodName}");

            }
            return result;
        }
    }

    public static class Reflection
    {
        public static bool HasStaticField<T>(string fieldName)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return field != null;
        }

        public static U GetStaticField<T, U>(string fieldName)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var value = field.GetValue(null);
            return (U)value;
        }

        public static U SetStaticField<T, U>(string fieldName, U value)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
            return value;
        }

        public static bool HasField(this object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null;
        }

        public static T GetField<T>(this object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field.GetValue(obj);
            return (T)value;
        }

        public static T SetField<T>(this object obj, string fieldName, T value)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
            return value;
        }
    }
}
