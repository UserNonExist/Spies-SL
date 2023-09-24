using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace CiSpy.Items;

public class SpyDetector : CustomItem
{
    public override uint Id { get; set; } = 51;
    public override string Name { get; set; } = "LJ-E-61";
    public override string Description { get; set; } = "เข็มที่สามารถใช้เช็คสปายได้";
    public override float Weight { get; set; } = 0.1f;
    public override SpawnProperties SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Location = SpawnLocationType.InsideNukeArmory,
                Chance = 100
            },
        },
    };
    
    public override ItemType Type { get; set; } = ItemType.Adrenaline;
    
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        if (Entrypoint.Instance.Config.NtfCaptainCanSpawnWithSpyCheck)
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;

        base.SubscribeEvents();
    }
    
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        if (Entrypoint.Instance.Config.NtfCaptainCanSpawnWithSpyCheck)
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        
        base.UnsubscribeEvents();
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.NewRole == RoleTypeId.NtfCaptain)
        {
            if (ev.Reason == SpawnReason.Respawn|| (ev.Reason == SpawnReason.ForceClass && 
                (ev.SpawnFlags == RoleSpawnFlags.AssignInventory || ev.SpawnFlags == RoleSpawnFlags.All)))
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    TryGive(ev.Player, Id);
                });
            }
        }
        
        if (ev.NewRole == RoleTypeId.ChaosMarauder)
        {
            if (ev.Reason == SpawnReason.Respawn || (ev.Reason == SpawnReason.ForceClass && 
                (ev.SpawnFlags == RoleSpawnFlags.AssignInventory || ev.SpawnFlags == RoleSpawnFlags.All)))
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    TryGive(ev.Player, Id);
                });
            }
        }
    }

    public void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;
        
        Player target = GetPlayerAimedAt(ev.Player);

        if (target == null)
        {
            ev.Player.ShowHint("ไม่พบเป้าหมาย", 5f);
            ev.IsAllowed = false;
            return;
        }

        if (target.Role.Type == RoleTypeId.NtfPrivate || target.Role.Type == RoleTypeId.NtfSpecialist ||
            target.Role.Type == RoleTypeId.ChaosConscript ||target.Role.Type == RoleTypeId.ChaosRifleman)
        {
            if (!Entrypoint.EventHandlers.SpyList.Contains(target))
            {
                Timing.CallDelayed(1.5f, () =>
                {
                    ev.Player.ShowHint($"ผลการทดสอบ: ไม่ใช่สปาย\n{target.Nickname}", 5f);
                });
                return;
            }

            if (target.SessionVariables["ShootedAsSpy"].Equals(true))
            {
                Timing.CallDelayed(1.5f, () =>
                {
                    switch (target.Role.Type)
                    {
                        case RoleTypeId.ChaosConscript:
                            ev.Player.ShowHint($"ผลการทดสอบ: {Entrypoint.Instance.Translation.ChaosSpyName}\n{target.Nickname}", 5f);
                            target.SessionVariables["Damagable"] = true;
                            break;
                        case RoleTypeId.NtfSpecialist:
                            ev.Player.ShowHint($"ผลการทดสอบ: {Entrypoint.Instance.Translation.NtfSpyName}\n{target.Nickname}", 5f);
                            target.SessionVariables["Damagable"] = true;
                            break;
                    }
                });
            }
            else
            {
                Timing.CallDelayed(1.5f, () =>
                {
                    switch (target.Role.Type)
                    {
                        case RoleTypeId.NtfPrivate:
                            ev.Player.ShowHint($"ผลการทดสอบ: {Entrypoint.Instance.Translation.ChaosSpyName}\n{target.Nickname}", 5f);
                            Entrypoint.EventHandlers.TurnSpyRole(target);
                            target.SessionVariables["ShootedAsSpy"] = true;
                            target.SessionVariables["Damagable"] = true;
                            break;
                        case RoleTypeId.ChaosRifleman:
                            ev.Player.ShowHint($"ผลการทดสอบ: {Entrypoint.Instance.Translation.NtfSpyName}\n{target.Nickname}", 5f);
                            Entrypoint.EventHandlers.TurnSpyRole(target);
                            target.SessionVariables["ShootedAsSpy"] = true;
                            target.SessionVariables["Damagable"] = true;
                            break;
                    }
                });
            }
        }
        else
        {
            ev.Player.ShowHint("ไม่พบเป้าหมาย", 5f);
            ev.IsAllowed = false;
            return;
        }
        
    }
    
    private Player GetPlayerAimedAt(Player player, float maxDistance = 3f)
    {
        Ray ray = new Ray(player.CameraTransform.position + (player.CameraTransform.forward * 0.1f), player.CameraTransform.forward);
        RaycastHit hitInfo;
        if (!Physics.Raycast(ray, out hitInfo, maxDistance))
            return null;
        var Victim = Player.Get(hitInfo.collider);

        if (Victim == null)
            return null;
        
        if (Victim == player)
            return null;


        return Victim;
    }
}