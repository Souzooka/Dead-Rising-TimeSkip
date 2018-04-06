using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
        private static uint campaignProgress;
        private static bool inCutsceneOrLoad;
        private static int loadingRoomId;
        private static byte caseMenuState;
        private static dynamic old = new ExpandoObject();

        public static void Init()
        {

        }

        public static string StringTime(uint time)
        {
            uint hours = time / (108000) % 24,
                 minutes = time / (108000 / 60) % 60,
                 seconds = time / (108000 / 60 / 60) % 60;

            string suffix = "AM";
            if (hours >= 12)
            {
                suffix = "PM";
                hours %= 12;
            }
            if (hours == 0) { hours = 12; }

            return $"{hours.ToString("D2")}:{minutes.ToString("D2")}:{seconds.ToString("D2")} {suffix}";
        }

        public static void UpdateEvent(Object source, ElapsedEventArgs e)
        {
            if (gameMemory != null && !gameMemory.CheckProcess()) { gameMemory = null; UpdateTimer.Enabled = false; return; }
            if (gameMemory == null) { gameMemory = new ReadWriteMemory.ProcessMemory(GameProcess); }
            if (!gameMemory.IsProcessStarted()) { gameMemory.StartProcess(); }

            gameTimePtr = gameMemory.Pointer("DeadRising.exe", 0x1944DD8, 0x20DC0);
            if (gameTimePtr == IntPtr.Zero)
            {
                if (!form.IsDisposed) { form.TimeDisplayUpdate("<missing>"); }
                return;
            }

            // update old object
            old.gameTime = gameTime;
            old.campaignProgress = campaignProgress;
            old.inCutsceneOrLoad = inCutsceneOrLoad;
            old.loadingRoomId = loadingRoomId;
            old.caseMenuState = caseMenuState;

            // update current params
            gameTime = gameMemory.ReadUInt(IntPtr.Add(gameTimePtr, gameTimeOffset));
            campaignProgress = gameMemory.ReadUInt(IntPtr.Add(gameMemory.Pointer("DeadRising.exe", 0x1944DD8, 0x20DC0), 0x150));
            inCutsceneOrLoad = (gameMemory.ReadByte(IntPtr.Add(gameMemory.Pointer("DeadRising.exe", 0x1945F70), 0x70)) & 1) == 1;
            loadingRoomId = gameMemory.ReadInt(IntPtr.Add(gameMemory.Pointer("DeadRising.exe", 0x1945F70), 0x48));
            caseMenuState = gameMemory.ReadByte(IntPtr.Add(gameMemory.Pointer("DeadRising.exe", 0x1946FC0, 0x2F058), 0x182));

            form.TimeDisplayUpdate(StringTime(gameTime));

            // Case menu closes
            if ((old.caseMenuState == 2 || old.caseMenuState == 19) && caseMenuState == 0)
            {
                if (campaignProgress == 140) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 5832000); } // Case 2 (Day 2, 06:00)
                if (campaignProgress == 215) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 6372000); } // Case 3 (Day 2, 11:00)
                if (campaignProgress == 220) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 6804000); } // Case 4 (Day 2, 15:00)
                if (campaignProgress == 250) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 7776000); } // Case 5 (Day 3, 00.00)
                if (campaignProgress == 290) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 8100000); } // Case 6 (Day 3, 03:00)
                if (campaignProgress == 300) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 8964000); } // Case 7 (Day 3, 11:00)
                if (campaignProgress == 340) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 9612000); } // Case 8 (Day 3, 17:00)
                if (campaignProgress == 390) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 10152000); } // The Facts: Memories (Day 3, 22:00)
                if (campaignProgress == 400) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 10260000); } // Cutscene: Jessie calls Frank (Day 3, 23:00)
            }

            // Jessie calls Frank again (needs 10 minutes after last cutscene so wait until we return)
            if (campaignProgress == 402 && old.inCutsceneOrLoad && !inCutsceneOrLoad) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), gameTime + (30 * 600) + 1); }

            // Jessie && Spec Ops (needs 40 minutes after last cutscene so wait until we return)
            if (campaignProgress == 404 && old.inCutsceneOrLoad && !inCutsceneOrLoad) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 10368001); }

            // Note: I added 1 unit to these times because the game seems to use a greater than check, and there's a bug where time will not progress.
            if (old.campaignProgress != 410 && campaignProgress == 410) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 11448000); } // Cutscene: Military leaves (Day 4, 10:00)
            if (old.campaignProgress != 415 && campaignProgress == 415) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 11448000); } // Cutscene: Military leaves (Day 4, 10:00)

            // Ed shows up && crashes like a wuss (Day 4, 12:00)
            if (campaignProgress == 420 && loadingRoomId == 288 && old.inCutsceneOrLoad && !inCutsceneOrLoad) { gameMemory.WriteUInt(IntPtr.Add(gameTimePtr, gameTimeOffset), 11664500 + 1); }
        }
    }
}
