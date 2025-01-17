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

                // Sets up the culling behavior via UserDistanceValueDriver and VirtualParent
                UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>(true, null);
                UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().Node.Value = UserRoot.UserNode.View;
                UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().TargetField.Value = user.Root.Slot.ActiveSelf_Field.ReferenceID;
                UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().NearValue.Value = true;

                UserCullingSlot.AttachComponent<VirtualParent>(true, null);
                UserCullingSlot.GetComponent<VirtualParent>().OverrideParent.Value = user.Root.HeadSlot.ReferenceID;
                UserCullingSlot.GetComponent<VirtualParent>().SetVirtualChild(UserCullingSlot, false);

                // Workaround for odd behavior on user initial focus
                UserCullingSlot.AttachComponent<ValueUserOverride<bool>>(true, null);
                UserCullingSlot.GetComponent<ValueUserOverride<bool>>().Default.Value = false;
                UserCullingSlot.GetComponent<ValueUserOverride<bool>>().CreateOverrideOnWrite.Value = true;
                UserCullingSlot.GetComponent<ValueUserOverride<bool>>().Target.Value = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().FarValue.ReferenceID;
                UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().FarValue.Value = true;

                // Sets up dyn vars to be adjustable by the user
                Slot DistanceVarSlot = UserCullingSlot.AddSlot("Distance", false);
                DistanceVarSlot.AttachComponent<DynamicValueVariableDriver<float>>(true, null);
                DistanceVarSlot.GetComponent<DynamicValueVariableDriver<float>>().VariableName.Value = "HeadlessAvatarCulling/CullingDistance";
                DistanceVarSlot.GetComponent<DynamicValueVariableDriver<float>>().Target.Value = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().Distance.ReferenceID;
            }
        }, false, null, false);
    }
}