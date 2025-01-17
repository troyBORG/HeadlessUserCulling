using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeCulling(World world)
    {
        world.RunSynchronously(() => {
            if (Config.GetValue(Enable))
            {
                // Create and setup culling system root slot
                var CullingRoot = world.RootSlot.AddSlot("HeadlessCullingSystem", false);
                CullingRoot.ParentReference.DriveFromRef(CullingRoot.ParentReference, false, false, true);
                CullingRoot.Tag = "HeadlessCullingRoot";
                CullingRoot.OrderOffset = -1;
                
                // Protect work slot from being tampered with
                CullingRoot.AttachComponent<DestroyBlock>(true, null);
                CullingRoot.AttachComponent<DuplicateBlock>(true, null);
                CullingRoot.AttachComponent<SearchBlock>(true, null);

                // Setup DynamicVariableSpace to allow for user configuration
                CullingRoot.AttachComponent<DynamicVariableSpace>(true, null);
                CullingRoot.GetComponent<DynamicVariableSpace>().SpaceName.Value = "HeadlessAvatarCulling";
            }
        }, false, null, false);
    }
}