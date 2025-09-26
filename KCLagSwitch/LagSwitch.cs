using UnityEngine;

namespace KCLagSwitch
{
    public struct LagSwitch
    {
        /* Toggle the lag switch, by fixing Unity's target frame rate.
         *
         * Toggling the switch manipulates `Application.targetFrameRate` and `QualitySettings.vSyncCount`.
         * Every uneven invocation number enables the switch, and stores the settings values at the time of invocation.
         * Every even invocation number disables the switch, and restores the settings stored by the preceding activation.
         */
        public static void Toggle()
        {
            if (LagSwitch.active)
            {
                // Restore the settings stored when the switch was toggled.
                Application.targetFrameRate = LagSwitch._oldTargetFrameRate;
                QualitySettings.vSyncCount = LagSwitch._oldVSyncCount;
            }
            else
            {
                // Store the current settings before toggling.
                LagSwitch._oldTargetFrameRate = Application.targetFrameRate;
                LagSwitch._oldVSyncCount = QualitySettings.vSyncCount;
                // Override the current settings to simulate lag by fixing the framerate.
                Application.targetFrameRate = LagSwitch.targetFrameRate; // Limit the framerate.
                QualitySettings.vSyncCount  = 0; // Do not ignore `Application.targetFrameRate`.
            }
            LagSwitch.active = ! LagSwitch.active;
        }

        // Induce "lag" by fixing the framerate (fps) to this value.
        public static int targetFrameRate { get; private set; } = 1;
        // Store old settings right before they are overriden by toggling the lag switch.
        private static int _oldTargetFrameRate;
        private static int _oldVSyncCount;
        // True if the lag switch is currently active, else false.
        public static bool active { get; private set; } = false;
    }
}