using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeContextMenu(User user, Slot CullingRoot, Slot UserCullingSlot)
    {
        // Sets up context menu
        Slot ContextMenuSlot = user.Root.Slot.AddSlot("HeadlessCullingContextMenu", false);
        Slot DistVarSlot = CullingRoot.GetChildrenWithTag("DistanceVar").First();
        var DistVar = DistVarSlot.GetComponent<DynamicValueVariable<float>>();

        var ItemSource = ContextMenuSlot.AttachComponent<ContextMenuItemSource>();
        ItemSource.Color.Value = colorX.Yellow;

        var StringDriver = ContextMenuSlot.AttachComponent<MultiValueTextFormatDriver>();
        StringDriver.Sources.Add(DistVar.Value);
        StringDriver.Format.Value = "Culling Distance: {0}";
        StringDriver.Text.Target = ItemSource.Label;

        var RootItem = ContextMenuSlot.AttachComponent<RootContextMenuItem>();
        RootItem.ExcludeOnTools.Value = true;
        RootItem.Item.Target = ItemSource;

        var ButtonCycle = ContextMenuSlot.AttachComponent<ButtonValueCycle<float>>();
        ButtonCycle.TargetValue.Target = DistVar.Value;
        ButtonCycle.Values.Add(float.PositiveInfinity);
        ButtonCycle.Values.Add(20F);
        ButtonCycle.Values.Add(10F);
        ButtonCycle.Values.Add(5F);
        ButtonCycle.Values.Add(2F);

        // Sets up the context menu to destroy itself when
        // the user's culling root is destroyed
        UserCullingSlot.Destroyed += d => { if (user != null && user.Root.Slot != null) ContextMenuSlot.Destroy(); };

        // Regenerates the context menu if it's directly destroyed
        ContextMenuSlot.Destroyed += d => { if (!UserCullingSlot.IsDestroyed) InitializeContextMenu(user, CullingRoot, UserCullingSlot); };
    }
}