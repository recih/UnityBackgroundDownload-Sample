using System;
using System.Collections.Generic;
using System.IO;
using Unity.Networking;
using UnityEngine;

namespace BackgroundDownloadSample
{
    public class BackgroundDownloadManager : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Downloading,
            Finished,
        }
        
        private BackgroundDownload[] _downloads;
        
        public float updateInterval = 0.5f;
        
        public float TotalProgress { get; set; }

        private State _state;
        public State CurrentState
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged(_state);
                    StateChanged?.Invoke(_state);
                }
            }
        }

        public event Action<State> StateChanged;

        private void Start()
        {
            TotalProgress = 0;
            _timer = 0;
            _downloads = BackgroundDownload.backgroundDownloads;
            CurrentState = _downloads.Length > 0 ? State.Downloading : State.Idle;
        }

        private void OnStateChanged(State state)
        {
            switch (state)
            {
                case State.Idle:
                    TotalProgress = 0;
                    _timer = 0;
                    break;
                case State.Downloading:
                    Debug.Log($"Start downloading {_downloads.Length} files.");
                    break;
                case State.Finished:
                    var successCount = 0;
                    BackgroundDownload firstErrorDownload = null; 
                    foreach (var download in _downloads)
                    {
                        if (download.status == BackgroundDownloadStatus.Done)
                        {
                            successCount++;
                        }

                        if (firstErrorDownload == null && download.status == BackgroundDownloadStatus.Failed)
                        {
                            firstErrorDownload = download;
                        }
                    }

                    if (successCount < _downloads.Length)
                    {
                        Debug.LogError($"Finished downloading files, {successCount}/{_downloads.Length} succeeded.");
                        if (firstErrorDownload != null)
                        {
                            Debug.LogError($"First error: {firstErrorDownload.config.url} {firstErrorDownload.error}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Finished downloading files, {successCount}/{_downloads.Length} succeeded.");
                    }
                    break;
            }
        }

        private float _timer;

        private void Update()
        {
            if (_timer > updateInterval)
            {
                _timer = 0;
                UpdateDownload();
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }

        private void UpdateDownload()
        {
            if (_downloads == null || _downloads.Length <= 0) return;
            
            float totalProgress = 0;
            bool finished = true;
            foreach (BackgroundDownload download in _downloads)
            {
                totalProgress += download.progress;
                if (download.status == BackgroundDownloadStatus.Downloading)
                {
                    finished = false;
                }
            }

            TotalProgress = totalProgress / _downloads.Length;
            CurrentState = finished ? State.Finished : State.Downloading;
        }

        private string ResolveFileNameFromUrl(Uri url)
        {
            return Path.GetFileName(url.AbsolutePath);
        }

        public void StartDownload(string destPath, List<string> urlList)
        {
            if (CurrentState != State.Idle)
            {
                Debug.LogError("Can't start download, current state is not idle.");
                return;
            }
            
            if (urlList.Count <= 0) return;

            var configs = new List<BackgroundDownloadConfig>();
            foreach (var url in urlList)
            {
                var uri = new Uri(url);
                var fileName = ResolveFileNameFromUrl(uri);
                var destFilePath = Path.Combine(destPath, fileName);
                configs.Add(new BackgroundDownloadConfig{url = uri, filePath = destFilePath});
            }

            _downloads = BackgroundDownload.Start(configs.ToArray());
            CurrentState = _downloads.Length > 0 ? State.Downloading : State.Idle;
            TotalProgress = 0;
            _timer = 0;
        }

        public void ClearAll()
        {
            if (_downloads == null) return;
            
            foreach (var download in _downloads)
            {
                download.Dispose();
            }

            _downloads = null;
            CurrentState = State.Idle;
        }
    }
}