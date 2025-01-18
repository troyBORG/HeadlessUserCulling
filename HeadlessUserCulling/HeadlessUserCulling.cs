using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    public override string Name => "HeadlessUserCulling";
    public override string Author => "Raidriar796";
    public override string Version => "1.0.0";
    public override string Link => "";
    public static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new("net.raidriar796.HeadlessUserCulling");
        Config = GetConfiguration();
        Config?.Save(true);
        harmony.PatchAll();

        Engine.Current.RunPostInit(() => 
        {
            Engine.Current.WorldManager.WorldAdded += InitializeWorld;
        });
    }
}
