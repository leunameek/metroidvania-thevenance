# Level triggers

Two reusable trigger prefabs live in `Assets/Prototype/Prefabs`.

## CameraZoomTrigger

1. Drag `CameraZoomTrigger.prefab` into the scene and resize its `Box Collider` to cover the
   desired area.
2. Leave `Target Camera` empty to use the camera tagged `MainCamera`, or assign a camera
   explicitly.
3. Set `Zoomed Out Field Of View` (80 by default) and `Transition Duration` (0.5 seconds by
   default).

While any collider belonging to the player remains inside the volume, the camera transitions to
the configured field of view. It returns to its original field of view after the player exits.

## LevelTeleportTrigger

1. Drag `LevelTeleportTrigger.prefab` into the scene and resize its `Box Collider`.
2. Enter the destination in `Destination Scene Name`. A scene name such as `Movement` or its full
   asset path are both accepted.
3. Ensure the destination scene is enabled under **File > Build Profiles > Scene List**.

The destination is loaded in `Single` mode the first time the player enters the volume. Empty or
unavailable destinations report an error in the Console instead of attempting a scene load.
