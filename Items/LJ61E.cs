using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using SpiesSl.Enums;
using UnityEngine;

namespace SpiesSl.Items;

public class LJ61E : CustomItem
{
    public override ItemType Type { get; set; } = ItemType.Adrenaline;
    public override uint Id { get; set; } = 61;
    public override string Name { get; set; } = "LJ-61-E";
    public override string Description { get; set; } = "An electronic injection that can be used to check player loyalty.";
    public override float Weight { get; set; } = 1f;
    public string NoTargetFound { get; set; } = "No target found";
    public string ResultMessage { get; set; } = "Scan result: %result%\n%player%";
    public string TargetMessage { get; set; } = "You have been scanned by %player%";
    public string NotSpyResult { get; set; } = "Not a spy";
    public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
    {
        Limit = 2,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new DynamicSpawnPoint
            {
                Chance = 50,
                Location = SpawnLocationType.InsideEscapePrimary,
            },
            new DynamicSpawnPoint
            {
                Chance = 50,
                Location = SpawnLocationType.InsideSurfaceNuke,
            }
        }
    };
    public List<RoleTypeId> RoleSpawningWithItem { get; set; } = new List<RoleTypeId>
    {
        RoleTypeId.ChaosRepressor,
        RoleTypeId.NtfCaptain
    };
    
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        if (RoleSpawningWithItem.Count > 0)
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        base.SubscribeEvents();
    }
    
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        if (RoleSpawningWithItem.Count > 0)
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        base.UnsubscribeEvents();
    }

    internal void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.SpawnFlags == RoleSpawnFlags.None || ev.SpawnFlags == RoleSpawnFlags.UseSpawnpoint)
            return;

        if (RoleSpawningWithItem.Contains(ev.NewRole))
        {
            Timing.CallDelayed(0.5f, () =>
            {
                TryGive(ev.Player, Id, false);
            });
        }
    }
    
    internal void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Item))
            return;

        var forward = ev.Player.CameraTransform.forward;
        Ray ray = new Ray(ev.Player.CameraTransform.position + (forward * 0.1f), forward);
        
        RaycastHit hit;
        
        if (!Physics.Raycast(ray, out hit, 3f))
        {
            ev.Player.ShowHint(NoTargetFound, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        Player target = Player.Get(hit.collider);
        
        if (target == null || target == ev.Player)
        {
            ev.Player.ShowHint(NoTargetFound, 5f);
            ev.IsAllowed = false;
            return;
        }
        
        target.ShowHint(TargetMessage.Replace("%player%", ev.Player.Nickname), 5f);
        
        Timing.CallDelayed(1.5f, () =>
        {
            switch (Entrypoint.SpyHandlers.GetSpyType(target))
            {
                case SpyType.ChaosSpy:
                    ev.Player.ShowHint(ResultMessage.Replace("%result%", Entrypoint.Instance.Translation.ChaosSpyName)
                        .Replace("%player%", target.Nickname), 5f);
                    if (target.SessionVariables["Undetectable"] is bool undetectable && undetectable)
                    {
                        Entrypoint.SpyHandlers.TurnSpyRole(target);
                        target.SessionVariables["Undetectable"] = false;
                    }
                    target.SessionVariables["Vulnerable"] = true;
                    break;
                case SpyType.NtfSpy:
                    ev.Player.ShowHint(ResultMessage.Replace("%result%", Entrypoint.Instance.Translation.NtfSpyName)
                        .Replace("%player%", target.Nickname), 5f);
                    if (target.SessionVariables["Undetectable"] is bool undetectable2 && undetectable2)
                    {
                        Entrypoint.SpyHandlers.TurnSpyRole(target);
                        target.SessionVariables["Undetectable"] = false;
                    }
                    target.SessionVariables["Vulnerable"] = true;
                    break;
                default:
                    ev.Player.ShowHint(ResultMessage.Replace("%result%", NotSpyResult)
                        .Replace("%player%", target.Nickname), 5f);
                    break;
            }
        });
    }
}