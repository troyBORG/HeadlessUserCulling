using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeWorld(World world)
    {
        world.RunSynchronously(() => 
        {
            if (Config!.GetValue(Enable))
            {
                // Create and setup culling system root slot
                Slot CullingRoot = world.RootSlot.AddSlot("HeadlessCullingSystem", false);
                CullingRoot.ParentReference.DriveFromRef(CullingRoot.ParentReference, false, false, true);
                CullingRoot.OrderOffset = long.MinValue;
                Slot DynVarSlot = CullingRoot.AddSlot("DynVars", false);
                CullingRoot.Tag = "HeadlessCullingRoot";

                // Setup DynamicVariableSpace to allow for user configuration
                var DynVarSpace = CullingRoot.AttachComponent<DynamicVariableSpace>();
                DynVarSpace.SpaceName.Value = "HeadlessUserCulling";

                // Setup HeadlessCullingRoot variable
                Slot RootVarSlot = DynVarSlot.AddSlot("HeadlessCullingRoot", false);

                var RootDynVar = RootVarSlot.AttachComponent<DynamicReferenceVariable<Slot>>();
                RootDynVar.VariableName.Value = "World/HeadlessCullingRoot";
                RootDynVar.Reference.Target = CullingRoot;
                RootDynVar.Reference.DriveFrom(RootDynVar.Reference);

                // Setup CullingDistance variable
                Slot DistanceVarSlot = DynVarSlot.AddSlot("CullingDistance", false);
                DistanceVarSlot.Tag = "DistanceVar";

                var DistanceDynVar = DistanceVarSlot.AttachComponent<DynamicValueVariable<float>>();
                DistanceDynVar.VariableName.Value = "HeadlessUserCulling/CullingDistance";

                var DistanceOverride = DistanceVarSlot.AttachComponent<ValueUserOverride<float>>();
                DistanceOverride.Default.Value = 10;
                DistanceOverride.CreateOverrideOnWrite.Value = true;
                DistanceOverride.Target.Target = DistanceDynVar.Value;

                // Sets up this world to set up users when they join
                world.UserSpawn += InitializeUser;

                // Regenerates the world's culling slots when destroyed
                CullingRoot.Destroyed += d =>
                {
                    world.UserSpawn -= InitializeUser;
                    InitializeWorld(world); 
                };
            }
        });
    }
}