using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarkusSecundus.MultiInput
{
    public interface IKeyboard
    {
        public int Id { get; }
        public bool IsActive { get; set; }

        public bool GetButton(KeyCode buttonNumber);
        public bool GetButtonDown(KeyCode buttonNumber);
        public bool GetButtonUp(KeyCode buttonNumber);
        public bool IsAnyButtonDown { get; }
        public bool IsAnyButtonUp { get; }
        public bool IsAnyButtonPressed { get; }

        public IReadOnlyCollection<KeyCode> ButtonsDown { get; }
        public IReadOnlyCollection<KeyCode> ButtonsUp { get; }
        public IReadOnlyCollection<KeyCode> ButtonsPressed { get; }

        public IConfiguration Config { get; }

        public interface IConfiguration
        {
            int _placeholder { get; set; }
        }
    }
}