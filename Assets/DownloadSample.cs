using System;
using System.Collections.Generic;
using System.IO;
using Unity.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace BackgroundDownloadSample
{
    public class DownloadSample : MonoBehaviour
    {
        private static string[] DownloadUrls = new[]
        {
            "https://dldir1.qq.com/weixin/Windows/WeChatSetup.exe",
            "https://dldir1.qq.com/weixin/android/weixin8018android2060_arm64.apk",
            "https://dldir1.qq.com/weixin/mac/WeChatMac.dmg"
        };
        
        public string DestPath;
        public BackgroundDownloadManager manager;
        public Slider slider;
        public Button startButton;
        public Button clearButton;

        private void Start()
        {
            if (manager != null)
            {
                manager.StateChanged += OnStateChanged;
                UpdateButtonState(manager.CurrentState);
            }
            if (startButton != null)
            {
                startButton.onClick.AddListener(StartDownload);
            }
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearDownload);
            }
        }
        
        private string AbsoluteDestPath => Path.Combine(Application.persistentDataPath, DestPath);

        private void StartDownload()
        {
            if (manager == null) return;
            
            Directory.CreateDirectory(AbsoluteDestPath);
            manager.StartDownload(DestPath, new List<string>(DownloadUrls));
        }
        
        private void ClearDownload()
        {
            if (manager == null) return;
            
            manager.ClearAll();
        }

        private void OnStateChanged(BackgroundDownloadManager.State state)
        {
            UpdateButtonState(state);

            switch (state)
            {
                case BackgroundDownloadManager.State.Finished:
                    Debug.Log($"Finished download, dest path: {AbsoluteDestPath}");
                    break;
            }
        }

        private void UpdateButtonState(BackgroundDownloadManager.State state)
        {
            if (startButton != null)
            {
                startButton.interactable = state == BackgroundDownloadManager.State.Idle;
            }

            if (clearButton != null)
            {
                clearButton.interactable = state == BackgroundDownloadManager.State.Finished;
            }
        }

        private void Update()
        {
            if (manager == null || slider == null) return;

            slider.value = manager.TotalProgress;
        }
    }
}