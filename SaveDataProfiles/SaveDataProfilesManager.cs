using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveDataProfiles
{
    public class SaveDataProfilesManager : MonoBehaviour
    {
        public static SaveDataProfilesManager instance { get; private set; }

        public string currentProfileSavePath => GetProfileSavePath(currentProfile);

        public string currentProfile { get; private set; } = string.Empty;


        string _saveDirectoryPath => $"{Application.dataPath}/Saves";

        string _lastProfileFilePath => $"{_saveDirectoryPath}/profile.txt";

        List<string> _profiles = new List<string>();

        string _newProfileName = string.Empty;
        Vector2 _profileListScroll;
        ProfileMenuMode _menuMode;
        string _selectedProfile;


        public IEnumerable<string> GetCustomProfiles() => _profiles.AsReadOnly();

        public bool ProfileExists(string profile)
        {
            return string.IsNullOrEmpty(profile) || _profiles.Contains(profile);
        }

        public string GetProfileSavePath(string profile)
        {
            if (string.IsNullOrEmpty(profile)) return Application.persistentDataPath;
            return $"{_saveDirectoryPath}/{profile}";
        }

        public bool CreateProfile(string newProfile)
        {
            if (ProfileExists(newProfile)) return false;
            _profiles.Add(newProfile);
            return true;
        }

        public bool DeleteProfile(string profile)
        {
            if (string.IsNullOrEmpty(profile)) return false;
            if (!ProfileExists(profile)) return false;
            _profiles.Remove(profile);
            Directory.Delete(GetProfileSavePath(profile));
            return true;
        }

        public bool CopyProfile(string oldProfile, string newProfile)
        {
            if (!ProfileExists(oldProfile)) return false;
            if (ProfileExists(newProfile)) return false;
            _profiles.Add(newProfile);
            Directory.CreateDirectory(GetProfileSavePath(newProfile));
            CopyDirectory(GetProfileSavePath(oldProfile), GetProfileSavePath(newProfile));
            return true;
        }

        public bool SwitchProfiles(string newProfile)
        {
            if (newProfile == currentProfile) return false;

            GlobalGameManager.instance.ReleaseGame();

            currentProfile = newProfile;
            File.WriteAllText(_lastProfileFilePath, currentProfile);

            GlobalGameManager.instance.SetField("saveFileName", $"{currentProfileSavePath}/saveData170808.dat");
            GlobalGameManager.instance.SetField("saveGlobalFileName", $"{currentProfileSavePath}/saveGlobal170808.dat");
            GlobalGameManager.instance.SetField("saveUnlimitFileName", $"{currentProfileSavePath}/saveUnlimitV5170808.dat");
            GlobalGameManager.instance.SetField("saveEtcFileName", $"{currentProfileSavePath}/etc170808.dat");

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
            GameStaticDataLoader.ReloadData();
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
                _profiles.Add(dirName);
            }

            if (File.Exists(_lastProfileFilePath))
            {
                var lastProfile = File.ReadAllText(_lastProfileFilePath);
                SwitchProfiles(lastProfile);
            }
        }

        void OnGUI()
        {
            if (NewTitleScript.instance == null && AlterTitleController.Controller == null) return;
            if (CreatureInfoWindow.CurrentWindow != null && CreatureInfoWindow.CurrentWindow.IsEnabled) return;
            if (OptionUI.Instance != null && OptionUI.Instance.IsEnabled) return;
            if (GlobalGameManager.instance.loadingScreen.isLoading) return;
            if (NewTitleScript.instance != null && NewTitleScript.instance.GetField<bool>("isNewGame")) return;

            var leftSide = AlterTitleController.Controller != null;

            GUILayout.BeginArea(new Rect(leftSide ? 0f : Screen.width / 2f, 0f, Screen.width / 2f, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical("Save Profiles", GUI.skin.window, GUILayout.MinWidth(200f));
            if (_menuMode != ProfileMenuMode.Main)
            {
                GUILayout.Label($"{_menuMode} Profile");
            }
            if (_menuMode != ProfileMenuMode.Main && _menuMode != ProfileMenuMode.Create)
            {
                _profileListScroll = GUILayout.BeginScrollView(_profileListScroll);
                if (_menuMode != ProfileMenuMode.Delete) {
                    GUI.enabled = _selectedProfile != string.Empty;
                    if (GUILayout.Button("Default Profile" + (currentProfile == string.Empty ? " (current)" : "")))
                    {
                        _selectedProfile = string.Empty;
                    }
                }
                foreach (var profile in _profiles)
                {
                    GUI.enabled = _selectedProfile != profile;
                    if (GUILayout.Button(profile + (currentProfile == profile ? " (current)" : "")))
                    {
                        _selectedProfile = profile;
                    }
                }
                GUI.enabled = true;
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
            }
            switch (_menuMode)
            {
                case ProfileMenuMode.Main:
                    GUILayout.Label("Current Profile:");
                    GUILayout.Label(currentProfile == string.Empty ? "Default Profile" :  currentProfile);
                    if (GUILayout.Button("Create"))
                    {
                        _menuMode = ProfileMenuMode.Create;
                        _newProfileName = string.Empty;
                    }
                    if (GUILayout.Button("Load"))
                    {
                        _menuMode = ProfileMenuMode.Load;
                        _selectedProfile = currentProfile;
                    }
                    if (GUILayout.Button("Copy"))
                    {
                        _menuMode = ProfileMenuMode.Copy;
                        _selectedProfile = currentProfile;
                        _newProfileName = string.Empty;
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        _menuMode = ProfileMenuMode.Delete;
                        _selectedProfile = currentProfile;
                    }
                    break;
                case ProfileMenuMode.Load:
                    GUI.enabled = _selectedProfile != currentProfile;
                    if (GUILayout.Button("Load Profile"))
                    {
                        if (SwitchProfiles(_selectedProfile))
                        {
                            _menuMode = ProfileMenuMode.Main;
                        }
                    }
                    break;
                case ProfileMenuMode.Copy:
                    GUILayout.Label($"New Profile Name:");
                    _newProfileName = GUILayout.TextField(_newProfileName);
                    GUI.enabled = _newProfileName.Length > 0 && !_profiles.Contains(_newProfileName);
                    if (GUILayout.Button("Copy Profile"))
                    {
                        if (CopyProfile(_selectedProfile, _newProfileName))
                        {
                            _menuMode = ProfileMenuMode.Main;
                        }
                    }
                    break;
                case ProfileMenuMode.Delete:
                    GUI.enabled = _selectedProfile != currentProfile;
                    if (GUILayout.Button("Delete Profile"))
                    {
                        if (DeleteProfile(_selectedProfile))
                        {
                            _menuMode = ProfileMenuMode.Main;
                        }
                    }
                    break;
                case ProfileMenuMode.Create:
                    GUILayout.Label($"New Profile Name:");
                    _newProfileName = GUILayout.TextField(_newProfileName);
                    GUI.enabled = _newProfileName.Length > 0 && !_profiles.Contains(_newProfileName);
                    if (GUILayout.Button("Create Profile"))
                    {
                        if (CreateProfile(_newProfileName))
                        {
                            _menuMode = ProfileMenuMode.Main;
                        }
                    }
                    break;
            }
            GUI.enabled = true;
            if (_menuMode != ProfileMenuMode.Main)
            {
                if (GUILayout.Button("Cancel"))
                {
                    _menuMode = ProfileMenuMode.Main;
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void CopyDirectory(string srcPath, string dstPath)
        {
            CopyDirectory(new DirectoryInfo(srcPath), new DirectoryInfo(dstPath));
        }

        void CopyDirectory(DirectoryInfo src, DirectoryInfo dst)
        {
            foreach (var subDir in src.GetDirectories())
            {
                CopyDirectory(subDir, dst.CreateSubdirectory(subDir.Name));
            }
            foreach (var file in src.GetFiles())
            {
                var dstFilePath = Path.Combine(dst.FullName, file.Name);
                file.CopyTo(dstFilePath, true);
            }
        }

        enum ProfileMenuMode
        {
            Main = 0,
            Load = 1,
            Copy = 2,
            Delete = 3,
            Create = 4,
        }
    }
}
