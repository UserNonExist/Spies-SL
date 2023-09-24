using System;
using CiSpy.Items;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using HarmonyLib;
using MEC;

namespace CiSpy;

public class Entrypoint : Plugin<Config, Translation>
{
    public override string Author { get; } = "User_NotExist";
    public override string Name { get; } = "CiSpy";
    public override Version Version { get; } = new Version(1, 2, 6);
    public override Version RequiredExiledVersion { get; } = new Version(8, 2, 1);
    
    public static Entrypoint Instance { get; private set; }
    public static EventHandlers EventHandlers;

    public SpyDetector SpyDetector;

    public override void OnEnabled()
    {
        Instance = this;
        EventHandlers = new EventHandlers();
        SpyDetector = new SpyDetector();
        
        Timing.CallDelayed(5f, () =>
        {
            SpyDetector.Register();
        });
        

        Exiled.Events.Handlers.Player.Verified += EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Dying += EventHandlers.OnDying;
        Exiled.Events.Handlers.Server.RespawningTeam += EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnHurting;
        Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;
        Exiled.Events.Handlers.Player.Handcuffing += EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Player.Shooting += EventHandlers.OnShooting;
        Exiled.Events.Handlers.Player.Left += EventHandlers.OnLeft;
        Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnChangingRole;
        Exiled.Events.Handlers.Player.ThrownProjectile += EventHandlers.OnThrownProjectile;
    }

    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnVerified;
        Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnDying;
        Exiled.Events.Handlers.Server.RespawningTeam -= EventHandlers.OnRespawningTeam;
        Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= EventHandlers.OnChangingSpectatedPlayer;
        Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnHurting;
        Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;
        Exiled.Events.Handlers.Player.Handcuffing -= EventHandlers.OnHandcuffing;
        Exiled.Events.Handlers.Player.Shooting -= EventHandlers.OnShooting;
        Exiled.Events.Handlers.Player.Left -= EventHandlers.OnLeft;
        Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnChangingRole;
        Exiled.Events.Handlers.Player.ThrownProjectile -= EventHandlers.OnThrownProjectile;
        
        SpyDetector.Unregister();
        SpyDetector = null;
        
        Instance = null;
        EventHandlers = null;
        
    }
}