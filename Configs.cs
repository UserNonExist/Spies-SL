using System.ComponentModel;
using Exiled.API.Interfaces;
using SpiesSl.Enums;
using SpiesSl.Items;

namespace SpiesSl;

public class Configs : IConfig
{
    [Description("Whether or not the plugin is enabled.")]
    public bool IsEnabled { get; set; } = true;
    [Description("Whether or not the plugin is in debug mode.")]
    public bool Debug { get; set; } = false;
    
    [Description("The base chance for a spy to spawn in each spawnwave. -1 to disable spy naturally spawning.")]
    public int BaseSpawnChance { get; set; } = 30;
    [Description("Maximum cap for the chance for the spy to spawn in a spawnwave.")]
    public int MaxSpawnChance { get; set; } = 100;
    [Description("Whether additional chance should be scaled by the number of players in the server (PlayerList) or by number of respawning players (RespawningPlayer).")]
    public ScaleType ScaleType { get; set; } = ScaleType.RespawningPlayer;
    [Description("Additional chance per player that will based on scale_type, set 0 to disable.")]
    public int AdditionalSpawnChance { get; set; } = 3;
    
    [Description("Spawn chance for a second spy to spawn in the same spawnwave. -1 to disable second spy spawning.")]
    public int SecondSpyBaseChance { get; set; } = 20;
    [Description("Maximum cap for the chance for the second spy to spwan in a spawnwave.")]
    public int SecondSpyMaxSpawnChance { get; set; } = 80;
    [Description("Additional chance per scaletype, set 0 to disable.")]
    public int SecondSpyAdditionalChance { get; set; } = 5;
    [Description("Minimum number of respawning players in the spawnwave for second spy to spawn.")]
    public int MinimumPlayerForSecondSpy { get; set; } = 10;
    
    [Description("Duration of the spawn message.")]
    public float SpawnMessageDuration { get; set; } = 20f;
    [Description("Duration of spy being undetectable at all. (shooting spy will not show hit mark or damage, timer starts when spawned)")]
    public int UndetectableDuration { get; set; } = 60;
    [Description("Duration of spy being invulnerable to other team damage. (shooting spy will show hit mark, timer starts when spawned)")]
    public int InvulnerableDuration { get; set; } = 120;
    [Description("Configuration for the spychecker tools.")]
    public LJ61E Lj61E { get; set; } = new LJ61E();
}