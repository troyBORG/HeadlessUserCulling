using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.ParsingFormatting;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Interaction;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Slots;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeContextMenu(User user, Slot CullingRoot, Slot UserCullingSlot)
    {
        // Sets up context menu
        Slot ContextMenuSlot = user.Root.Slot.AddSlot("HeadlessCullingContextMenu", false);

        var ItemSource = ContextMenuSlot.AttachComponent<ContextMenuItemSource>();
        ItemSource.Color.Value = colorX.Yellow;

        var RootItem = ContextMenuSlot.AttachComponent<RootContextMenuItem>();
        RootItem.ExcludeOnTools.Value = true;
        RootItem.Item.Target = ItemSource;

        // This holds the value that will be used to update
        // the dynamic variable through protoflux
        var DistValue = ContextMenuSlot.AttachComponent<ValueField<float>>();

        // This fetches the dynamic variable to keep the distance value
        // persistent between rejoins, respawns, and regenerations
        Slot DistVarSlot = CullingRoot.GetChildrenWithTag("DistanceVar").First();
        var DistVar = DistVarSlot.GetComponent<DynamicValueVariable<float>>();
        DistValue.Value.DriveFrom(DistVar.Value, true);

        var ButtonCycle = ContextMenuSlot.AttachComponent<ButtonValueCycle<float>>();
        ButtonCycle.TargetValue.Target = DistValue.Value;
        ButtonCycle.Values.Add(float.PositiveInfinity);
        ButtonCycle.Values.Add(20F);
        ButtonCycle.Values.Add(10F);
        ButtonCycle.Values.Add(5F);
        ButtonCycle.Values.Add(2F);

        // This generates protoflux to drive the context menu
        // label and actually update dynamic variables
        Slot ProtofluxSlot = ContextMenuSlot.AddSlot("protoflux", false);
        ProtofluxSlot.Tag = null!;

        // Context menu Label Drive

        // string Input Node
        var MenuStringInput = ProtofluxSlot.AttachComponent<ValueObjectInput<string>>();
        MenuStringInput.Value.Value = "Culling Distance: ";

        // Source Node
        var DistValueSource = (ProtoFluxNode)ProtofluxSlot.AttachComponent(ProtoFluxHelper.GetSourceNode(typeof(float)));
        ((ISource)DistValueSource).TrySetRootSource(DistValue.Value);

        // ToString Node
        var ToString = ProtofluxSlot.AttachComponent<ToString_Float>();
        ToString.TryConnectInput(ToString.GetInput(0), DistValueSource.GetOutput(0), false, false);

        // ValueAdd Node
        var AddStrings = ProtofluxSlot.AttachComponent<ConcatenateString>();
        AddStrings.TryConnectInput(AddStrings.GetInput(0), MenuStringInput.GetOutput(0), false, false);
        AddStrings.TryConnectInput(AddStrings.GetInput(1), ToString.GetOutput(0), false, false);

        // Object Field Drive<string> Node
        var MenuLabelDrive = (ProtoFluxNode)ProtofluxSlot.AttachComponent(ProtoFluxHelper.GetDriverNode(typeof(string)));
        MenuLabelDrive.TryConnectInput(MenuLabelDrive.GetInput(0), AddStrings.GetOutput(0), false, false);
        ((IDrive)MenuLabelDrive).TrySetRootTarget(ItemSource.Label);

        // Dynamic variable write on button event pressed
        
        // Button Events Node
        var ButtonEvents = ProtofluxSlot.AttachComponent<ButtonEvents>();
        var ButtonGlobalRef = ProtofluxSlot.AttachComponent<GlobalReference<IButton>>();
        ButtonGlobalRef.Reference.Target = ItemSource;
        ButtonEvents.Button.Target = ButtonGlobalRef;

        // Root Slot Node
        var RootSlotNode = ProtofluxSlot.AttachComponent<RootSlot>();

        // string Input Node
        var TargetSlotStringInput = ProtofluxSlot.AttachComponent<ValueObjectInput<string>>();
        TargetSlotStringInput.Value.Value = "HeadlessCullingRoot";

        // int Input Node
        var SearchDepth = ProtofluxSlot.AttachComponent<ValueInput<int>>();
        SearchDepth.Value.Value = 1;

        // Find Child By Tag Node
        var FindCullingRoot = ProtofluxSlot.AttachComponent<FindChildByTag>();
        FindCullingRoot.TryConnectInput(FindCullingRoot.GetInput(0), RootSlotNode.GetOutput(0), false, false);
        FindCullingRoot.TryConnectInput(FindCullingRoot.GetInput(1), TargetSlotStringInput.GetOutput(0), false, false);
        FindCullingRoot.TryConnectInput(FindCullingRoot.GetInput(2), SearchDepth.GetOutput(0), false, false);

        // string Input Node
        var DynVarStringInput = ProtofluxSlot.AttachComponent<ValueObjectInput<string>>();
        DynVarStringInput.Value.Value = "HeadlessUserCulling/CullingDistance";

        // Write Dynamic float Node
        var WriteDistVar = ProtofluxSlot.AttachComponent<WriteDynamicValueVariable<float>>();
        ButtonEvents.GetImpulse(0).Target = WriteDistVar;
        WriteDistVar.TryConnectInput(WriteDistVar.GetInput(0), FindCullingRoot.GetOutput(0), false, false);
        WriteDistVar.TryConnectInput(WriteDistVar.GetInput(1), DynVarStringInput.GetOutput(0), false, false);
        WriteDistVar.TryConnectInput(WriteDistVar.GetInput(2), DistValueSource.GetOutput(0), false, false);

        // Sets up the context menu to destroy itself when
        // the user's culling root is destroyed
        UserCullingSlot.Destroyed += d => { if (user != null && user.Root.Slot != null) ContextMenuSlot.Destroy(); };

        // Regenerates the context menu if it's directly destroyed
        ContextMenuSlot.Destroyed += d => { if (!UserCullingSlot.IsDestroyed) InitializeContextMenu(user, CullingRoot, UserCullingSlot); };
    }
}