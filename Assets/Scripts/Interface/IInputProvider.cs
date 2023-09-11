using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputProvider
{
    public IReadOnlyList<IMouse> Mice { get; }

    public static IInputProvider Instance { get; }
}
