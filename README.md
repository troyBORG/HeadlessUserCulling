# HeadlessUserCulling

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that provides a user culling system managed by the headless client.

## Features

- Generates a user distance culling system on the fly

- Optionally generates a context menu for system settings

- Fallback audio, head and hands visuals, and nameplates

- Dynamic variables for world integration

## Planned Features

- Settings template .resonitepackage

- Headless commands

- Modular fallback visuals

- User slot frustum culling

- Hardended mode

## Requirements
- [Headless Client](https://wiki.resonite.com/Headless_Server_Software)
- [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader)

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [HeadlessUserCulling.dll](https://github.com/Raidriar796/HeadlessUserCulling/releases/latest/download/HeadlessUserCulling.dll) into your `rml_mods` folder inside of the headless installation. You can create it if it's missing, or if you launch the client once with ResoniteModLoader installed it will create the folder for you.
3. Start the client. If you want to verify that the mod is working you can check the headless client's logs.

## Config Options

- `Enabled`
  - Determines if the mod runs or not.

- `AutoGenContextMenu`
  - Creates a context menu on each user to access settings on the culling system.

## Dynamic Variables

If you want to integrate the culling system's settings into your world, the following dynamic variables are provided, but make sure `AutoGenContextMenu` is set to false:

- `World/HeadlessCullingRoot`
  - Slot Reference
  - Read Only

- `HeadlessUserCulling/CullingDistance`
  - Float Value
  - Read/Write
  - Local Overrides

## What is user culling?

User culling involves using systems in Resonite to disable users by either distance or being in different parts of the world to improve performance in a session.

## Why use this mod over existing in-engine systems?

Through a mod, the headless can create all the necessary slots and components needed to make a user culling system on the fly for any world. This removes the need to setup a user culling system on a per world/session basis. The headless can also access values and references that would normally require ref hacking to access.

Culling within Resonite is often achieved through parenting the user or their avatar under a controlled slot, or accessing the user's root slot active field to control the active state directly via ref hacking. These are prone to breaking or causing unintended behavior, which this mod is aiming to avoid while being just as, if not more effective.

This is not meant to target, call out, or be hostile towards anyone's user culling system, but rather the goal is to iterate on and improve user culling overall. I have been analyzing every public user culling system I could find to figure out their strengths and weaknesses, and this mod is the result.

## How does it work?

There are 2 major parts to this mod, that being the world setup, and the user setup:

### Part 1: World Setup

When a session is started, a non persistent slot is created. This slot is where the headless will setup and manage culling behavior.

Next, dynamic variables are setup. This allows hosts/world creators to integrate the variables into the world's settings.

### Part 2: User Setup

This is where the bulk of the work happens. When a user joins the session or respawns, the headless will create a new slot under the culling system. The main thing that's done to this slot is it's position, rotation, and scale are driven to follow the user, which will be important for the rest of the system to function and remain consistent.

Next, a new slot is created for the components that will handle the distance check, which is primarily handled with the `UserDistanceValueDriver` component. This slot also includes some overrides and other components to enable other parts of the system when the user is culled.

After this, the dynamic variable drivers are setup, this is what allows the user's settings to adjust the culling system.

Now here's where we solve some problems. Disabling the user's root slot disallows us from seeing or hearing them, which is where the culled audio and visuals come in. Another slot is created which will hold a few more slots, that being for the head, the left hand, and the right hand culled visuals. Procedural meshes are setup on these slots which will give you an approximation of a user's movements while they're culled.

It may not be clear who the culled user is, so a nameplate is generated. Including the live indicator.

Finally, a new slot is created underneath the culled head visual, and an audio output is setup mimicking the audio outputs that are found on avatars. The audio output is then supplied the user's audio stream.

The end result is a collection of slots that's independent of a user's slots that positions itself at the user, handles variables and distance checks for you, and provides fallback audio and visuals when they are culled.

There is one more optional step that may occur though. If the `AutoGenContextMenu` config option is enabled, a context menu will be created directly on the user so they can interface with the culling system without any extra setup from the host/world creator.