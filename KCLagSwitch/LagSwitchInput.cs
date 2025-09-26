using System.Runtime.InteropServices;
using System.Threading;

namespace KCLagSwitch
{
    /* A class that starts a separate thread that polls the OS for a keyboard key press.
     *
     * Using a thread separate from Unity's main thread, which handles the Unity input system,
     * aims to sidestep the problem of "queued input events" in case of low frame rates.
     * For example, at 1 fps holding down right arrow for a few seconds and then pressing shift once may
     * result in tens of seconds of the player moving right and only then the shift press being processed.
     */
    public class LagSwitchInput
    {
        // Create the input detection thread if it is not yet running.
        public static void Start()
        {
            // Ignore duplicate Start calls; Stop must be called before Start can be called a second time.
            if (LagSwitchInput._isInputThreadRunning) return;

            LagSwitchInput._isInputThreadRunning = true;
            LagSwitchInput._inputThread = new Thread(InputLoop);
            LagSwitchInput._inputThread.IsBackground = true;
            LagSwitchInput._inputThread.Start();
        }

        // Terminate the input detection thread synchronously if it is running.
        public static void Stop()
        {
            // Tell the input while(active) loop to break out; tell the input thread to finish execution / return.
            LagSwitchInput._isInputThreadRunning = false;
            // Synchronously wait until the input thread finishes execution / return.
            LagSwitchInput._inputThread?.Join();
            // Optional cleanup.
            LagSwitchInput._isToggleRequested = false;
        }

        // The lag switch hook that toggles the lag switch on/off if the toggle key was pressed.
        // Return true iff. the switch did get toggled by this invocation, else false.
        public static bool Update()
        {
            if (LagSwitchInput._isToggleRequested)
            {
                LagSwitchInput._isToggleRequested = false;
                LagSwitch.Toggle();
                return true;
            }

            return false;
        }

        private static void InputLoop()
        {
            // True iff. the button was down in the immediately preceding while-loop iteration.
            bool wasButtonAlreadyDown = false;

            while (LagSwitchInput._isInputThreadRunning)
            {
                // Poll: check if the most significant bit is set, which would mean the key is currently down.
                bool isButtonDown = (GetAsyncKeyState(VK_TOGGLE) & 0x8000) != 0;

                // Only request a toggle on the rising edge of pressing the button, ignore
                // the period where the button is continuously held down and then released.
                if (isButtonDown && !wasButtonAlreadyDown)
                    LagSwitchInput._isToggleRequested = true;

                // Once the button is released (isButtonDown = false), the
                // next rising edge may satisfy the toggle conditional again.
                wasButtonAlreadyDown = isButtonDown;
                Thread.Sleep(10); // Poll every 10ms.
            }
        }

        // Use the Win32 API to query real-time keyboard key state.
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // The separate thread that polls input to toggle the lag switch on/off.
        private static Thread _inputThread;
        // True if the input thread is running, else false.
        private static volatile bool _isInputThreadRunning = false;
        // True if the input thread has requested the lag switch to be disabled, else false.
        private static volatile bool _isToggleRequested = false;

        // The Virtual-Key code for the HOME key.
        private const int VK_HOME = 0x0024;

        // An alias for the Virtual-Key code that toggles the lag switch on/off. 
        public const int VK_TOGGLE = VK_HOME;
        // The string name for the toggle key, for use in logging.
        public const string VK_TOGGLE_NAME = "HOME";
    }
}
