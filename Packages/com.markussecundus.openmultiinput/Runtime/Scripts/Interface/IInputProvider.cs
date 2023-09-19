using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarkusSecundus.MultiInput
{
    public interface IInputProvider
    {
        public IReadOnlyCollection<IMouse> ActiveMice { get; }
        public IReadOnlyCollection<IKeyboard> ActiveKeyboards { get; }

        /// <summary>
        /// Called for each mouse just after it is added to <see cref="ActiveMice"/>
        /// </summary>
        public event Action<IMouse> OnMouseActivated;

        public event Action<IKeyboard> OnKeyboardActivated;

        public static IInputProvider Instance =>
#if PLATFORM_STANDALONE_WIN
            Windows.MultiInputManagerWin32.GetInstance()
#else
            Fallback.FallbackMultiInputProvider.Instance
#endif
            ;
    }
}