using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.MultiheartHelper;

public class MultiheartHelperModuleSettings : EverestModuleSettings {
    [DefaultButtonBinding(Buttons.RightStick, Keys.Tab)]
    public ButtonBinding DisplayInventoryUI { get; set; }
}