using UnityEngine;
using BepInEx;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine.Networking;

namespace MonkeMonkeMusic
{
    [BepInPlugin("com.sev.gorillatag.MonkeMonkeMusic", "Monke Monke Music", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal GameObject PlayButton;
        internal GameObject PauseButton;
        internal GameObject NextSong;
        internal GameObject PreviousSong;

        void Awake()
        {
            AssetLoader.LoadAssets();
            GorillaTagger.OnPlayerSpawned(() => CustomStart());
            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] Error loading sound: " + e.Message);} }

        void CustomStart()
        {
            GameObject prefab;
            if (AssetLoader.TryGetAsset<GameObject>("MMMusic", out prefab))
            {
                GameObject go = Instantiate(prefab);

                foreach (Transform c in go.transform)
                {
                    if (c.name == "Play")
                    {
                        PlayButton = c.gameObject;
                        c.AddComponent<Scripts.Button>().Click += (b) => TogglePlay();
                    }
                    else if (c.name == "Pause")
                    {
                        PauseButton = c.gameObject;
                        c.AddComponent<Scripts.Button>().Click += (b) => TogglePlay();
                    }
                    else if (c.name == "Forward")
                    {
                        NextSong = c.gameObject;
                        c.AddComponent<Scripts.Button>().Click += (b) => Next();
                    }
                    else if (c.name == "Back")
                    {
                        PreviousSong = c.gameObject;
                        c.AddComponent<Scripts.Button>().Click += (b) => Previous();
                    }
                }

                go.transform.position = new Vector3(-68.9884f, 11.9349f, -84.2524f);
                go.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                go.transform.rotation = Quaternion.Euler(0f, 149.8f, 5f);
            }
        }

        internal Dictionary<string, AudioClip> LoadedSounds = new Dictionary<string, AudioClip>();

        internal void PlaySound(string Url, float Volume = 0.4f)
        {
            try
            {
                AudioClip Clip = GetLoadedSound(Url);
                GameObject soundObject = new GameObject("Sev Essence Sound Player");
                AudioSource Player = soundObject.AddComponent<AudioSource>();
                Player.clip = Clip;
                Player.volume = Volume;
                Player.Play();
                GameObject.Destroy(soundObject, Player.clip.length);
            }
            catch { }
        }

        internal AudioClip GetLoadedSound(string fileLink)
        {
            try
            {
                if (string.IsNullOrEmpty(fileLink)) return null;
                if (LoadedSounds.TryGetValue(fileLink, out AudioClip clip)) return clip;
            }
            catch { }
            return null;
        }

        internal IEnumerator LoadWav(string fileLink)
        {
            if (string.IsNullOrEmpty(fileLink) || !fileLink.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) yield break;

            if (LoadedSounds.ContainsKey(fileLink)) yield break;

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Several Bees", "Resources", "Sounds");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            string fileName = Path.GetFileName(fileLink);
            string fullPath = Path.Combine(basePath, fileName);

            using (UnityWebRequest www = UnityWebRequest.Get(fileLink))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    yield break;

                File.WriteAllBytes(fullPath, www.downloadHandler.data);
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + fullPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    yield break;

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                LoadedSounds[fileLink] = clip;
            }
        }

        public void TogglePlay()
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            byte keyCode = 179;
            keybd_event(keyCode, 0, 1u, 0u);
            keybd_event(keyCode, 0, 3u, 0u);
        }

        public void Next()
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            byte keyCode = 176;
            keybd_event(keyCode, 0, 1u, 0u);
            keybd_event(keyCode, 0, 3u, 0u);
        }

        public void Previous()
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            byte keyCode = 177;
            keybd_event(keyCode, 0, 1u, 0u);
            keybd_event(keyCode, 0, 3u, 0u);
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
    }

    internal static class AssetLoader
    {
        // https://github.com/sevvy-wevvy/Several-Bees/blob/main/Several%20Bees/Extra.cs#L137

        private static readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        public static bool BundleLoaded => _assets.Count > 0;
        public static bool TryGetAsset<T>(string name, out T obj) where T : UnityEngine.Object
        {
            if (BundleLoaded && _assets.FirstOrDefault(asset => asset.name == name) is T prefab)
            {
                obj = prefab;
                return true;
            }

            obj = null!;
            return false;
        }
        public static void LoadAssets()
        {
            try
            {
                if (BundleLoaded) throw new Exception("Assets already loaded.");
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream? stream = assembly.GetManifestResourceStream($"MonkeMonkeMusic.mmmusic");
                AssetBundle bundle = AssetBundle.LoadFromStream(stream ?? throw new Exception("Failed to get stream."));

                UnityEngine.Debug.Log($"Retrieved bundle: {(bundle ?? throw new Exception("Failed to get bundle.")).name}");
                foreach (var asset in bundle.LoadAllAssets())
                {
                    _assets.AddIfNew(asset);
                    UnityEngine.Debug.Log($"Loaded asset: {asset.name} ({asset.GetType().FullName})");
                }

                stream.Close();
                UnityEngine.Debug.Log($"Loaded {_assets.Count} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }
    }
}
