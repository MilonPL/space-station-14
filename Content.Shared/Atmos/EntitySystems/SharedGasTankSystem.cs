using Content.Shared.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Atmos.EntitySystems;

public abstract class SharedGasTankSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasTankComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<GasTankComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<GasTankComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
    }

    private static void OnGetActions(Entity<GasTankComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
    }

    private void OnExamined(Entity<GasTankComponent> ent, ref ExaminedEvent args)
    {
        using var _ = args.PushGroup(nameof(GasTankComponent));
        if (args.IsInDetailsRange)
            args.PushMarkup(Loc.GetString("comp-gas-tank-examine", ("pressure", Math.Round(ent.Comp.Air.Pressure))));

        if (ent.Comp.IsConnected)
            args.PushMarkup(Loc.GetString("comp-gas-tank-connected"));

        args.PushMarkup(Loc.GetString(ent.Comp.IsValveOpen
            ? "comp-gas-tank-examine-open-valve"
            : "comp-gas-tank-examine-closed-valve"));
    }

    private void OnGetAlternativeVerb(Entity<GasTankComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = ent.Comp.IsValveOpen
                ? Loc.GetString("comp-gas-tank-close-valve")
                : Loc.GetString("comp-gas-tank-open-valve"),
            Act = () =>
            {
                ent.Comp.IsValveOpen = !ent.Comp.IsValveOpen;
                Audio.PlayPvs(ent.Comp.ValveSound, ent);
            },
            Disabled = ent.Comp.IsConnected,
        });
    }
}
