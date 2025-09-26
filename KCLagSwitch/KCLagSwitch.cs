using MelonLoader;

namespace KCLagSwitch
{
    public class KCLagSwitch : MelonMod
    {
        public override void OnApplicationStart()
        {
            // TODO: Linux+Proton causes input buffering problems with Unity in case of high lag, making
            // it difficult to actually toggle OFF the lag switch after it has been toggled ON. Oops.
            LoggerInstance.Warning($"{nameof(KCLagSwitch)} is not supported on platforms other than Windows.");

            // Start the separate input thread that listen for lag switch toggle key presses.
            LagSwitchInput.Start();

            // Log the actual toggle key, to avoid incorrect docs of which key should be pressed.
            LoggerInstance.Msg(
                $"{nameof(LagSwitchInput)} started listening for the {LagSwitchInput.VK_TOGGLE_NAME} key" +
                   $" to toggle {nameof(LagSwitch)} on/off."
            );
        }

        public override void OnUpdate()
        {
            // If the separate input thread detected a toggle key press, then the switch is toggled.
            if (LagSwitchInput.Update())
                LoggerInstance.Msg($"Toggled {nameof(LagSwitch)} (the {LagSwitchInput.VK_TOGGLE_NAME} key).)");
        }

        public override void OnApplicationQuit()
        {
            // Synchronously terminate the separate input thread.
            LagSwitchInput.Stop();
        }
    }
}