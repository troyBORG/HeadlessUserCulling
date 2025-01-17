using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeWorld(World world)
    {
        world.RunSynchronously(() => 
        {
            if (Config.GetValue(Enable))
            {
                // Create and setup culling system root slot
                Slot CullingRoot = world.RootSlot.AddSlot("HeadlessCullingSystem", false);
                CullingRoot.ParentReference.DriveFromRef(CullingRoot.ParentReference, false, false, true);
                CullingRoot.OrderOffset = -100000;
                Slot DynVarSlot = CullingRoot.AddSlot("DynVars", false);
                CullingRoot.Tag = "HeadlessCullingRoot";
                
                // Protect work slot from being tampered with
                CullingRoot.AttachComponent<DestroyBlock>(true, null);
                CullingRoot.AttachComponent<DuplicateBlock>(true, null);
                CullingRoot.AttachComponent<SearchBlock>(true, null);

                // Setup DynamicVariableSpace to allow for user configuration
                CullingRoot.AttachComponent<DynamicVariableSpace>(true, null);
                CullingRoot.GetComponent<DynamicVariableSpace>().SpaceName.Value = "HeadlessAvatarCulling";

                // Setup Distance variable
                Slot DistanceVarSlot = DynVarSlot.AddSlot("Distance", false);

                DistanceVarSlot.AttachComponent<DynamicValueVariable<float>>(true, null);
                DistanceVarSlot.GetComponent<DynamicValueVariable<float>>().VariableName.Value = "HeadlessAvatarCulling/CullingDistance";

                DistanceVarSlot.AttachComponent<ValueUserOverride<float>>(true, null);
                DistanceVarSlot.GetComponent<ValueUserOverride<float>>().Default.Value = 10;
                DistanceVarSlot.GetComponent<ValueUserOverride<float>>().CreateOverrideOnWrite.Value = true;
                DistanceVarSlot.GetComponent<ValueUserOverride<float>>().Target.Value = DistanceVarSlot.GetComponent<DynamicValueVariable<float>>().Value.ReferenceID;

                // Sets up this world to set up users when they join
                world.UserSpawn += InitializeUser;
            }
        }, false, null, false);
    }
}