using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveDataProfiles
{
    public class SaveDataProfilesManager : MonoBehaviour
    {
        public static SaveDataProfilesManager instance { get; private set; }

        public string currentSavePath =>
            string.IsNullOrEmpty(currentProfile)
            ? Application.persistentDataPath
            : $"{_saveDirectoryPath}/{currentProfile}";

        public string currentProfile { get; private set; } = string.Empty;

        string _saveDirectoryPath => $"{Application.dataPath}/Saves";

        string _lastProfileFilePath => $"{_saveDirectoryPath}/profile.txt";

        public List<string> profiles { get; private set; } = new List<string>();

        public bool CreateProfile(string newProfile)
        {
            if (profiles.Contains(newProfile)) return false;
            profiles.Add(newProfile);
            SwitchProfiles(newProfile);
            return true;
        }

        public bool DeleteProfile(string profile)
        {
            if (!profiles.Contains(profile)) return false;
            if (currentProfile == profile)
            {
                SwitchProfiles(string.Empty);
            }
            profiles.Remove(profile);
            Directory.Delete($"{_saveDirectoryPath}/{profile}");
            return true;
        }

        public bool SwitchProfiles(string newProfile)
        {
            if (newProfile == currentProfile) return false;

            GlobalGameManager.instance.ReleaseGame();

            currentProfile = newProfile;
            File.WriteAllText(_lastProfileFilePath, currentProfile);

            GlobalGameManager.instance.SetField("saveFileName", $"{currentSavePath}/saveData170808.dat");
            GlobalGameManager.instance.SetField("saveGlobalFileName", $"{currentSavePath}/saveGlobal170808.dat");
            GlobalGameManager.instance.SetField("saveUnlimitFileName", $"{currentSavePath}/saveUnlimitV5170808.dat");
            GlobalGameManager.instance.SetField("saveEtcFileName", $"{currentSavePath}/etc170808.dat");

            GlobalGameManager.instance.LoadStateData();
            AudioListener.volume = GlobalGameManager.instance.sceneDataSaver.currentVolume;
            string a = GlobalGameManager.instance.language;
            switch (a)
            {
                case "vn":
                    GlobalGameManager.instance.Language = SystemLanguage.Vietnamese;
                    break;
                case "bg":
                    GlobalGameManager.instance.Language = SystemLanguage.Bulgarian;
                    break;
                case "ru":
                    GlobalGameManager.instance.Language = SystemLanguage.Russian;
                    break;
                case "es":
                    GlobalGameManager.instance.Language = SystemLanguage.Spanish;
                    break;
                case "jp":
                    GlobalGameManager.instance.Language = SystemLanguage.Japanese;
                    break;
                case "kr":
                    GlobalGameManager.instance.Language = SystemLanguage.Korean;
                    break;
                case "cn_tr":
                    GlobalGameManager.instance.Language = SystemLanguage.ChineseTraditional;
                    break;
                case "cn":
                    GlobalGameManager.instance.Language = SystemLanguage.Chinese;
                    break;
                default:
                    GlobalGameManager.instance.Language = SystemLanguage.English;
                    break;
            }
            GlobalGameManager.instance.SetLanguageFont();
            GlobalGameManager.instance.LoadGlobalData();
            CreatureGenerate.CreatureGenerateInfoManager.Instance.Init();
            PlayerModel.instance.InitAddingCreatures();

            GlobalGameManager.instance.loadingScene = "DefaultEndScene";
            GlobalGameManager.instance.loadingScreen.LoadTitleScene();

            return true;
        }

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            Directory.CreateDirectory(_saveDirectoryPath);

            foreach (var fullPath in Directory.GetDirectories(_saveDirectoryPath))
            {
                var dirName = Path.GetFileName(fullPath);
                profiles.Add(dirName);
            }

            if (File.Exists(_lastProfileFilePath))
            {
                var lastProfile = File.ReadAllText(_lastProfileFilePath);
                SwitchProfiles(lastProfile);
            }
        }

        string _newProfileName = string.Empty;
        Vector2 _profileListScroll;

        void OnGUI()
        {
            if (NewTitleScript.instance == null) return;
            if (CreatureInfoWindow.CurrentWindow != null && CreatureInfoWindow.CurrentWindow.IsEnabled) return;
            if (OptionUI.Instance != null && OptionUI.Instance.IsEnabled) return;
            if (GlobalGameManager.instance.loadingScreen.isLoading) return;
            if (NewTitleScript.instance.GetField<bool>("isNewGame")) return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f, 0f, Screen.width / 2f, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical("Save Profiles", GUI.skin.window, GUILayout.MinWidth(200f), GUILayout.MinHeight(300f));
            _profileListScroll = GUILayout.BeginScrollView(_profileListScroll);
            {
                GUI.enabled = currentProfile != string.Empty;
                if (GUILayout.Button("Default Profile"))
                {
                    SwitchProfiles(string.Empty);
                }
                GUI.enabled = true;
            }
            foreach (var profile in profiles)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = profile != currentProfile;
                if (GUILayout.Button(profile))
                {
                    SwitchProfiles(profile);
                }
                if (GUILayout.Button("X", GUILayout.Width(25f)))
                {
                    DeleteProfile(profile);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            _newProfileName = GUILayout.TextField(_newProfileName);
            GUI.enabled = _newProfileName.Length > 0 && !profiles.Contains(_newProfileName);
            if (GUILayout.Button("New", GUILayout.Width(50f)))
            {
                var newProfileName = _newProfileName;
                _newProfileName = string.Empty;
                CreateProfile(newProfileName);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
