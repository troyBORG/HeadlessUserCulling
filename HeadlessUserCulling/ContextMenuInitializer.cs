using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Users;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeContextMenu(User user, Slot CullingRoot, Slot UserCullingSlot)
    {
        Slot ThisUserRoot = user.Root.Slot;

        // Sets up context menu
        Slot RootMenuSlot = ThisUserRoot.AddSlot("HeadlessCullingContextMenu", false);
        Slot DistanceMenuSlot = RootMenuSlot.AddSlot("CullingDistance", false);
        Slot MuteMenuSlot = RootMenuSlot.AddSlot("MuteSettings", false);

        Slot DistVarSlot = CullingRoot.GetChildrenWithTag("DistanceVar").First();
        if (!DistVarSlot.IsDestroyed)
        {
            var DistVar = DistVarSlot.GetComponent<DynamicValueVariable<float>>();

            // Root Context Menu Button
            var RootItemSource = RootMenuSlot.AttachComponent<ContextMenuItemSource>();
            RootItemSource.Label.Value = "User Culling Settings";
            RootItemSource.Color.Value = colorX.Cyan;

            var RootItem = RootMenuSlot.AttachComponent<RootContextMenuItem>();
            RootItem.ExcludeOnTools.Value = true;
            RootItem.Item.Target = RootItemSource;

            var RootSubmenu = RootMenuSlot.AttachComponent<ContextMenuSubmenu>();
            RootSubmenu.ItemsRoot.Target = RootMenuSlot;

            // Culling Distance Button
            var DistanceItemSource = DistanceMenuSlot.AttachComponent<ContextMenuItemSource>();
            DistanceItemSource.Color.Value = colorX.Yellow;

            var StringDriver = DistanceMenuSlot.AttachComponent<MultiValueTextFormatDriver>();
            StringDriver.Sources.Add(DistVar.Value);
            StringDriver.Format.Value = "Culling Distance: {0}";
            StringDriver.Text.Target = DistanceItemSource.Label;

            var ButtonCycle = DistanceMenuSlot.AttachComponent<ButtonValueCycle<float>>();
            ButtonCycle.TargetValue.Target = DistVar.Value;
            ButtonCycle.Values.Add(float.PositiveInfinity);
            ButtonCycle.Values.Add(20F);
            ButtonCycle.Values.Add(10F);
            ButtonCycle.Values.Add(5F);
            ButtonCycle.Values.Add(2F);

            // Mute Settings Submenu
            var MuteItemSource = MuteMenuSlot.AttachComponent<ContextMenuItemSource>();
            MuteItemSource.Label.Value = "Mute Culled User";
            MuteItemSource.Color.Value = colorX.Red;

            var MuteSubmenu = MuteMenuSlot.AttachComponent<ContextMenuSubmenu>();
            MuteSubmenu.ItemsRoot.Target = MuteMenuSlot;
            MuteSubmenu.Hidden.Value = true;

            // Populate Mute Submenu per user
            var UserList = user.World.AllUsers;
            for (int i = 0; i < UserList.Count; i++)
            {
                if (UserList.ElementAt(i).HeadDevice != HeadOutputDevice.Headless && UserList.ElementAt(i) != user)
                {
                    CreateMuteButton(UserList.ElementAt(i));
                }
            }

            user.World.UserJoined += CreateMuteButton;

            void CreateMuteButton(User user)
            {
                Slot UserDynVars = CullingRoot.GetChildrenWithTag(user.UserID + "-DynVars").First();
                if (!UserDynVars.IsDestroyed)
                {
                    Slot MuteControlSlot = MuteMenuSlot.AddSlot(user.UserID, false);

                    var UserMuteItemSource = MuteControlSlot.AttachComponent<ContextMenuItemSource>();
                    UserMuteItemSource.Label.Value = user.UserName;
                    
                    var UserMuteButtonToggle = MuteControlSlot.AttachComponent<ButtonToggle>();
                    UserMuteButtonToggle.TargetValue.Target = UserDynVars.GetComponent<DynamicValueVariable<bool>>().Value;
                }
            }
        }

        // Sets up the context menu to destroy itself when
        // the user's culling root is destroyed
        UserCullingSlot.Destroyed += d =>
        {
            if (!user.IsDestroyed && !ThisUserRoot.IsDestroyed && !RootMenuSlot.IsDestroyed) RootMenuSlot.Destroy();
        };

        // Regenerates the context menu if it's directly destroyed
        RootMenuSlot.Destroyed += d =>
        {
            if (!user.IsDestroyed && !CullingRoot.IsDestroyed && !UserCullingSlot.IsDestroyed && !ThisUserRoot.IsDestroyed)
            {
                InitializeContextMenu(user, CullingRoot, UserCullingSlot);
            }
        };
    }
}