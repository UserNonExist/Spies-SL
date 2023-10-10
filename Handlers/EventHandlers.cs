using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Permissions.Commands.Permissions.Group;
using MEC;
using PlayerRoles;
using Respawning;
using SpiesSl.Enums;

namespace SpiesSl.Handlers;

public class EventHandlers
{
    public bool ForceSpawning = false;
    
    public void OnRespawningTeam(RespawningTeamEventArgs ev)
    {
        int chance = Entrypoint.Instance.Config.BaseSpawnChance;
        int spySpawning = 0;

        if (chance == -1 && !ForceSpawning)
        {
            Log.Debug("Server owner disabled natural spawning, returning");
            return;
        }

        switch (Entrypoint.Instance.Config.ScaleType)
        {
            case ScaleType.PlayerList:
                chance += (Player.List.Count * Entrypoint.Instance.Config.AdditionalSpawnChance);
                break;
            case ScaleType.RespawningPlayer:
                chance += (ev.Players.Count * Entrypoint.Instance.Config.AdditionalSpawnChance);
                break;
        }
        
        if (chance > Entrypoint.Instance.Config.MaxSpawnChance)
        {
            chance = Entrypoint.Instance.Config.MaxSpawnChance;
        }
        
        if (ForceSpawning)
        {
            chance = 100;
        }

        int rnd = UnityEngine.Random.Range(0, 100);

        if (chance >= rnd)
        {
            Log.Debug("Random number is less than chance, queueing first spy");
            spySpawning++;
        }
        else
        {
            Log.Debug("Random number is greater than chance, returning");
        }

        if (Entrypoint.Instance.Config.SecondSpyBaseChance != -1 && (ev.Players.Count >= Entrypoint.Instance.Config.MinimumPlayerForSecondSpy))
        {
            int secondChance = Entrypoint.Instance.Config.SecondSpyBaseChance;
            
            switch (Entrypoint.Instance.Config.ScaleType)
            {
                case ScaleType.PlayerList:
                    secondChance += (Player.List.Count * Entrypoint.Instance.Config.SecondSpyAdditionalChance);
                    break;
                case ScaleType.RespawningPlayer:
                    secondChance += (ev.Players.Count * Entrypoint.Instance.Config.SecondSpyAdditionalChance);
                    break;
            }
            
            if (secondChance > Entrypoint.Instance.Config.SecondSpyMaxSpawnChance)
            {
                secondChance = Entrypoint.Instance.Config.SecondSpyMaxSpawnChance;
            }

            if (ForceSpawning)
            {
                secondChance = 100;
            }
            
            int secondRnd = UnityEngine.Random.Range(0, 100);
            
            if (secondChance >= secondRnd)
            {
                Log.Debug("Random number is less than second chance, queueing second spy");
                spySpawning++;
            }
            else
            {
                Log.Debug("Random number is greater than second chance, returning");
            }
        }
        
        ForceSpawning = false;
        Log.Debug("Total spies spawning attempt: " + spySpawning);
        
        Timing.CallDelayed(0.1f, () =>
        {
            List<Player> lowRankedPlayers = new List<Player>();

            switch (ev.NextKnownTeam)
            {
                case SpawnableTeamType.NineTailedFox:
                    foreach (Player player in ev.Players)
                    {
                        if (player.Role.Type == RoleTypeId.NtfPrivate)
                        {
                            lowRankedPlayers.Add(player);
                        }
                    }
                    break;
                case SpawnableTeamType.ChaosInsurgency:
                    foreach (Player player in ev.Players)
                    {
                        if (player.Role.Type == RoleTypeId.ChaosRifleman)
                        {
                            lowRankedPlayers.Add(player);
                        }
                    }
                    break;
            }
            
            if (lowRankedPlayers.Count < spySpawning)
            {
                Log.Debug("Not enough low ranked players, attempting to spawn only 1");
                spySpawning = 1;
            }
            
            List<Player> spies = new List<Player>();
            
            for (int i = 0; i < spySpawning; i++)
            {
                Player player = lowRankedPlayers[UnityEngine.Random.Range(0, lowRankedPlayers.Count)];
                spies.Add(player);
                Log.Debug("Appointing " + player.Nickname + " as spy candidate");
                lowRankedPlayers.Remove(player);
            }
            
            bool multipleSpies = spies.Count > 1;

            switch (ev.NextKnownTeam)
            {
                case SpawnableTeamType.NineTailedFox:
                    foreach (Player player in spies)
                    {
                        Log.Debug("Attempting to spawn " + player.Nickname + " as Chaos spy");
                        Entrypoint.SpyHandlers.SpawnChaosSpy(player, multipleSpies);
                        if (multipleSpies)
                        {
                            Player otherSpy = spies.Find(x => x != player);
                            string additionalMsg = Entrypoint.Instance.Translation.MultipleSpyAdditionalMessage.Replace("%player%", otherSpy.Nickname);
                            string msg = Entrypoint.Instance.Translation.ChaosSpySpawnMessage.Replace("%player%", player.Nickname).Replace("%multiplePlayer%", additionalMsg);
                            player.ShowHint(msg, Entrypoint.Instance.Config.SpawnMessageDuration);
                        }
                    }
                    break;
                case SpawnableTeamType.ChaosInsurgency:
                    foreach (Player player in spies)
                    {
                        Log.Debug("Attempting to spawn " + player.Nickname + " as NTF spy");
                        Entrypoint.SpyHandlers.SpawnNtfSpy(player, multipleSpies);
                        if (multipleSpies)
                        {
                            Player otherSpy = spies.Find(x => x != player);
                            string additionalMsg = Entrypoint.Instance.Translation.MultipleSpyAdditionalMessage.Replace("%player%", otherSpy.Nickname);
                            string msg = Entrypoint.Instance.Translation.NtfSpySpawnMessage.Replace("%player%", player.Nickname).Replace("%multiplePlayer%", additionalMsg);
                            player.ShowHint(msg, Entrypoint.Instance.Config.SpawnMessageDuration);
                        }
                    }
                    break;
            }
        });
    }

