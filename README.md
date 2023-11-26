# UnityMultiInput

### Getting started
Just download this repo as zip, unpack it somewhere and import it as a package into your Unity 2022+ project.    
To interact with the input devices, you need to get the instance of the `MarkusSecundus.MultiInput.IInputProvider` singleton - like this: `IInputProvider.Instance`.

All the available devices can obtained as elements of `IInputProvider.Instance.ActiveMice`/`ActiveKeyboards`, their available methods and properties mimick those of standard `UnityEngine.Input`.   

Devices become available at the time the player first interacts with them (first keypress, mouse movement registered etc.).


#### Limitations...
 - _Only Windows platform is currently fully supported. The package builds on other platforms, but uses a fallback implementation that just redirects to standard Unity input and thus always sees only one compound input source of each kind._
 - _Mouse cursors currently don't support automatic raycasting into Canvas UI_
 - _Still no time to write proper documentation and comments for intellisense_
 - _Probably lot more stuff_
