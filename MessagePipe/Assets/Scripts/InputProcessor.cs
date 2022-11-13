using System;
using UnityEngine;

namespace MessagePipe.Game
{
    public class InputProcessor : MonoBehaviour
    {
        public event Action<KeyCode> OnKeyDown = code => { };
        public event Action<KeyCode> OnKeyUp = code => { };

        private KeyCode[] _keyCodes;
        
        private void Awake()
        {
            var keys = Enum.GetValues(typeof(KeyCode));
            _keyCodes = new KeyCode[keys.Length];

            for (var i = 0; i < _keyCodes.Length; i++)
            {
                _keyCodes[i] = (KeyCode)keys.GetValue(i);
            }
        }

        private void Update()
        {
            foreach (var keyCode in _keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    OnKeyDown(keyCode);
                }

                if (Input.GetKeyUp(keyCode))
                {
                    OnKeyUp(keyCode);
                }
            }
        }
    }
}