    public void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
    {
        if (ev.Player == null || ev.OldTarget == null || ev.NewTarget == null)
        {
            return;
        }

        if (Entrypoint.SpyHandlers.IsSpy(ev.OldTarget) && !Entrypoint.SpyHandlers.IsSpy(ev.NewTarget))
        {
            ev.Player.ClearBroadcasts();
        }

        else if (Entrypoint.SpyHandlers.IsSpy(ev.NewTarget))
        {
            switch (Entrypoint.SpyHandlers.GetSpyType(ev.NewTarget))
            {
                case SpyType.ChaosSpy:
                    ev.Player.ClearBroadcasts();
                    ev.Player.Broadcast(3, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation.ChaosSpyName));
                    return;
                case SpyType.NtfSpy:
                    ev.Player.ClearBroadcasts();
                    ev.Player.Broadcast(3, Entrypoint.Instance.Translation.SpectatingSpyMessage.Replace("%role%", Entrypoint.Instance.Translation.NtfSpyName));
                    return;
            }
        }
    }
    
    public void OnVerified(VerifiedEventArgs ev)
    {
        ev.Player.SessionVariables["Undetectable"] = false;
        ev.Player.SessionVariables["Vulnerable"] = true;
    }

    public void OnLeft(LeftEventArgs ev)
    {
        if (ev.Player == null)
        {
            return;
        }
        
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player))
        {
            Entrypoint.SpyHandlers.Spies.Remove(ev.Player);
        }
    }
    
    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player) && ev.Reason == SpawnReason.ForceClass)
        {
            Entrypoint.SpyHandlers.Spies.Remove(ev.Player);
        }
    }
    
    public void OnDying(DyingEventArgs ev)
    {
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player))
        {
            Entrypoint.SpyHandlers.Spies.Remove(ev.Player);
            
            switch (Entrypoint.SpyHandlers.GetSpyType(ev.Player))
            {
                case SpyType.ChaosSpy:
                    ev.Player.Role.Set(RoleTypeId.ChaosConscript, SpawnReason.None, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
                case SpyType.NtfSpy:
                    ev.Player.Role.Set(RoleTypeId.NtfSpecialist, SpawnReason.None, RoleSpawnFlags.None);
                    ev.Player.Kill(ev.DamageHandler);
                    break;
            }
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
        
        bool attackerIsSpy = Entrypoint.SpyHandlers.IsSpy(ev.Attacker);
        bool victimIsSpy = Entrypoint.SpyHandlers.IsSpy(ev.Player);
        SpyType? victimSpyType = Entrypoint.SpyHandlers.GetSpyType(ev.Player);
        bool attackerVulnerable = ev.Attacker.SessionVariables["Vulnerable"] is bool vulnerable && vulnerable;
        bool victimVulnerable = ev.Player.SessionVariables["Vulnerable"] is bool vulnerable2 && vulnerable2;
        bool victimUndetectable = ev.Player.SessionVariables["Undetectable"] is bool undetectable && undetectable;
        
        if (attackerIsSpy && !attackerVulnerable)
        {
            ev.Attacker.SessionVariables["Vulnerable"] = true;
            return;
        }
        
        if (victimIsSpy && !victimVulnerable)
        {
            ev.IsAllowed = false;
            return;
        }

        if (victimIsSpy && victimUndetectable)
        {
            switch (victimSpyType)
            {
                case SpyType.ChaosSpy:
                    if (ev.Attacker.Role.Side == Side.ChaosInsurgency)
                    {
                        ev.Attacker.ShowHint(Entrypoint.Instance.Translation.FriendlyHurtSpyMessage);
                        ev.IsAllowed = false;
                    }
                    break;
                case SpyType.NtfSpy:
                    if (ev.Attacker.Role.Side == Side.Mtf)
                    {
                        ev.Attacker.ShowHint(Entrypoint.Instance.Translation.FriendlyHurtSpyMessage);
                        ev.IsAllowed = false;
                    }
                    break;
            }
            return;
        }
    }

    public void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player == null)
            return;
        
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player) && ev.Player.SessionVariables["Undetectable"] is bool undetectable && undetectable)
        {
            ev.Player.SessionVariables["Undetectable"] = false;
            Entrypoint.SpyHandlers.TurnSpyRole(ev.Player);
        }
    }

    public void OnThrownProjectile(ThrownProjectileEventArgs ev)
    {
        if (ev.Player == null)
            return;
        
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player) && (ev.Player.SessionVariables["Undetectable"] is bool undetectable && undetectable))
        {
            ev.Player.SessionVariables["Undetectable"] = false;
            Entrypoint.SpyHandlers.TurnSpyRole(ev.Player);
            Timing.CallDelayed(0.5f, () => ev.Projectile.PreviousOwner = ev.Player);
        }
        
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player) && (ev.Player.SessionVariables["Vulnerable"] is bool vulnerable && !vulnerable))
        {
            ev.Player.SessionVariables["Vulnerable"] = true;
        }
    }

    public void OnHandcuffing(HandcuffingEventArgs ev)
    {
        if (Entrypoint.SpyHandlers.IsSpy(ev.Player))
        {
            ev.Player.ShowHint(Entrypoint.Instance.Translation.CannotCuffMessage, 5f);
            ev.IsAllowed = false;
        }

        if (Entrypoint.SpyHandlers.IsSpy(ev.Target))
        {
            switch (Entrypoint.SpyHandlers.GetSpyType(ev.Target))
            {
                case SpyType.ChaosSpy:
                    if (ev.Player.Role.Side == Side.ChaosInsurgency)
                    {
                        ev.Player.ShowHint(Entrypoint.Instance.Translation.FriendlyCuffSpyMessage, 5f);
                        ev.IsAllowed = false;
                    }
                    break;
                case SpyType.NtfSpy:
                    if (ev.Player.Role.Side == Side.Mtf)
                    {
                        ev.Player.ShowHint(Entrypoint.Instance.Translation.FriendlyCuffSpyMessage, 5f);
                        ev.IsAllowed = false;
                    }
                    break;
            }
        }
    }
    
    public void OnRestartingRound()
    {
        foreach (var cHandle in Entrypoint.SpyHandlers.CoroutineHandles)
        {
            Timing.KillCoroutines(cHandle);
        }
        Entrypoint.SpyHandlers.Spies.Clear();
        Entrypoint.SpyHandlers.CoroutineHandles.Clear();
    }
}