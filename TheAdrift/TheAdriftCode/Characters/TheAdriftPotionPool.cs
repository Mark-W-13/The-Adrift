using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Characters;

public sealed class TheAdriftPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "TheAdrift";
    public override Color LabOutlineColor => TheAdriftCharacter.ThemeColor;

    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/ui/energy_icon_24.png";
    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/ui/energy_icon_74.png";
}
