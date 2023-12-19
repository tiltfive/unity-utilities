# Tilt Five Utilities Package for Unity
Version 1.0.1

## Dependencies

- [Unity 2019.4 LTS](https://unity.com/releases/2019-lts) or later
- [Tilt Five SDK 1.4.0](https://docs.tiltfive.com/t5_release_notes_20jj.html) or later
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

## Components

This package contains a selection of different scripts that can be used to facilitate faster development using the Tilt Five SDK. Most scripts are intended to be used as drag and droppable components that can be intermixed with other Unity components.
This package has baseline requirements of TextMeshPro and the Unity Input System being enabled.

### Gameboard Utilities
- `BoardRotate`: This script is applied to a canvas that you wish to rotate to stay in front of a player as they move around the Gameboard. Requires a PlayerIdent component to identify which player this canvas rotates towards.
- `GameboardRotator`: This script is applied to a `Gameboard` object and rotates the Gameboard to continually face the specified player.
- `GameboardUtility`: This is a set of useful utility functions such as identifying nearest Gameboard corners.
- `PseudoParent`: Places the object and its children under the Scene Hierarchy object of the same name. Used to create per player menus that follow a shared Gameboard.

### General Utilities
- `EaseUtils`: Some simple easing functions.
- `ExponentialFilterUtils`: Some simple exponential filter functions.
- `GlassesPoseFilter`: An accessor for a filtered Glasses Pose. Requires a PlayerIdent component to identify which glasses pose to filter.
- `LocalPositionInMeters`: An accessor for the local position of a specified player's glasses
- `TransformLocalValue`: A local-to-Gameboard space position for a world space transform.
- `WandAimPointFilter`: An accessor for a filtered Wand Pose. Requires a `WandIdent` component to identify which wand pose to filter.

### Player Utilities
- `ApplyPlayerColor`: Applies the color specified by the `PlayerColor` class to all text and renderer components on this object. Requires a `PlayerIdent` component to know which color to apply.
- `ApplyPlayerLayer`: Applies the layer specified by the `PlayerLayer` class to the object and all of its children. Requires a `PlayerIdent` component to know which layer to apply.
- `PlayerColors`: Specify the colors to use for each Player
- `PlayerIdent`: Use either a field or a Player Input component to define which player number a Game Object is associated with
- `PlayerLayers`: Specify the layers to use for each Player
- `PlayerUtils`: Easily access the Player Settings for a given player Index

### UI Utilities
- `CancelHandler`: Used in conjunction with Quit controller to handle exiting a quit menu
- `GlassesDisconnectManager`: Provides events to monitor and notify when relevant(watched) glasses or wands disconnect or reconnect
- `ObjectBillboard`: Specify an object to billboard to each player camera during rendering
- `ObjectBillboardHDRP`: When using HDRP, this script can be added to an object to ensure it auto-billboards
- `QuitController`: Manages the inputs for displaying/using a quit menu
- `UIHinge`: Add to an object to ensure it tilts towards the camera with a bottom anchor.
- `VersionChecker`: A script to check whether the user's drivers are out of date and thus enable a specified overlay.

### Wand Utilities
- `Grabber`: Abstract class specifier for something that interacts with an object.  Requires a `WandIdent` to know which wand to respond to.
- `GrabManager`: The controller script for various interaction methods. Allows you to specify different interaction methods on different button presses. Requires a `WandIdent` to now which wand to respond to.
- `GrabMover`: An interactor that allows you to move objects that have been grabbed around the scene. 
- `GrabRotator`: An interactor that allows you to rotate the grabbed object in place.
- `Panner`: An interactor that allows you to pan the Gameboard through the scene.
- `RaycastGrabber`: Abstract class representing something that grabs an object via Raycast with the wand.
- `RaycastGrabRotator`: Grab and Rotate an object via raycast.
- `RaycastLine`: Draw a raycast line from the wand using either line of sight or forward direction.
- `RotationUtilities`: Utility functions to enable the rotation methods.
- `WandIdent`: Use either a field or a PlayerInput component to specify the wand to interact with.
- `WandRaycaster`: Use a Wand Raycast Line to determine an interaction target.
