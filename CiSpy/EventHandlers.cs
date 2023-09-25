using System;
using System.Collections.Generic;
using System.Linq;
using CiSpy.Enums;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Core.Generic;
using Exiled.API.Features.DamageHandlers;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Firearms.BasicMessages;
using JetBrains.Annotations;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Events;
using Respawning;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CiSpy;

public class EventHandlers
{
    public List<CoroutineHandle> CoroutineHandles = new List<CoroutineHandle>();
    public List<Player> SpyList = new List<Player>();
    public List<Player> NtfSpyList = new List<Player>();
    public List<Player> ChaosSpyList = new List<Player>();

    public void OnRoundRestart()
    {
        foreach (var cHandles in CoroutineHandles)
        {
            Timing.KillCoroutines(cHandles);
        }
        
        CoroutineHandles.Clear();
        SpyList.Clear();
        NtfSpyList.Clear();
        ChaosSpyList.Clear();
    }

    public void OnLeft(LeftEventArgs ev)
    {
        Timing.CallDelayed(0.5f, () =>
        {
            if (SpyList.Contains(ev.Player))
            {
                SpyList.Remove(ev.Player);
                ev.Player.SessionVariables["CancellationToken"] = true;
            }
        
            if (NtfSpyList.Contains(ev.Player))
            {
                NtfSpyList.Remove(ev.Player);
            }
        
            if (ChaosSpyList.Contains(ev.Player))
            {
                ChaosSpyList.Remove(ev.Player);
            }
        });
        
        
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        try
        {
            if (ev.Player.SessionVariables["DoNotUpdate"] is bool DoNotUpdate && DoNotUpdate)
            {
                ev.Player.SessionVariables["DoNotUpdate"] = false;
                return;
            }

            if (ev.Player.SessionVariables["IsSpy"] is bool IsSpy && IsSpy)
            {
                ev.Player.SessionVariables["IsSpy"] = false;
                ev.Player.SessionVariables["Damagable"] = true;
                ev.Player.SessionVariables["ShootedAsSpy"] = false;
                ev.Player.SessionVariables["CancellationToken"] = true;
                SpyList.Remove(ev.Player);
                if (NtfSpyList.Contains(ev.Player))
                {
                    NtfSpyList.Remove(ev.Player);
                }
                if (ChaosSpyList.Contains(ev.Player))
                {
                    ChaosSpyList.Remove(ev.Player);
                }
            }
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
    public void OnVerified(VerifiedEventArgs ev)
    {
        ev.Player.SessionVariables["IsSpy"] = false;
        ev.Player.SessionVariables["Damagable"] = true;
        ev.Player.SessionVariables["ShootedAsSpy"] = false;
        ev.Player.SessionVariables["CancellationToken"] = false;
        ev.Player.SessionVariables["DoNotUpdate"] = false;
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
        player.SessionVariables["DoNotUpdate"] = true;
        SpyList.Add(player);
        ChaosSpyList.Add(player);
        player.SessionVariables["IsSpy"] = true;
        CoroutineHandles.Add(Timing.RunCoroutine(DamagableTimer(player, Entrypoint.Instance.Config.UnDamagableDuration)));
        CoroutineHandles.Add(Timing.RunCoroutine(ShootedSpyTimer(player, Entrypoint.Instance.Config.SpyCheckDuration)));
        player.Role.Set(RoleTypeId.NtfPrivate, RoleSpawnFlags.None);
        player.ShowHint(Entrypoint.Instance.Translation.ChaosSpySpawnMessage, Entrypoint.Instance.Config.SpawnMessageDuration);
        Log.Debug($"Spawned {player.Nickname} as Chaos Spy");
    }
    
    public void SpawnNtfSpy(Player player)
    {
        player.SessionVariables["DoNotUpdate"] = true;
        SpyList.Add(player);
        NtfSpyList.Add(player);
        player.SessionVariables["IsSpy"] = true;
        CoroutineHandles.Add(Timing.RunCoroutine(DamagableTimer(player, Entrypoint.Instance.Config.UnDamagableDuration)));
        CoroutineHandles.Add(Timing.RunCoroutine(ShootedSpyTimer(player, Entrypoint.Instance.Config.SpyCheckDuration)));
        player.Role.Set(RoleTypeId.ChaosRifleman, RoleSpawnFlags.None);
        player.ShowHint(Entrypoint.Instance.Translation.MtfSpySpawnMessage, Entrypoint.Instance.Config.SpawnMessageDuration);
        Log.Debug($"Spawned {player.Nickname} as NTF Spy");
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && !isSpy)
            return;
        
        ev.Player.SessionVariables["IsSpy"] = false;
        ev.Player.SessionVariables["DoNotUpdate"] = true;

        if (ev.Player.SessionVariables["ShootedAsSpy"] is bool shotedAsSpy && !shotedAsSpy)
        {
            switch (ev.Player.Role.Type)
            {
                case RoleTypeId.NtfPrivate:
                    ev.Player.Role.Set(RoleTypeId.ChaosConscript, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
                case RoleTypeId.ChaosRifleman:
                    ev.Player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
            }
        }
        else
        {
            switch (ev.Player.Role.Type)
            {
                case RoleTypeId.ChaosConscript:
                    ev.Player.Role.Set(RoleTypeId.ChaosConscript, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
                case RoleTypeId.NtfSpecialist:
                    ev.Player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
            }
        }
        
        ev.Player.SessionVariables["ShootedAsSpy"] = false;
        ev.Player.SessionVariables["Damagable"] = true;
        SpyList.Remove(ev.Player);
        
        if (NtfSpyList.Contains(ev.Player))
        {
            NtfSpyList.Remove(ev.Player);
        }
        if (ChaosSpyList.Contains(ev.Player))
        {
            ChaosSpyList.Remove(ev.Player);
        }
    }

    public void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player.IsScp)
            return;
        
        
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && !isSpy)
            return;

        if (ev.Player.SessionVariables["ShootedAsSpy"] is bool shooted && shooted)
            return;

        Player player = ev.Player;
        
        if (player == null)
            return;
        
        TurnSpyRole(player);
        player.SessionVariables["ShootedAsSpy"] = true;
    }

