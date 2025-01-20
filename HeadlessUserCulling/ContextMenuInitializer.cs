using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeContextMenu(User user, Slot CullingRoot)
    {
        // Checks for existing context menu
        if (user.Root.Slot.GetChildrenWithTag("HeadlessCullingContextMenu") == null)
        {
            // Sets up context menu
            Slot ContextMenuSlot = user.Root.Slot.AddSlot("HeadlessCullingContextMenu", false);
            ContextMenuSlot.Tag = "HeadlessCullingContextMenu";

            var ItemSource = ContextMenuSlot.AttachComponent<ContextMenuItemSource>();
            ItemSource.Color.Value = colorX.Yellow;

            var RootItem = ContextMenuSlot.AttachComponent<RootContextMenuItem>();
            RootItem.Item.Target = ItemSource;

            // This holds the value that will be used to update
            // the dynamic variable through protoflux
            var DistValue = ContextMenuSlot.AttachComponent<ValueField<float>>();
            DistValue.Value.Value = 10F;

            var ButtonCycle = ContextMenuSlot.AttachComponent<ButtonValueCycle<float>>();
            ButtonCycle.TargetValue.Target = DistValue.Value;
            ButtonCycle.Values.Add(float.PositiveInfinity);
            ButtonCycle.Values.Add(20F);
            ButtonCycle.Values.Add(10F);
            ButtonCycle.Values.Add(5F);
            ButtonCycle.Values.Add(2F);
        }
    }
}