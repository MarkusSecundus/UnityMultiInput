# UnityMultiInput

### Getting started
Just download this repo as zip, unpack it somewhere and import it as a package into your project.    
To interact with the input devices, you need to get the instance of the `MarkusSecundus.MultiInput.IInputProvider` singleton - like this: `IInputProvider.Instance`.

All the available devices can obtained as elements of `IInputProvider.Instance.ActiveMice`/`ActiveKeyboards`, their available methods and properties mimick those of standard `UnityEngine.Input`.   

Devices become available at the time the player first interacts with them (first keypress, mouse movement registered etc.).
