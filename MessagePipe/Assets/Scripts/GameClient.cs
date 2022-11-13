using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MessagePipe.Game
{
    using Common;

    public class GameClient : MonoBehaviour
    {
        private readonly Queue<Action> _actions = new Queue<Action>(16);
        private readonly object _lockObj = new object();

        private MessageClient _client;
        private Thread _thread;
        private int _serverPid;

        private string _inPipeName;
        private string _outPipeName;

        private void Start()
        {
            PipeLogger.VerboseLogger += UnityEngine.Debug.Log;
            
            Screen.SetResolution(1600, 900, FullScreenMode.Windowed);

            var guid = Guid.NewGuid();

            _inPipeName = Constants.DefaultS2C + guid;
            _outPipeName = Constants.DefaultC2S + guid;

            var arguments = new PipeArgument.Builder()
                .ServerToClient(_inPipeName)
                .ClientToServer(_outPipeName)
                .OwnerProcess(Process.GetCurrentProcess().Id)
                .Build();

            print("[In  Pipe] " + _inPipeName);
            print("[Out Pipe] " + _outPipeName);
            print("arguments: " + arguments);

#if UNITY_EDITOR
            var subProcessPath = Path.Combine(Application.dataPath, "Plugins", "x64", "MessagePipe.Server.Managed.exe");
#else
            var subProcessPath = Path.Combine(Application.dataPath, "Plugins", "x86_64", "MessagePipe.Server.Managed.exe");
#endif
            subProcessPath = Path.GetFullPath(subProcessPath);

            var processStartInfo = new ProcessStartInfo(subProcessPath)
            {
#if !UNITY_EDITOR
                CreateNoWindow = true,
                UseShellExecute = false,
#endif
                Arguments = arguments
            };

#if UNITY_EDITOR
            var process = Process.Start(processStartInfo);
            _serverPid = process?.Id ?? 0;
#else
            _serverPid = NativeProcess.Start(processStartInfo);
#endif
            print($"server process pid: {_serverPid}");

            if (_serverPid > 0)
            {
                var threadStart = new ThreadStart(RunMessageClient);

                _thread = new Thread(threadStart);
                _thread.Start();
            }
        }

        private void RunMessageClient()
        {
            _client = new MessageClient(_inPipeName, _outPipeName);
            _client.OnMessageReceived += OnMessage;

            _client.Connect();
            _client.WaitForExit();
        }

        private void OnDestroy()
        {
            _client?.Dispose();
            _thread?.Abort();
        }

        private void Update()
        {
            lock (_lockObj)
            {
                if (_actions.Count > 0)
                {
                    var action = _actions.Dequeue();
                    action();
                }
            }
        }

        private void OnMessage(Packet packet)
        {
            Dispatch(() => { print("Packet received! " + packet); });
        }

        private void Dispatch(Action action)
        {
            lock (_lockObj)
            {
                _actions.Enqueue(action);
            }
        }

        public void Send(Packet packet)
        {
            _client.Send(packet);
        }
    }
}