using Exiled.API.Interfaces;

namespace CiSpy;

public class Translation : ITranslation
{
    public string ChaosSpySpawnMessage { get; set; } = "\n\nคุณได้เกิดเป็นสปายของฝ่าย<color=green>เคออส</color>\nผู้เล่นคนอื่นจะเห็นคุณเป็น <color=blue>MTF</color>\nอีกฝ่ายจะไม่สามารถยิงคุณได้จนกว่าคุณยิงกลับ\nคุณสามารถเปิดเผยตัวตนของคุณได้โดยใช้คำสั่ง .reveal";
    public string MtfSpySpawnMessage { get; set; } = "\n\nคุณได้เกิดเป็นสปายของฝ่าย <color=blue>NTF</color>\nผู้เล่นคนอื่นจะเห็นคุณเป็น<color=green>เคออส</color>\nอีกฝ่ายจะไม่สามารถยิงคุณได้จนกว่าคุณยิงกลับ\nคุณสามารถเปิดเผยตัวตนของคุณได้โดยใช้คำสั่ง .reveal";
    public string SpectatingSpyMessage { get; set; } = "ผู้เล่นคนนี้เป็นสปาย";
    public string SpyRevealedMessage { get; set; } = "คุณได้เปิดเผยตัวตนแล้ว";
    public string NowDamagableMessage { get; set; } = "\n\nอีกฝ่ายสามารถยิงคุณได้แล้ว";
    public string CannotCuffMessage { get; set; } = "คุณไม่สามารถจับคนในขณะที่เป็นสปายได้";
}