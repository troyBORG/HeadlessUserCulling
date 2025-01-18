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
                var DynVarSpace = CullingRoot.AttachComponent<DynamicVariableSpace>(true, null);
                DynVarSpace.SpaceName.Value = "HeadlessAvatarCulling";

                // Setup Distance variable
                Slot DistanceVarSlot = DynVarSlot.AddSlot("Distance", false);

                var DistanceDynVar = DistanceVarSlot.AttachComponent<DynamicValueVariable<float>>(true, null);
                DistanceDynVar.VariableName.Value = "HeadlessAvatarCulling/CullingDistance";

                var DistanceOverride = DistanceVarSlot.AttachComponent<ValueUserOverride<float>>(true, null);
                DistanceOverride.Default.Value = 10;
                DistanceOverride.CreateOverrideOnWrite.Value = true;
                DistanceOverride.Target.Target = DistanceDynVar.Value;

                // Sets up this world to set up users when they join
                world.UserSpawn += InitializeUser;
            }
        }, false, null, false);
    }
}