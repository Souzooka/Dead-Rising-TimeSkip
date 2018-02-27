using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DR_RTM
{
    public static class TimeSkip
    {
        public static double TimerInterval = 1000 / 60.0d;
        public static Timer UpdateTimer = new Timer(TimerInterval);
        public static Process GameProcess;
        public static Form1 form;
        private static ReadWriteMemory.ProcessMemory gameMemory;
        private static IntPtr gameTimePtr;
        private const int gameTimeOffset = 0x198;
        private static uint gameTime;

        public static void Init()
        {

        }

        public static void UpdateEvent(Object source, ElapsedEventArgs e)
        {
            if (gameMemory == null) { gameMemory = new ReadWriteMemory.ProcessMemory(GameProcess); }
            if (!gameMemory.IsProcessStarted()) { gameMemory.StartProcess(); }

            gameTimePtr = gameMemory.Pointer("DeadRising.exe", 0x1944DD8, 0x20DC0);
            if (gameTimePtr == IntPtr.Zero)
            {
                if (!form.IsDisposed) { form.TimeDisplayUpdate("<missing>"); }
                return;
            }

            gameTime = gameMemory.ReadUInt(IntPtr.Add(gameTimePtr, 0x198));
            if (!form.IsDisposed) { form.TimeDisplayUpdate(gameTime.ToString()); }
        }

    }
}
