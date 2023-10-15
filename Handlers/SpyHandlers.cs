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
        
        CoroutineHandles.Add(Timing.RunCoroutine(UndetectableCoroutine(player)));
        CoroutineHandles.Add(Timing.RunCoroutine(VulnerableCoroutine(player)));
        
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
        
        CoroutineHandles.Add(Timing.RunCoroutine(UndetectableCoroutine(player)));
        CoroutineHandles.Add(Timing.RunCoroutine(VulnerableCoroutine(player)));

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
        
        Timing.CallDelayed(0.6f, () =>
        {
            player.CurrentItem = lastItem;
            player.IsSpawnProtected = false;
        });
    }

    public IEnumerator<float> UndetectableCoroutine(Player player)
    {
        player.SessionVariables["Undetectable"] = true;
        int timer = Entrypoint.Instance.Config.UndetectableDuration;
        float spawnMsgDuration = Entrypoint.Instance.Config.UndetectableDuration - Entrypoint.Instance.Config.SpawnMessageDuration;

        while (timer > 0)
        {
            if (!IsSpy(player))
            {
                player.SessionVariables["Undetectable"] = false;
                yield break;
            }
            
            if (player.SessionVariables["Undetectable"] is bool undetectable && !undetectable)
            {
                player.Broadcast(9, Entrypoint.Instance.Translation.DetectableMessage);
                yield break;
            }
            
            if (timer < spawnMsgDuration)
            {
                player.ShowHint(Entrypoint.Instance.Translation.UndetectableMessage.Replace("%time%", timer.ToString()), 1f);
            }

            yield return Timing.WaitForSeconds(1f);
            timer--;
        }
        
        TurnSpyRole(player);
        player.Broadcast(9, Entrypoint.Instance.Translation.DetectableMessage);
        player.SessionVariables["Undetectable"] = false;
    }

    public IEnumerator<float> VulnerableCoroutine(Player player)
    {
        player.SessionVariables["Vulnerable"] = false;
        int timer = Entrypoint.Instance.Config.InvulnerableDuration;

        while (timer > 0)
        {
            if (!IsSpy(player))
            {
                player.SessionVariables["Vulnerable"] = true;
                yield break;
            }
            
            if (player.SessionVariables["Vulnerable"] is bool vulnerable && vulnerable)
            {
                player.ShowHint(Entrypoint.Instance.Translation.VulnerableMessage, 9f);
                yield break;
            }
            
            timer--;
            yield return Timing.WaitForSeconds(1f);
        }
        
        player.ShowHint(Entrypoint.Instance.Translation.VulnerableMessage, 9f);
        player.SessionVariables["Vulnerable"] = true;
    }
}