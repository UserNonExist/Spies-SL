using System.Collections.Generic;
using System.Linq;
using CiSpy.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning;
using UnityEngine;

namespace CiSpy;

public class EventHandlers
{
    public List<Player> ChaosSpies = new List<Player>();
    public List<Player> MtfSpies = new List<Player>();
    public List<CoroutineHandle> CoroutineHandles = new List<CoroutineHandle>();

    public bool ShowHitmarker;


    public void OnRoundRestart()
    {
        Server.FriendlyFire = false;
        ShowHitmarker = false;
        ChaosSpies.Clear();
        MtfSpies.Clear();
        
        foreach (var cHandles in CoroutineHandles)
        {
            Timing.KillCoroutines(cHandles);
            CoroutineHandles.Remove(cHandles);
        }
    }
    
    public void OnRoundStarted()
    {
        ShowHitmarker = false;
    }
    
    public void OnVerified(VerifiedEventArgs ev)
    {
        ev.Player.SessionVariables["IsSpy"] = false;
        ev.Player.SessionVariables["Damagable"] = true;
    }
    
    public void OnRespawningTeam(RespawningTeamEventArgs ev)
    {
        int randomChance = Random.Range(1, 100);

        int threasholdChance = Entrypoint.Instance.Config.BaseSpawnChance;
        
        switch (Entrypoint.Instance.Config.ScaleType)
        {
            case ScaleType.ScaleByPlayer:
                threasholdChance += Entrypoint.Instance.Config.AdditionalSpawnChance * Player.List.Count;
                break;
            case ScaleType.ScaleByRespawn:
                threasholdChance += Entrypoint.Instance.Config.AdditionalSpawnChance * ev.Players.Count;
                break;
            default:
                break;
        }
        
        if (randomChance > threasholdChance)
        {
            Log.Debug("Rolled above threashold, not spawning spy");
            return;
        }
        
        Log.Debug("Check passed, spawning spy");
        
        List<Player> lowRankPlayers = new List<Player>();
        
        if (ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (var player in ev.Players)
                {
                    if (player.Role != RoleTypeId.ChaosRifleman)
                        continue;
                    
                    lowRankPlayers.Add(player);
                }
                
                if (lowRankPlayers.Count == 0)
                    return;
                
                lowRankPlayers.ShuffleList();
                
                SpawnNtfSpy(lowRankPlayers[Random.Range(0, lowRankPlayers.Count-1)]);
            });
        }
        else if (ev.NextKnownTeam == SpawnableTeamType.NineTailedFox)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                foreach (var player in ev.Players)
                {
                    if (player.Role != RoleTypeId.NtfPrivate)
                        continue;
                    
                    lowRankPlayers.Add(player);
                }
                
                if (lowRankPlayers.Count == 0)
                    return;
                
                lowRankPlayers.ShuffleList();
                
                SpawnChaosSpy(lowRankPlayers[Random.Range(0, lowRankPlayers.Count-1)]);
            });
        }
    }

    public void SpawnChaosSpy(Player player)
    {
        CoroutineHandles.Add(Timing.RunCoroutine(HitmarkerTimer(player, Entrypoint.Instance.Config.HitmarkerDuration)));
        player.SessionVariables["IsSpy"] = true;
        CoroutineHandles.Add(Timing.RunCoroutine(DamagableTimer(player, Entrypoint.Instance.Config.UnDamagableDuration)));
        player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.None);
        Timing.CallDelayed(0.1f, () =>
        {
            player.ChangeAppearance(RoleTypeId.NtfPrivate);
            player.ShowHint(Entrypoint.Instance.Translation.ChaosSpySpawnMessage, Entrypoint.Instance.Config.SpawnMessageDuration);
        });
    }
    
    public void SpawnNtfSpy(Player player)
    {
        CoroutineHandles.Add(Timing.RunCoroutine(HitmarkerTimer(player, Entrypoint.Instance.Config.HitmarkerDuration)));
        player.SessionVariables["IsSpy"] = true;
        CoroutineHandles.Add(Timing.RunCoroutine(DamagableTimer(player, Entrypoint.Instance.Config.UnDamagableDuration)));
        player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
        Timing.CallDelayed(0.1f, () =>
        {
            player.ChangeAppearance(RoleTypeId.ChaosRifleman);
            player.ShowHint(Entrypoint.Instance.Translation.MtfSpySpawnMessage, Entrypoint.Instance.Config.SpawnMessageDuration);
            
        });
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && !isSpy)
            return;
        
        ev.Player.SessionVariables["IsSpy"] = false;
        ev.Player.SessionVariables["Damagable"] = true;

        switch (ev.Player.Role.Type)
        {
            case RoleTypeId.ChaosRifleman:
                ev.Player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.None);
                ev.Player.Kill(ev.DamageHandler);
                break;
            case RoleTypeId.NtfSpecialist:
                ev.Player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                ev.Player.Kill(ev.DamageHandler);
                break;
        }
    }

    public void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
    {
        if (ev.NewTarget == null || ev.Player == null)
            return;
        
        if (ev.NewTarget.SessionVariables["IsSpy"] is bool isSpy && isSpy)
        {
            ev.Player.ClearBroadcasts();
            ev.Player.Broadcast(2, Entrypoint.Instance.Translation.SpectatingSpyMessage);
        }
    }
    
    public void OnHandcuffing(HandcuffingEventArgs ev)
    {
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && isSpy)
        {
            ev.Player.ShowHint(Entrypoint.Instance.Translation.CannotCuffMessage);
            ev.IsAllowed = false;
        }
    }
    
    public void OnShooting(ShootingEventArgs ev)
    {
        if (!ShowHitmarker)
            return;
        
        if (ev.Player.IsScp)
            return;
        
        ev.Player.ShowHitMarker();
    }

    public void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Attacker == null)
            return;
        
        if (ev.DamageHandler.IsSuicide)
            return;
        
        if (ev.Attacker.IsScp || ev.Player.IsScp)
            return;

        if (ev.Attacker.Role.Side == ev.Player.Role.Side && ShowHitmarker)
        {
            ev.IsAllowed = false;
        }
        
        bool attackerIsSpy = ev.Attacker.SessionVariables["IsSpy"] is bool isSpy && isSpy;
        bool victimIsSpy = ev.Player.SessionVariables["IsSpy"] is bool isSpy2 && isSpy2;
        bool attackerDamagable = ev.Attacker.SessionVariables["Damagable"] is bool damagable && damagable;
        bool victimDamagable = ev.Player.SessionVariables["Damagable"] is bool damagable2 && damagable2;

        if (attackerIsSpy && !attackerDamagable)
        {
            ev.Attacker.SessionVariables["Damagable"] = true;
            return;
        }
        
        if (victimIsSpy && !victimDamagable)
        {
            ev.IsAllowed = false;
            return;
        }
    }

    public IEnumerator<float> HitmarkerTimer(Player player, int time)
    {
        
        ShowHitmarker = true;
        
        while (time > 0)
        {
            if (player.SessionVariables["Damagable"] is bool isDamagable && isDamagable)
            {
                ShowHitmarker = false;
                yield break;
            }

            time--;
            yield return Timing.WaitForSeconds(1f);
        }
        
        ShowHitmarker = false;
    }
    
    public IEnumerator<float> DamagableTimer(Player player, int time)
    {
        player.SessionVariables["Damagable"] = false;
        
        while (time > 0)
        {
            if (player.SessionVariables["Damagable"] is bool isDamagable && isDamagable)
            {
                player.ShowHint(Entrypoint.Instance.Translation.NowDamagableMessage, 5f);
                yield break;
            }
            time--;
            yield return Timing.WaitForSeconds(1f);
        }
        
        player.ShowHint(Entrypoint.Instance.Translation.NowDamagableMessage, 5f);
        player.SessionVariables["Damagable"] = true;
    }
}