using Exiled.API.Interfaces;

namespace SpiesSl;

public class Translations : ITranslation
{
    public string ChaosSpySpawnMessage { get; set; } = "\n\nYou have spawned as <color=green>chaos</color> spy.\nOthers will see you as <color=blue>NTF</color> while you are alive.\nThe other team cannot damage you until you shoot back.\nYou can reveal your role at anytime by using \".reveal\".\n%multiplePlayer%";
    public string NtfSpySpawnMessage { get; set; } = "\n\nYou have spawned as <color=blue>NTF</color> spy.\nOthers will see you as <color=green>chaos</color> while you are alive.\nThe other team cannot damage you until you shoot back.\nYou can reveal your role at anytime by using \".reveal\".\n%multiplePlayer%";
    public string MultipleSpyAdditionalMessage { get; set; } = "This time, %player% will be joining you.";
    public string UndetectableMessage { get; set; } = "Spycheck Protection: %time% second(s)";
    public string InvulnerableMessage { get; set; } = "Damage Protection: %time% second(s)";
    public string DetectableMessage { get; set; } = "This is an expected behaivor.\\nOther team still see you as normal, but they can now see hitmark when they shoot you.";
    public string VulnerableMessage { get; set; } = "The other team can now damage/kill you.";
    public string SpyRevealedMessage { get; set; } = "You have revealed your role.";
    public string CannotCuffMessage { get; set; } = "You cannot cuff while you are a spy.";
    public string FriendlyCuffSpyMessage { get; set; } = "You cannot cuff your team spy.";
    public string FriendlyHurtSpyMessage { get; set; } = "This player is your team spy.";
    public string SpectatingSpyMessage { get; set; } = "This player is %role%";
    public string ChaosSpyName { get; set; } = "<color=green>Chaos Spy</color>";
    public string NtfSpyName { get; set; } = "<color=blue>NTF Spy</color>";
}