    public void TurnSpyRole(Player player)
    {
        player.SessionVariables["DoNotUpdate"] = true;
        Vector3 lastPos = player.Position;
        float lastHealth = player.Health;
        float lastMaxHealth = player.MaxHealth;
        float lastArtificialHealth = player.ArtificialHealth;
        float lastMaxArtificialHealth = player.MaxArtificialHealth;
        float lastStamina = player.Stamina;
        
        
        Quaternion lastRot = player.Rotation;
        Quaternion lastPlayerCamRot = player.CameraTransform.rotation;
        
        switch (player.Role.Type)
        {
            case RoleTypeId.NtfPrivate:
                player.Role.Set(RoleTypeId.ChaosConscript, RoleSpawnFlags.None);
                player.Position = lastPos;
                Timing.CallDelayed(0.1f, () =>
                {
                    player.ChangeAppearance(RoleTypeId.NtfPrivate);
                    player.Rotation = lastRot;
                    player.CameraTransform.rotation = lastPlayerCamRot;
                });
                break;
            
            case RoleTypeId.ChaosRifleman:
                player.Role.Set(RoleTypeId.NtfSpecialist, RoleSpawnFlags.None);
                player.Position = lastPos;
                Timing.CallDelayed(0.1f, () =>
                {
                    player.ChangeAppearance(RoleTypeId.ChaosRifleman);
                    player.Rotation = lastRot;
                    player.CameraTransform.rotation = lastPlayerCamRot;
                });
                break;
        }
        
        player.Health = lastHealth;
        player.MaxHealth = lastMaxHealth;
        player.ArtificialHealth = lastArtificialHealth;
        player.MaxArtificialHealth = lastMaxArtificialHealth;
        player.Stamina = lastStamina;
    }

