using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using PlayerRoles;
using SpiesSl.Enums;
using UnityEngine;
namespace SpiesSl.Handlers;

public class SpyHandlers
{
    public Dictionary<Player, SpyType> Spies = new Dictionary<Player, SpyType>();

    public List<CoroutineHandle> CoroutineHandles = new List<CoroutineHandle>();
    
    public bool IsSpy(Player player)
    {
        if (player == null)
            return false;
        
        try
        {
            return Spies.ContainsKey(player);
        }
        catch (Exception)
        {
            return false;
            throw;
        }
    }
    
    public SpyType? GetSpyType(Player player)
    {
        try
        {
            if (!IsSpy(player))
            {
                return null;
            }
            return Spies[player];
        }
        catch (Exception)
        {
            return null;
            throw;
        }
    }
    
    public void SpawnChaosSpy(Player player, bool multiple = false)
    {
        player.Role.Set(RoleTypeId.NtfPrivate, SpawnReason.None, RoleSpawnFlags.None);
        
        Spies.Add(player, SpyType.ChaosSpy);
        
        CoroutineHandles.Add(Timing.RunCoroutine(TimerCoroutine(player)));
        
        if (!multiple)
        {
            player.ShowHint(Entrypoint.Instance.Translation.ChaosSpySpawnMessage.Replace("%multiplePlayer%", ""), Entrypoint.Instance.Config.SpawnMessageDuration);
        }
        
        Log.Debug($"[SPY] spawned {player.Nickname} as a Chaos spy.");
    }
    
    public void SpawnNtfSpy(Player player, bool multiple = false)
    {
        player.Role.Set(RoleTypeId.ChaosRifleman, SpawnReason.None, RoleSpawnFlags.None);
        
        Spies.Add(player, SpyType.NtfSpy);
        
        CoroutineHandles.Add(Timing.RunCoroutine(TimerCoroutine(player)));

        if (!multiple)
        {
            player.ShowHint(Entrypoint.Instance.Translation.NtfSpySpawnMessage.Replace("%multiplePlayer%", ""), Entrypoint.Instance.Config.SpawnMessageDuration);
        }
        
        Log.Debug($"[SPY] spawned {player.Nickname} as a NTF spy.");
    }

    public void TurnSpyRole(Player player)
    {
        Vector3 lastPos = player.Position;
        Quaternion lastRot = player.Rotation;
        Quaternion lastPlayerCamRot = player.CameraTransform.rotation;
        float lastHealth = player.Health;
        float lastMaxHealth = player.MaxHealth;
        float lastArtificialHealth = player.ArtificialHealth;
        float lastMaxArtificialHealth = player.MaxArtificialHealth;
        float lastStamina = player.Stamina;
        
        Item lastItem = player.CurrentItem;

        switch (Spies[player])
        {
            case SpyType.ChaosSpy:
                player.Role.Set(RoleTypeId.ChaosConscript, SpawnReason.None, RoleSpawnFlags.None);
                player.Position = lastPos;
                Timing.CallDelayed(0.1f, () =>
                {
                    player.ChangeAppearance(RoleTypeId.NtfPrivate);
                    player.Health = lastHealth;
                    player.MaxHealth = lastMaxHealth;
                    player.ArtificialHealth = lastArtificialHealth;
                    player.MaxArtificialHealth = lastMaxArtificialHealth;
                    player.Stamina = lastStamina;
                    player.Rotation = lastRot;
                    player.CameraTransform.rotation = lastPlayerCamRot;
                    player.CurrentItem = null;
                });
                break;
            case SpyType.NtfSpy:
                player.Role.Set(RoleTypeId.NtfSpecialist, SpawnReason.None, RoleSpawnFlags.None);
                player.Position = lastPos;
                Timing.CallDelayed(0.1f, () =>
                {
                    player.ChangeAppearance(RoleTypeId.ChaosRifleman);
                    player.Health = lastHealth;
                    player.MaxHealth = lastMaxHealth;
                    player.ArtificialHealth = lastArtificialHealth;
                    player.MaxArtificialHealth = lastMaxArtificialHealth;
                    player.Stamina = lastStamina;
                    player.Rotation = lastRot;
                    player.CameraTransform.rotation = lastPlayerCamRot;
                    player.CurrentItem = null;
                });
                break;
        }
        
        Timing.CallDelayed(0.7f, () =>
        {
            player.CurrentItem = lastItem;
            player.IsSpawnProtected = false;
            CoroutineHandles.Add(Timing.RunCoroutine(AppearanceCoroutine(player)));
        });
    }

