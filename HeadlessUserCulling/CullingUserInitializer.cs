using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using ResoniteModLoader;
using System.Reflection;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeUser(User user)
    {
        user.World.RunSynchronously(() => 
        {
            if (!user.IsDestroyed && !user.World.IsDestroyed && user != user.World.HostUser)
            {
                // Create and setup user specific culling slots
                Slot ThisUserRoot = user.Root.Slot;
                Slot CullingRoot = user.World.RootSlot.GetChildrenWithTag("HeadlessCullingRoot").First();
                Slot UserCullingSlot = CullingRoot.AddSlot(user.UserName, false);
                UserCullingSlot.Tag = null!;
                Slot DistCheckSlot = UserCullingSlot.AddSlot("DistCheck", false);
                Slot DynVarSlot = UserCullingSlot.AddSlot("DynVars", false);
                Slot HelpersSlot = UserCullingSlot.AddSlot("Helpers", false);

                // Prevents the user from culling themselves
                var UserOverride = UserCullingSlot.ActiveSelf_Field.OverrideForUser(user, false);
                UserOverride.Default.Value = true;

                // Sets up the culling behavior via UserDistanceValueDriver, 
                // CopyGlobalTransform, and CopyGlobalScale
                var DistanceCheck = DistCheckSlot.AttachComponent<UserDistanceValueDriver<bool>>();
                DistanceCheck.Node.Value = UserRoot.UserNode.View;
                DistanceCheck.NearValue.Value = true;

                // The host needs to see the user to spawn properly,
                // this override prevents weird issues on user spawn.
                DistanceCheck.FarValue.OverrideForUser(user.World.HostUser, true);

                // Keeps the root of the user's culling slots positioned and scaled
                // at the user's root slot, this is the only transform that updates
                // while the user is culled.
                var CopyGlobalTransform = UserCullingSlot.AttachComponent<CopyGlobalTransform>();
                CopyGlobalTransform.Source.Target = ThisUserRoot;

                var CopyGlobalScale = UserCullingSlot.AttachComponent<CopyGlobalScale>();
                CopyGlobalScale.Source.Target = ThisUserRoot;
                
                // Links the user active field to the distance component
                // instead of writing it once to improve reliability,
                // primarily for the user respawning.
                user.World.RunInUpdates(6, () =>
                {
                    if (!ThisUserRoot.IsDestroyed && !DistanceCheck.IsDestroyed)
                    {
                        var RefCast = DistCheckSlot.AttachComponent<ReferenceCast<Sync<bool>,IField<bool>>>();
                        RefCast.Source.Target = ThisUserRoot.ActiveSelf_Field;
                        RefCast.Target.Target = DistanceCheck.TargetField;
                    }
                });

                // Sets up a bool value driver to read the culled state and flip
                // the value for other values to be enabled while the user is culled
                var BoolFlip = DistCheckSlot.AttachComponent<BooleanValueDriver<bool>>();
                BoolFlip.State.DriveFrom(ThisUserRoot.ActiveSelf_Field);
                BoolFlip.TargetField.Value = HelpersSlot.ActiveSelf_Field.ReferenceID;
                BoolFlip.FalseValue.Value = true;
                BoolFlip.TrueValue.DriveFrom(DistanceCheck.FarValue);

                // Sets up dyn vars to be adjustable by the user
                var DistanceDriver = DynVarSlot.AttachComponent<DynamicValueVariableDriver<float>>();
                DistanceDriver.VariableName.Value = "HeadlessUserCulling/CullingDistance";
                DistanceDriver.Target.Target = DistanceCheck.Distance;

                // Generates visuals for culled user's head and hands

                // Gets the default pbs metallic to avoid duplicating materials
                var DefaultMaterial = user.World.GetSharedComponentOrCreate("DefaultMaterial", delegate(PBS_Metallic mat) {});

                // This sets up the visuals and uses existing value streams from
                // the user to drive the position and rotation of the culled visuals

                // Head visual setup
                Slot HeadVisualSlot = HelpersSlot.AddSlot("HeadVisual", false);

                Slot HeadMeshSlot = HeadVisualSlot.AddSlot("Mesh", false);

                var UserInfo = HeadMeshSlot.AttachComponent<CloudUserInfo>();
                UserInfo.UserId.Value = user.UserID;
                var UserIconURL = UserInfo.IconURL;

                var UserIcon = HeadMeshSlot.AttachComponent<StaticTexture2D>();
                UserIcon.URL.DriveFrom(UserIconURL);

                var HeadMat = HeadMeshSlot.AttachComponent<UnlitMaterial>();
                HeadMat.Texture.Target = UserIcon;
                HeadMat.TextureScale.Value = new float2(-1F, 1F);
                HeadMat.BlendMode.Value = BlendMode.Cutout;
                HeadMat.Sidedness.Value = Sidedness.Double;

                var HeadMesh = HeadMeshSlot.AttachMesh<CurvedPlaneMesh>(HeadMat, false);
                HeadMesh.Size.Value = new float2(0.5F, 0.5F);
                HeadMesh.Curvature.Value = 0.75F;

                var HeadPosStream = user.GetStream<ValueStream<float3>>(s => s.Name == "Head");
                if (HeadPosStream != null)
                {
                    var HeadPosDriver = HeadVisualSlot.AttachComponent<ValueDriver<float3>>();
                    HeadPosDriver.ValueSource.Target = HeadPosStream;
                    HeadPosDriver.DriveTarget.Target = HeadVisualSlot.Position_Field;
                }

                var HeadRotStream = user.GetStream<ValueStream<floatQ>>(s => s.Name == "Head");
                if (HeadRotStream != null)
                {
                    var HeadRotDriver = HeadVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                    HeadRotDriver.ValueSource.Target = HeadRotStream;
                    HeadRotDriver.DriveTarget.Target = HeadVisualSlot.Rotation_Field;
                }

                // Left hand visual setup
                Slot LeftHandVisualSlot = HelpersSlot.AddSlot("LeftHandVisual", false);

                Slot LeftHandMeshSlot = LeftHandVisualSlot.AddSlot("Mesh", false);
                LeftHandMeshSlot.Rotation_Field.Value = floatQ.Euler(90F, 180F, 180F);
                var LeftHandMesh = LeftHandMeshSlot.AttachMesh<ConeMesh>(DefaultMaterial, false);
                LeftHandMesh.Height.Value = 0.2F;
                LeftHandMesh.RadiusBase.Value = 0.1F;
                LeftHandMesh.Sides.Value = 3;
                LeftHandMesh.FlatShading.Value = true;

                var LeftHandPosStream = user.GetStream<ValueStream<float3>>(s => s.Name == "LeftHand");
                if (LeftHandPosStream != null)
                {
                    var LeftHandPosDriver = LeftHandVisualSlot.AttachComponent<ValueDriver<float3>>();
                    LeftHandPosDriver.ValueSource.Target = LeftHandPosStream;
                    LeftHandPosDriver.DriveTarget.Target = LeftHandVisualSlot.Position_Field;
                }
                
                var LeftHandRotStream = user.GetStream<ValueStream<floatQ>>(s => s.Name == "LeftHand");
                if (LeftHandRotStream != null)
                {
                    var LeftHandRotDriver = LeftHandVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                    LeftHandRotDriver.ValueSource.Target = LeftHandRotStream;
                    LeftHandRotDriver.DriveTarget.Target = LeftHandVisualSlot.Rotation_Field;
                }

                // Right hand visual setup
                Slot RightHandVisualSlot = HelpersSlot.AddSlot("RightHandVisual", false);

                Slot RightHandMeshSlot = RightHandVisualSlot.AddSlot("Mesh", false);
                RightHandMeshSlot.Rotation_Field.Value = floatQ.Euler(90F, 180F, 180F);
                var RightHandMesh = RightHandMeshSlot.AttachMesh<ConeMesh>(DefaultMaterial, false);
                RightHandMesh.Height.Value = 0.2F;
                RightHandMesh.RadiusBase.Value = 0.1F;
                RightHandMesh.Sides.Value = 3;
                RightHandMesh.FlatShading.Value = true;

                var RightHandPosStream = user.GetStream<ValueStream<float3>>(s => s.Name == "RightHand");
                if (RightHandPosStream != null)
                {
                    var RightHandPosDriver = RightHandVisualSlot.AttachComponent<ValueDriver<float3>>();
                    RightHandPosDriver.ValueSource.Target = RightHandPosStream;
                    RightHandPosDriver.DriveTarget.Target = RightHandVisualSlot.Position_Field;
                }

                var RightHandRotStream = user.GetStream<ValueStream<floatQ>>(s => s.Name == "RightHand");
                if (RightHandRotStream != null)
                {
                    var RightHandRotDriver = RightHandVisualSlot.AttachComponent<ValueDriver<floatQ>>();
                    RightHandRotDriver.ValueSource.Target = RightHandRotStream;
                    RightHandRotDriver.DriveTarget.Target = RightHandVisualSlot.Rotation_Field;
                }

                // Mimics the default nameplate to keep consistency, but it's not 1:1
                Slot NameplateSlot = HelpersSlot.AddSlot("Nameplate", false);
                var NameplatePosDriver = NameplateSlot.AttachComponent<ValueDriver<float3>>();
                if (HeadPosStream != null) NameplatePosDriver.ValueSource.Target = HeadPosStream;
                NameplatePosDriver.DriveTarget.Target = NameplateSlot.Position_Field;

                Slot NameBadgeSlot = NameplateHelper.SetupDefaultNameBadge(NameplateSlot, user);

                var NameTagAssigner = NameBadgeSlot.GetComponent<AvatarNameTagAssigner>();
                NameTagAssigner.UpdateTags(ThisUserRoot.GetComponentInChildren<AvatarManager>());
                NameTagAssigner.UserIdTargets.Clear();
                NameTagAssigner.ColorTargets.Clear();
                NameTagAssigner.OutlineTargets.Clear();

                NameBadgeSlot.GetComponent<ContactLink>().UserId.Value = user.UserID;

                NameBadgeSlot.GetComponent<PositionAtUser>().Destroy();

                NameBadgeSlot.Position_Field.Value = new float3(0F, 0.35F, 0F);

                Slot LiveIndicatorSlot = NameplateHelper.SetupDefaultLiveIndicator(NameplateSlot, user);
                LiveIndicatorSlot.GetComponent<PositionAtUser>().Destroy();
                LiveIndicatorSlot.Position_Field.Value = new float3(0F, 0.55F, 0F);

                // Positions strictly the distance check at the user's head
                // via value streams to keep the distance check comparing between
                // one your user's head to another user's head.
                // This is placed later so I can reuse the existing
                // stream var, which isn't declared yet where I would've
                // preferred to include this.
                var DistCheckPosDriver = DistCheckSlot.AttachComponent<ValueDriver<float3>>();
                if (HeadPosStream != null) DistCheckPosDriver.ValueSource.Target = HeadPosStream;
                DistCheckPosDriver.DriveTarget.Target = DistCheckSlot.Position_Field;

                // Recreates the Audio Output on the user
                // to keep audio working while a user is culled
                var UserVoice = ThisUserRoot.GetComponent<AvatarVoiceInfo>().AudioSource.Value;

                Slot AudioSlot = HeadVisualSlot.AddSlot("Audio", false);

                var AudioOutput = AudioSlot.AttachComponent<AudioOutput>();
                AudioOutput.Source.Value = UserVoice;
                AudioOutput.Priority.Value = 0;
                AudioOutput.AudioTypeGroup.Value = AudioTypeGroup.Voice;

                var AudioManager = AudioSlot.AttachComponent<AvatarAudioOutputManager>();
                AudioManager.AudioOutput.Target = AudioOutput;
                AudioManager.OnEquip(ThisUserRoot.GetComponentInChildren<AvatarObjectSlot>());

                // Sets the scale compensation to 1 to prevent the culled audio from breaking
                ((Sync<float>)AudioManager.GetType().GetField("_scaleCompensation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(AudioManager)!).Value = 1.0F;

                // Causes the user's culled slots to regenerate if destroyed
                UserCullingSlot.Destroyed += d => 
                {
                    if (!user.IsDestroyed)
                    {
                        user.World.RunInUpdates(3, () => 
                        {
                            if (!user.IsDestroyed && !ThisUserRoot.IsDestroyed) InitializeUser(user);
                        });
                    }
                };

                // Causes the user's culled slots to be deleted when the
                // user's root slot is destroyed for any reason
                ThisUserRoot.Destroyed += d =>
                {
                    if (!UserCullingSlot.IsDestroyed) UserCullingSlot.Destroy();
                };

                // Generates a context menu if enabled in the mod config
                if (Config!.GetValue(AutoGenContextMenu)) InitializeContextMenu(user, CullingRoot, UserCullingSlot);
            }
        });
    }
}