    public void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
    {
        if (ev.NewTarget == null || ev.Player == null)
            return;
        
        if (ev.NewTarget.SessionVariables["IsSpy"] is bool isSpy && isSpy)
        {
            if (ev.NewTarget.SessionVariables["ShootedAsSpy"] is bool shooted && shooted)
            {
                switch (ev.NewTarget.Role.Type)
                {
                    case RoleTypeId.ChaosConscript:
                        ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(2, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation. ChaosSpyName));
                        break;
                    case RoleTypeId.NtfSpecialist:
                        ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(2, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation. NtfSpyName));
                        break;
                }
            }
            else
            {
                switch (ev.NewTarget.Role.Type)
                {
                    case RoleTypeId.ChaosRifleman:
                        ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(2, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation. NtfSpyName));
                        break;
                    case RoleTypeId.NtfPrivate:
                        ev.Player.ClearBroadcasts();
                        ev.Player.Broadcast(2, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation. ChaosSpyName));
                        break;
                }
            }
        }
    }
    
    public void OnHandcuffing(HandcuffingEventArgs ev)
    {
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && isSpy)
        {
            ev.Player.ShowHint(Entrypoint.Instance.Translation.CannotCuffMessage, 5f);
            ev.IsAllowed = false;
        }
        
        if (ev.Target.SessionVariables["IsSpy"] is bool isSpy2 && isSpy2 && ev.Target.SessionVariables["ShootedAsSpy"] is bool shooted && !shooted)
        {
            if (ev.Player.Role.Side != ev.Target.Role.Side)
            {
                ev.Player.ShowHint(Entrypoint.Instance.Translation.FriendlyCuffSpyMessage, 5f);
                ev.IsAllowed = false;
            }
        }
    }

    public void OnThrownProjectile(ThrownProjectileEventArgs ev)
    {
        if (ev.Player.SessionVariables["IsSpy"] is bool isSpy && !isSpy)
            return;
        

        if (ev.Player.SessionVariables["ShootedAsSpy"] is bool shooted && !shooted)
        {
            TurnSpyRole(ev.Player);
            ev.Player.SessionVariables["ShootedAsSpy"] = true;
            Timing.CallDelayed(0.2f, () =>
            {
                ev.Projectile.PreviousOwner = ev.Player;
            });
        }
        
        if (ev.Player.SessionVariables["Damagable"] is bool damagable && !damagable)
        {
            ev.Player.SessionVariables["Damagable"] = true;
        }
    }

    public void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Attacker == null)
            return;
        
        if (ev.DamageHandler.IsSuicide)
            return;
        
        if (ev.Attacker.IsScp || ev.Player.IsScp)
            return;
        
        bool attackerIsSpy = ev.Attacker.SessionVariables["IsSpy"] is bool isSpy && isSpy;
        bool victimIsSpy = ev.Player.SessionVariables["IsSpy"] is bool isSpy2 && isSpy2;
        bool attackerDamagable = ev.Attacker.SessionVariables["Damagable"] is bool damagable && damagable;
        bool victimDamagable = ev.Player.SessionVariables["Damagable"] is bool damagable2 && damagable2;
        bool victimShootedAsSpy = ev.Player.SessionVariables["ShootedAsSpy"] is bool shootedAsSpy && shootedAsSpy;
        

        if (attackerIsSpy && !attackerDamagable)
        {
            ev.Attacker.SessionVariables["Damagable"] = true;
            return;
        }

        if (victimIsSpy && !victimShootedAsSpy)
        {
            switch (ev.Player.Role.Type)
            {
                case RoleTypeId.NtfPrivate:
                    if (ev.Attacker.Role.Side == Side.ChaosInsurgency)
                    {
                        ev.Attacker.ShowHint(Entrypoint.Instance.Translation.FriendlyHurtSpyMessage, 5f);
                        ev.IsAllowed = false;
                    }
                    break;
                case RoleTypeId.ChaosRifleman:
                    if (ev.Attacker.Role.Side == Side.Mtf)
                    {
                        ev.Attacker.ShowHint(Entrypoint.Instance.Translation.FriendlyHurtSpyMessage, 5f);
                        ev.IsAllowed = false;
                    }
                    break;
            }
            return;
            
        }
        
        if (victimIsSpy && !victimDamagable)
        {
            ev.IsAllowed = false;
            return;
        }
    }
    
    public IEnumerator<float> DamagableTimer(Player player, int time)
    {
        player.SessionVariables["Damagable"] = false;
        
        while (time > 0)
        {
            if (player.SessionVariables["CancellationToken"] is bool token && token)
                yield break;
            
            if (player.SessionVariables["Damagable"] is bool isDamagable && isDamagable)
            {
                player.ShowHint(Entrypoint.Instance.Translation.NowDamagableMessage, 5f);
                yield break;
            }
            
            if (player.IsDead)
                yield break;
            
            time--;
            yield return Timing.WaitForSeconds(1f);
        }
        
        player.ShowHint(Entrypoint.Instance.Translation.NowDamagableMessage, 5f);
        player.SessionVariables["Damagable"] = true;
    }
    
    public IEnumerator<float> ShootedSpyTimer(Player player, int time)
    {
        player.SessionVariables["ShootedAsSpy"] = false;

        while (time > 0)
        {
            if (player.SessionVariables["CancellationToken"] is bool token && token)
            {
                player.SessionVariables["CancellationToken"] = false;
                yield break;
            }
            
            
            if (player.SessionVariables["ShootedAsSpy"] is bool shootedAsSpy && shootedAsSpy)
            {
                player.Broadcast(5, Entrypoint.Instance.Translation.FirstShootMessage);
                yield break;
            }
            
            if (player.IsDead)
                yield break;
            
            if (time < (Entrypoint.Instance.Config.SpyCheckDuration - Entrypoint.Instance.Config.SpawnMessageDuration))
                player.ShowHint(Entrypoint.Instance.Translation.SpawnProtectMessage.Replace("%time%", time.ToString()), 1f);

            time--;
            yield return Timing.WaitForSeconds(1f);
        }
        
        TurnSpyRole(player);
        player.Broadcast(5, Entrypoint.Instance.Translation.FirstShootMessage);
        player.SessionVariables["ShootedAsSpy"] = true;
    }
}