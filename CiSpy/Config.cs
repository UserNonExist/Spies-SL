using System.ComponentModel;
using Exiled.API.Interfaces;
using CiSpy.Enums;

namespace CiSpy;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    [Description("Base chance for a spy to spawn in each spawnwaves.")]
    public int BaseSpawnChance { get; set; } = 20;
    [Description("Whether additional chance should be scaled by the number of players in the server (ScaleByPlayer) or by number of respawning players (ScaleByRespawn).")]
    public ScaleType ScaleType { get; set; } = ScaleType.ScaleByRespawn;
    [Description("Additional chance per player, set 0 to disable.")]
    public int AdditionalSpawnChance { get; set; } = 2;
    [Description("Duration of the spawn message.")]
    public float SpawnMessageDuration { get; set; } = 20f;
    [Description("Duration of spy having a grace period.")]
    public int UnDamagableDuration { get; set; } = 120;
    [Description("Duration of player cannot spycheck.")]
    public int SpyCheckDuration { get; set; } = 75;
    [Description("Whether NTF Captain can spawn with a spy checking tool.")]
    public bool NtfCaptainCanSpawnWithSpyCheck { get; set; } = true;
}