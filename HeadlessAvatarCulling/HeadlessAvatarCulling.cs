using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Reflection;
using FrooxEngine;
using Elements.Core;

namespace HeadlessAvatarCulling;

public partial class HeadlessAvatarCulling : ResoniteMod
{
    public override string Name => "HeadlessAvatarCulling";
    public override string Author => "Raidriar796";
    public override string Version => "1.0.0";
    public override string Link => "";
    public static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new("net.raidriar796.HeadlessAvatarCulling");
        Config = GetConfiguration();
        Config?.Save(true);
        harmony.PatchAll();
    }
}
