using MessagePipe.Common;
using UnityEngine;

namespace MessagePipe.Game
{
    public class MessageBroadcaster : MonoBehaviour
    {
        private GameClient _client = default;
        private InputProcessor _processor = default;

        private void Awake()
        {
            _client = FindObjectOfType<GameClient>();
            _processor = FindObjectOfType<InputProcessor>();
        }

        private void Start()
        {
            _processor.OnKeyDown += OnKeyDown;
            _processor.OnKeyUp += OnKeyUp;
        }

        private void OnKeyUp(KeyCode keyCode)
        {
            _client.Send(Packet.Create((int)keyCode));
            print("keyCode: " + keyCode);
        }

        private void OnKeyDown(KeyCode keyCode)
        {
            _client.Send(Packet.Create((int)keyCode));
            print("keyCode: " + keyCode);
        }
    }
}