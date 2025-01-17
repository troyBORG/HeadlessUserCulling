using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeUser(User user)
    {
        user.World.RunSynchronously(() => 
        {
            if (user != user.World.HostUser)
            {
                // Create and setup user specific culling slot
                Slot CullingRoot = user.World.RootSlot.GetChildrenWithTag("HeadlessCullingRoot").First();
                Slot UserCullingSlot = CullingRoot.AddSlot(user.UserID, false);
                UserCullingSlot.Tag = null;
                Slot DynVarSlot = CullingRoot.AddSlot("DynVars", false);

                // Sets up the culling behavior via UserDistanceValueDriver and VirtualParent
                var PrimaryDistCheck = UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>(true, null);
                PrimaryDistCheck.Node.Value = UserRoot.UserNode.View;
                PrimaryDistCheck.TargetField.Value = user.Root.Slot.ActiveSelf_Field.ReferenceID;
                PrimaryDistCheck.NearValue.Value = true;

                var SecondaryDistCheck = UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>(true, null);
                SecondaryDistCheck.Node.Value = UserRoot.UserNode.View;
                SecondaryDistCheck.FarValue.Value = true;

                var VirtualParent = UserCullingSlot.AttachComponent<VirtualParent>(true, null);
                VirtualParent.OverrideParent.Value = user.Root.HeadSlot.ReferenceID;
                VirtualParent.SetVirtualChild(UserCullingSlot, false);

                // Workaround for odd behavior on user initial focus
                // I intend to replace this eventually, but if I cannot
                // find a better method this will stay as is
                var HostOverride = UserCullingSlot.AttachComponent<ValueUserOverride<bool>>(true, null);
                HostOverride.Default.Value = false;
                HostOverride.CreateOverrideOnWrite.Value = true;
                HostOverride.Target.Value = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().FarValue.ReferenceID;
                PrimaryDistCheck.FarValue.Value = true;

                // Sets up dyn vars to be adjustable by the user
                Slot DistanceVarSlot = DynVarSlot.AddSlot("Distance", false);
                DistanceVarSlot.AttachComponent<DynamicValueVariableDriver<float>>(true, null);
                DistanceVarSlot.GetComponent<DynamicValueVariableDriver<float>>().VariableName.Value = "HeadlessAvatarCulling/CullingDistance";
                DistanceVarSlot.GetComponent<DynamicValueVariableDriver<float>>().Target.Value = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().Distance.ReferenceID;
            }
        }, false, null, false);
    }
}