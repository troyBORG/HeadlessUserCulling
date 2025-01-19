using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
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
                // Create and setup user specific culling slots
                Slot CullingRoot = user.World.RootSlot.GetChildrenWithTag("HeadlessCullingRoot").First();
                Slot UserCullingSlot = CullingRoot.AddSlot(user.UserID, false);
                UserCullingSlot.Tag = null;
                Slot DynVarSlot = UserCullingSlot.AddSlot("DynVars", false);
                Slot HelpersSlot = UserCullingSlot.AddSlot("Helpers", false);

                // Sets up a destroy proxy to clean up culling slots when the user leaves or respawns
                var DestroyProxy = user.Root.Slot.AttachComponent<DestroyProxy>();
                DestroyProxy.DestroyTarget.Target = UserCullingSlot;

                // Sets up the culling behavior via UserDistanceValueDriver and CopyGlobalTransform
                var DistanceCheck = UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>();
                DistanceCheck.Node.Value = UserRoot.UserNode.View;
                DistanceCheck.NearValue.Value = true;

                var CopyGlobalTransform = UserCullingSlot.AttachComponent<CopyGlobalTransform>();
                CopyGlobalTransform.Source.Target = user.Root.Slot;

                // Drives the root scale of the user's culling
                // slots so the scale stays consistent
                var RootScaleStream = user.GetStreamOrAdd<ValueStream<float3>>("Root.Scale", null);
                var RootScaleDriver = UserCullingSlot.AttachComponent<ValueDriver<float3>>();
                RootScaleDriver.ValueSource.Target = RootScaleStream;
                RootScaleDriver.DriveTarget.Target = UserCullingSlot.Scale_Field;
                
                // Links the user active field to the distance component
                // instead of writing it once to improve reliability,
                // primarily for the user respawning.
                var RefCast = UserCullingSlot.AttachComponent<ReferenceCast<Sync<bool>,IField<bool>>>();
                RefCast.Source.Target = user.Root.Slot.ActiveSelf_Field;
                RefCast.Target.Target = DistanceCheck.TargetField;

                // Sets up a bool value driver to read the culled state and flip
                // the value for other values to be enabled while the user is culled
                var BoolFlip = UserCullingSlot.AttachComponent<BooleanValueDriver<bool>>();
                BoolFlip.State.DriveFrom(user.Root.Slot.ActiveSelf_Field);
                BoolFlip.TargetField.Value = HelpersSlot.ActiveSelf_Field.ReferenceID;
                BoolFlip.FalseValue.Value = true;
                BoolFlip.TrueValue.DriveFrom(DistanceCheck.FarValue);

                // Workaround for odd behavior on user initial focus
                // I intend to replace this eventually, but if I cannot
                // find a better method this will stay as is
                var HostOverride = UserCullingSlot.AttachComponent<ValueUserOverride<bool>>();
                HostOverride.Default.Value = false;
                HostOverride.CreateOverrideOnWrite.Value = true;
                HostOverride.Target.Target = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().FarValue;
                DistanceCheck.FarValue.Value = true;

                // Sets up dyn vars to be adjustable by the user
                Slot DistanceVarSlot = DynVarSlot.AddSlot("Distance", false);

                var DistanceDriver = DistanceVarSlot.AttachComponent<DynamicValueVariableDriver<float>>();
                DistanceDriver.VariableName.Value = "HeadlessAvatarCulling/CullingDistance";
                DistanceDriver.Target.Target = DistanceCheck.Distance;

                // Generates visuals for culled user's head and hands
                Slot VisualSlot = HelpersSlot.AddSlot("Visuals", false);

                // Gets the default pbs metallic to avoid duplicating materials
                var DefaultMaterial = user.World.GetSharedComponentOrCreate("DefaultMaterial", delegate(PBS_Metallic mat) {}, 0, false, false, null);

                // This sets up the visuals and uses existing value streams from
                // the user to drive the position and rotation of the culled visuals

                // Head visual setup
                Slot HeadVisualSlot = VisualSlot.AddSlot("HeadVisual", false);

                Slot HeadMeshSlot = HeadVisualSlot.AddSlot("Mesh", false);
                HeadMeshSlot.Rotation_Field.Value = floatQ.Euler(90F, 180F, 180F);
                var HeadMesh = HeadMeshSlot.AttachMesh<ConeMesh>(DefaultMaterial, false);
                HeadMesh.Height.Value = 0.25F;
                HeadMesh.RadiusBase.Value = 0.15F;
                HeadMesh.Sides.Value = 3;
                HeadMesh.FlatShading.Value = true;

                var HeadPosStream = user.GetStreamOrAdd<ValueStream<float3>>("Head", null);
                var HeadPosDriver = HeadVisualSlot.AttachComponent<ValueDriver<float3>>();
                HeadPosDriver.ValueSource.Target = HeadPosStream;
                HeadPosDriver.DriveTarget.Target = HeadVisualSlot.Position_Field;

                var HeadRotStream = user.GetStreamOrAdd<ValueStream<floatQ>>("Head", null);
                var HeadRotDriver = HeadVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                HeadRotDriver.ValueSource.Target = HeadRotStream;
                HeadRotDriver.DriveTarget.Target = HeadVisualSlot.Rotation_Field;

                // Left hand visual setup
                Slot LeftHandVisualSlot = VisualSlot.AddSlot("LeftHandVisual", false);

                Slot LeftHandMeshSlot = LeftHandVisualSlot.AddSlot("Mesh", false);
                LeftHandMeshSlot.Rotation_Field.Value = floatQ.Euler(90F, 180F, 180F);
                var LeftHandMesh = LeftHandMeshSlot.AttachMesh<ConeMesh>(DefaultMaterial, false);
                LeftHandMesh.Height.Value = 0.2F;
                LeftHandMesh.RadiusBase.Value = 0.1F;
                LeftHandMesh.Sides.Value = 3;
                LeftHandMesh.FlatShading.Value = true;

                var LeftHandPosStream = user.GetStreamOrAdd<ValueStream<float3>>("LeftHand", null);
                var LeftHandPosDriver = LeftHandVisualSlot.AttachComponent<ValueDriver<float3>>();
                LeftHandPosDriver.ValueSource.Target = LeftHandPosStream;
                LeftHandPosDriver.DriveTarget.Target = LeftHandVisualSlot.Position_Field;
                
                var LeftHandRotStream = user.GetStreamOrAdd<ValueStream<floatQ>>("LeftHand", null);
                var LeftHandRotDriver = LeftHandVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                LeftHandRotDriver.ValueSource.Target = LeftHandRotStream;
                LeftHandRotDriver.DriveTarget.Target = LeftHandVisualSlot.Rotation_Field;

                // Right hand visual setup
                Slot RightHandVisualSlot = VisualSlot.AddSlot("RightHandVisual", false);

                Slot RightHandMeshSlot = RightHandVisualSlot.AddSlot("Mesh", false);
                RightHandMeshSlot.Rotation_Field.Value = floatQ.Euler(90F, 180F, 180F);
                var RightHandMesh = RightHandMeshSlot.AttachMesh<ConeMesh>(DefaultMaterial, false);
                RightHandMesh.Height.Value = 0.2F;
                RightHandMesh.RadiusBase.Value = 0.1F;
                RightHandMesh.Sides.Value = 3;
                RightHandMesh.FlatShading.Value = true;

                var RightHandPosStream = user.GetStreamOrAdd<ValueStream<float3>>("RightHand", null);
                var RightHandPosDriver = RightHandVisualSlot.AttachComponent<ValueDriver<float3>>();
                RightHandPosDriver.ValueSource.Target = RightHandPosStream;
                RightHandPosDriver.DriveTarget.Target = RightHandVisualSlot.Position_Field;

                var RightHandRotStream = user.GetStreamOrAdd<ValueStream<floatQ>>("RightHand", null);
                var RightHandRotDriver = RightHandVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                RightHandRotDriver.ValueSource.Target = RightHandRotStream;
                RightHandRotDriver.DriveTarget.Target = RightHandVisualSlot.Rotation_Field;

                // Recreates the Audio Output on the user
                // to keep audio working while a user is culled
                var UserVoice = user.Root.Slot.GetComponent<AvatarVoiceInfo>().AudioSource.Value;

                Slot AudioSlot = HeadVisualSlot.AddSlot("Audio", false);

                var AudioOutput = AudioSlot.AttachComponent<AudioOutput>();
                AudioOutput.Source.Value = UserVoice;
                AudioOutput.Priority.Value = 0;
                AudioOutput.AudioTypeGroup.Value = AudioTypeGroup.Voice;

                var AudioManager = AudioSlot.AttachComponent<AvatarAudioOutputManager>();
                AudioManager.AudioOutput.Target = AudioOutput;
                AudioManager.OnEquip(user.Root.Slot.GetComponentInChildren<AvatarObjectSlot>());

                // This is needed because otherwise, the min scale will
                // be set to Infinity, making the audio output not work
                AudioOutput.MinScale.ActiveLink.ReleaseLink(true);
                AudioOutput.MinScale.Value = 1F;
            }
        }, false, null, false);
    }
}