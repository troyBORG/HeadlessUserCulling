using ResoniteModLoader;

namespace HeadlessAvatarCulling;

public partial class HeadlessAvatarCulling : ResoniteMod
{
    [AutoRegisterConfigKey] public static readonly ModConfigurationKey<bool> Enable =
            new ModConfigurationKey<bool>(
                "Enable",
                "Enable HeadlessAvatarCulling",
                () => true);
}