    public IEnumerator<float> TimerCoroutine(Player player)
    {
        player.SessionVariables["Undetectable"] = true;
        player.SessionVariables["Vulnerable"] = false;
        
        int undetectTimer = Entrypoint.Instance.Config.UndetectableDuration;
        int vulnerableTimer = Entrypoint.Instance.Config.InvulnerableDuration;
        
        
        int timer = undetectTimer + vulnerableTimer;
        
        Log.Debug($"[SPY] {player.Nickname} is now undetectable for {undetectTimer} seconds and vulnerable for {vulnerableTimer} seconds. Total time: {timer} seconds.");
        
        int msgDuration = timer - Entrypoint.Instance.Config.SpawnMessageDuration;
        string stringBuilder = "";
        bool undetectedMsg = false;
        bool vulnerableMsg = false;
        
        int undetectTimerConst = undetectTimer - Entrypoint.Instance.Config.SpawnMessageDuration;
        int vulnerableTimerConst = vulnerableTimer - Entrypoint.Instance.Config.SpawnMessageDuration;

        while (timer > 0)
        {
            bool undetectable = (bool)player.SessionVariables["Undetectable"];
            bool vulnerable = (bool)player.SessionVariables["Vulnerable"];
            
            if (undetectedMsg && vulnerableMsg)
            {
                yield break;
            }
            
            if (!IsSpy(player))
            {
                player.SessionVariables["Undetectable"] = false;
                player.SessionVariables["Vulnerable"] = true;
                yield break;
            }

            if (undetectTimer > 0 && undetectable)
            {
                undetectTimer--;
                if (undetectTimer <= 0)
                {
                    player.SessionVariables["Undetectable"] = false;
                }
            }
            
            if (!undetectable && !undetectedMsg)
            {
                TurnSpyRole(player);
                player.Broadcast(9, Entrypoint.Instance.Translation.DetectableMessage);
                undetectedMsg = true;
                undetectTimer = -1;
            }
            
            if (!vulnerable && vulnerableTimer > 0)
            {
                vulnerableTimer--;
                if (vulnerableTimer <= 0)
                {
                    player.SessionVariables["Vulnerable"] = true;
                }
            }
            if (vulnerable && !vulnerableMsg)
            {
                player.Broadcast(9, Entrypoint.Instance.Translation.VulnerableMessage);
                vulnerableMsg = true;
                vulnerableTimer = -1;
            }
            
            if (timer < msgDuration)
            {
                stringBuilder = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=72%><align=right>";
                if (!undetectedMsg)
                    stringBuilder += Entrypoint.Instance.Translation.UndetectableMessage.Replace("%time%", ObscureTimer(undetectTimerConst, undetectTimer)) + "\n";
                if (!vulnerableMsg)
                    stringBuilder += Entrypoint.Instance.Translation.InvulnerableMessage.Replace("%time%", ObscureTimer(vulnerableTimerConst, vulnerableTimer));
                    
                stringBuilder += "</align></size>";
                player.ShowHint(stringBuilder, 1f);
            }
            
            timer--;
            yield return Timing.WaitForSeconds(1f);
        }
        if ((bool)player.SessionVariables["Undetectable"])
        {
            TurnSpyRole(player);
            player.SessionVariables["Undetectable"] = false;
            player.Broadcast(9, Entrypoint.Instance.Translation.DetectableMessage);
        }
        if (!(bool)player.SessionVariables["Vulnerable"])
        {
            player.SessionVariables["Vulnerable"] = true;
            player.Broadcast(9, Entrypoint.Instance.Translation.VulnerableMessage);
        }
    }
    
    public IEnumerator<float> AppearanceCoroutine(Player player)
    {
        for (;;)
        {
            yield return Timing.WaitForSeconds(20);
            
            Log.Debug($"[SPY] Changing appearance of {player.Nickname}.");
            
            if (!IsSpy(player) || player.Role == RoleTypeId.Spectator || Round.IsEnded)
            {
                yield break;
            }

            if (GetSpyType(player) == SpyType.ChaosSpy)
            {
                player.ChangeAppearance(RoleTypeId.NtfPrivate);
            }
            else if (GetSpyType(player) == SpyType.NtfSpy)
            {
                player.ChangeAppearance(RoleTypeId.ChaosRifleman);
            }
        }
    }

    public string ObscureTimer(int defaultTimer, int currentTime)
    {
        string timer = "";
        
        int percentage = (int) Math.Round((double) currentTime / defaultTimer * 100);
        
        if (percentage > 50)
            timer += "<color=green>";
        else if (percentage > 25)
            timer += "<color=orange>";
        else
            timer += "<color=red>";
        
        for (int i = 0; i < percentage / 10; i++)
        {
            timer += "█";
        }
        for (int i = 0; i < 10 - percentage / 10; i++)
        {
            timer += "░";
        }
        timer += "</color>";
        
        return timer;
    }
}