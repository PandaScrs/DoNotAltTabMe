using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DoNotAltTabMe_4._7._2
{
    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private readonly LowLevelKeyboardProc proc;
        private IntPtr hookId = IntPtr.Zero;
        private HashSet<Keys> currentlyPressedKeys = new HashSet<Keys>();
        private Keys[] registeredKeyCombo;

        public event Action KeyCombinationPressed;

        public KeyboardHook()
        {
            proc = HookCallback;
            hookId = SetHook(proc);
        }

        public void RegisterKeyCombo(Keys[] combo)
        {
            registeredKeyCombo = combo;
        }

        public void UnregisterAll()
        {
            registeredKeyCombo = null;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    currentlyPressedKeys.Add(key);
                    CheckKeyCombo();
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    currentlyPressedKeys.Remove(key);
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private void CheckKeyCombo()
        {
            if (registeredKeyCombo == null || registeredKeyCombo.Length == 0)
                return;

            // Convertir las teclas actuales a sus equivalentes genéricos
            var normalizedPressedKeys = new HashSet<Keys>();
            foreach (var key in currentlyPressedKeys)
            {
                // Normalizar tipos de teclas
                if (key == Keys.LControlKey || key == Keys.RControlKey)
                    normalizedPressedKeys.Add(Keys.ControlKey);
                else if (key == Keys.LShiftKey || key == Keys.RShiftKey)
                    normalizedPressedKeys.Add(Keys.ShiftKey);
                else if (key == Keys.LMenu || key == Keys.RMenu) // Alt
                    normalizedPressedKeys.Add(Keys.Menu);
                else
                    normalizedPressedKeys.Add(key);
            }

            // Check si todas las teclas registradas están presionadas
            bool allKeysPressed = registeredKeyCombo.All(k =>
                normalizedPressedKeys.Contains(k) ||
                (k == Keys.ControlKey && (normalizedPressedKeys.Contains(Keys.LControlKey) || normalizedPressedKeys.Contains(Keys.RControlKey))) ||
                (k == Keys.ShiftKey && (normalizedPressedKeys.Contains(Keys.LShiftKey) || normalizedPressedKeys.Contains(Keys.RShiftKey))) ||
                (k == Keys.Menu && (normalizedPressedKeys.Contains(Keys.LMenu) || normalizedPressedKeys.Contains(Keys.RMenu)))
            );

            // Sólo se validan las teclas que están presionadas
            int normalizedPressedCount = normalizedPressedKeys.Count;
            int registeredCount = registeredKeyCombo.Length;

            if (allKeysPressed && normalizedPressedCount >= registeredCount)
            {
                KeyCombinationPressed?.Invoke();
            }
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(hookId);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
