using ResoniteModLoader;
using FrooxEngine;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    public override string Name => "HeadlessUserCulling";
    public override string Author => "Raidriar796";
    public override string Version => "0.1.0";
    public override string Link => "https://github.com/Raidriar796/HeadlessUserCulling";
    public static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Config = GetConfiguration();
        Config?.Save(true);

        Engine.Current.RunPostInit(() => 
        {
            if (ModLoader.IsHeadless) Engine.Current.WorldManager.WorldAdded += InitializeWorld;
            else Msg("This mod is intended for headless clients only, please uninstall");
        });
    }
}
