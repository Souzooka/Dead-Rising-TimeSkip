using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Windows.Forms;
using System.Media;
using System.Text;
using System.Threading;
namespace ReadWriteMemory
{
    internal class ProcessMemory
    {
        // Fields
        protected IntPtr BaseAddress;
        protected Process[] MyProcess;
        protected ProcessModule myProcessModule;
        private const uint PAGE_EXECUTE = 16;
        private const uint PAGE_EXECUTE_READ = 32;
        private const uint PAGE_EXECUTE_READWRITE = 64;
        private const uint PAGE_EXECUTE_WRITECOPY = 128;
        private const uint PAGE_GUARD = 256;
        private const uint PAGE_NOACCESS = 1;
        private const uint PAGE_NOCACHE = 512;
        private const uint PAGE_READONLY = 2;
        private const uint PAGE_READWRITE = 4;
        private const uint PAGE_WRITECOPY = 8;
        private const uint PROCESS_ALL_ACCESS = 2035711;
        protected int processHandle;
        protected Process process;

        // Methods
        public ProcessMemory(string pProcessName)
        {
            Process[] processess = Process.GetProcessesByName(pProcessName);
            if (processess.Length == 0)
            {
                throw new Exception("No processes exist by that name.");
            }

            process = processess[0];
        }

        public ProcessMemory(Process pProcess)
        {
            process = pProcess;
        }

        public bool CheckProcess()
        {
            return !process.HasExited;
        }

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(int hObject);
        public string CutString(string mystring)
        {
            char[] chArray = mystring.ToCharArray();
            string str = "";
            for (int i = 0; i < mystring.Length; i++)
            {
                if ((chArray[i] == ' ') && (chArray[i + 1] == ' '))
                {
                    return str;
                }
                if (chArray[i] == '\0')
                {
                    return str;
                }
                str = str + chArray[i].ToString();
            }
            return mystring.TrimEnd(new char[] { '0' });
        }

        public IntPtr DllImageAddress(string dllname)
        {
            ProcessModuleCollection modules = this.process.Modules;

            foreach (ProcessModule procmodule in modules)
            {
                if (dllname == procmodule.ModuleName)
                {
                    return procmodule.BaseAddress;
                }
            }
            throw new ArgumentException("Module not found.");

        }
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern int FindWindowByCaption(int ZeroOnly, string lpWindowName);
        public IntPtr ImageAddress()
        {
            this.myProcessModule = this.MyProcess[0].MainModule;
            this.BaseAddress = this.myProcessModule.BaseAddress;
            return this.BaseAddress;
        }

        public IntPtr ImageAddress(int pOffset)
        {
            this.myProcessModule = this.MyProcess[0].MainModule;
            this.BaseAddress = this.myProcessModule.BaseAddress;
            return IntPtr.Add(this.BaseAddress, pOffset);
        }
        public string MyProcessName()
        {
            return process.ProcessName;
        }

        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        public IntPtr Pointer(string Module, params int[] offsets)
        {
            IntPtr currAddr = this.DllImageAddress(Module);

            foreach (int offset in offsets)
            {
                currAddr = new IntPtr(this.ReadLong(IntPtr.Add(currAddr, offset)));
            }

            return currAddr;
        }

        public byte ReadByte(IntPtr pOffset)
        {
            byte[] buffer = new byte[1];
            ReadProcessMemory(this.processHandle, pOffset, buffer, 1, 0);
            return buffer[0];
        }

        public byte ReadByte(bool AddToImageAddress, int pOffset)
        {
            byte[] buffer = new byte[1];
            IntPtr lpBaseAddress = AddToImageAddress ? this.ImageAddress(pOffset) : new IntPtr(pOffset);
            ReadProcessMemory(this.processHandle, lpBaseAddress, buffer, 1, 0);
            return buffer[0];
        }

        public byte ReadByte(string Module, int pOffset)
        {
            byte[] buffer = new byte[1];
            ReadProcessMemory(this.processHandle, IntPtr.Add(this.DllImageAddress(Module), pOffset), buffer, 1, 0);
            return buffer[0];
        }

        public float ReadFloat(IntPtr pOffset)
        {
            return BitConverter.ToSingle(this.ReadMem(pOffset, 4), 0);
        }

        public float ReadFloat(bool AddToImageAddress, int pOffset)
        {
            return BitConverter.ToSingle(this.ReadMem(new IntPtr(pOffset), 4, AddToImageAddress), 0);
        }

        public float ReadFloat(string Module, int pOffset)
        {
            return BitConverter.ToSingle(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), 4), 0);
        }

        public int ReadInt(IntPtr pOffset)
        {
            return BitConverter.ToInt32(this.ReadMem(pOffset, 4), 0);
        }

        public int ReadInt(bool AddToImageAddress, IntPtr pOffset)
        {
            return BitConverter.ToInt32(this.ReadMem(pOffset, 4, AddToImageAddress), 0);
        }

        public int ReadInt(string Module, int pOffset)
        {
            return BitConverter.ToInt32(this.ReadMem(this.DllImageAddress(Module) + pOffset, 4), 0);
        }

        public long ReadLong(IntPtr pOffset)
        {
            return BitConverter.ToInt64(this.ReadMem(pOffset, 8), 0);
        }

        public long ReadLong(string Module, int pOffset)
        {
            return BitConverter.ToInt64(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), 8), 0);
        }

        public byte[] ReadMem(IntPtr pOffset, int pSize)
        {
            byte[] buffer = new byte[pSize];
            ReadProcessMemory(this.processHandle, pOffset, buffer, pSize, 0);
            return buffer;
        }

        public byte[] ReadMem(IntPtr pOffset, int pSize, bool AddToImageAddress)
        {
            byte[] buffer = new byte[pSize];
            IntPtr lpBaseAddress = AddToImageAddress ? this.ImageAddress((int)pOffset) : pOffset;
            ReadProcessMemory(this.processHandle, lpBaseAddress, buffer, pSize, 0);
            return buffer;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesRead);
        public short ReadShort(IntPtr pOffset)
        {
            return BitConverter.ToInt16(this.ReadMem(pOffset, 2), 0);
        }

        public short ReadShort(bool AddToImageAddress, IntPtr pOffset)
        {
            return BitConverter.ToInt16(this.ReadMem(pOffset, 2, AddToImageAddress), 0);
        }

        public short ReadShort(string Module, int pOffset)
        {
            return BitConverter.ToInt16(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), 2), 0);
        }

        public string ReadStringAscii(IntPtr pOffset, int pSize)
        {
            return this.CutString(Encoding.ASCII.GetString(this.ReadMem(pOffset, pSize)));
        }

        public string ReadStringAscii(bool AddToImageAddress, IntPtr pOffset, int pSize)
        {
            return this.CutString(Encoding.ASCII.GetString(this.ReadMem(pOffset, pSize, AddToImageAddress)));
        }

        public string ReadStringAscii(string Module, int pOffset, int pSize)
        {
            return this.CutString(Encoding.ASCII.GetString(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), pSize)));
        }

        public string ReadStringUnicode(IntPtr pOffset, int pSize)
        {
            return this.CutString(Encoding.Unicode.GetString(this.ReadMem(pOffset, pSize)));
        }

        public string ReadStringUnicode(bool AddToImageAddress, IntPtr pOffset, int pSize)
        {
            return this.CutString(Encoding.Unicode.GetString(this.ReadMem(pOffset, pSize, AddToImageAddress)));
        }

        public string ReadStringUnicode(string Module, int pOffset, int pSize)
        {
            return this.CutString(Encoding.Unicode.GetString(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), pSize)));
        }

        public uint ReadUInt(IntPtr pOffset)
        {
            return BitConverter.ToUInt32(this.ReadMem(pOffset, 4), 0);
        }

        public uint ReadUInt(bool AddToImageAddress, IntPtr pOffset)
        {
            return BitConverter.ToUInt32(this.ReadMem(pOffset, 4, AddToImageAddress), 0);
        }

        public uint ReadUInt(string Module, int pOffset)
        {
            return BitConverter.ToUInt32(this.ReadMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), 4), 0);
        }

        public bool StartProcess()
        {
            if (process.ProcessName != "")
            {
                this.MyProcess = Process.GetProcessesByName(process.ProcessName);
                if (this.MyProcess.Length == 0)
                {
                    MessageBox.Show(process.ProcessName + " is not running or has not been found. Please check and try again", "Process Not Found", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                this.processHandle = OpenProcess(2035711, false, this.MyProcess[0].Id);
                if (this.processHandle == 0)
                {
                    MessageBox.Show(process.ProcessName + " is not running or has not been found. Please check and try again", "Process Not Found", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                return true;
            }
            MessageBox.Show("Define process name first!");
            return false;
        }

        public bool IsProcessStarted()
        {
            return this.processHandle != 0;
        }

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(int hProcess, int lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
        public void WriteByte(IntPtr pOffset, byte pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes((short)pBytes));
        }

        public void WriteByte(bool AddToImageAddress, int pOffset, byte pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes((short)pBytes), AddToImageAddress);
        }

        public void WriteByte(string Module, int pOffset, byte pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes((short)pBytes));
        }

        public void WriteDouble(IntPtr pOffset, double pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes));
        }

        public void WriteDouble(bool AddToImageAddress, int pOffset, double pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes), AddToImageAddress);
        }

        public void WriteDouble(string Module, int pOffset, double pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes(pBytes));
        }

        public void WriteFloat(IntPtr pOffset, float pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes));
        }

        public void WriteFloat(bool AddToImageAddress, int pOffset, float pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes), AddToImageAddress);
        }

        public void WriteFloat(string Module, int pOffset, float pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes(pBytes));
        }

        public void WriteInt(IntPtr pOffset, int pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes));
        }

        public void WriteInt(bool AddToImageAddress, int pOffset, int pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes), AddToImageAddress);
        }

        public void WriteInt(string Module, int pOffset, int pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes(pBytes));
        }

        public void WriteMem(IntPtr pOffset, byte[] pBytes)
        {
            WriteProcessMemory(this.processHandle, pOffset, pBytes, pBytes.Length, 0);
        }

        public void WriteMem(int pOffset, byte[] pBytes, bool AddToImageAddress)
        {
            IntPtr lpBaseAddress = AddToImageAddress ? this.ImageAddress(pOffset) : new IntPtr(pOffset);
            WriteProcessMemory(this.processHandle, lpBaseAddress, pBytes, pBytes.Length, 0);
        }

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);
        public void WriteShort(IntPtr pOffset, short pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes));
        }

        public void WriteShort(bool AddToImageAddress, int pOffset, short pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes), AddToImageAddress);
        }

        public void WriteShort(string Module, int pOffset, short pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes(pBytes));
        }

        public void WriteStringAscii(IntPtr pOffset, string pBytes)
        {
            this.WriteMem(pOffset, Encoding.ASCII.GetBytes(pBytes + "\0"));
        }

        public void WriteStringAscii(bool AddToImageAddress, int pOffset, string pBytes)
        {
            this.WriteMem(pOffset, Encoding.ASCII.GetBytes(pBytes + "\0"), AddToImageAddress);
        }

        public void WriteStringAscii(string Module, int pOffset, string pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), Encoding.ASCII.GetBytes(pBytes + "\0"));
        }

        public void WriteStringUnicode(IntPtr pOffset, string pBytes)
        {
            this.WriteMem(pOffset, Encoding.Unicode.GetBytes(pBytes + "\0"));
        }

        public void WriteStringUnicode(bool AddToImageAddress, int pOffset, string pBytes)
        {
            this.WriteMem(pOffset, Encoding.Unicode.GetBytes(pBytes + "\0"), AddToImageAddress);
        }

        public void WriteStringUnicode(string Module, int pOffset, string pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), Encoding.Unicode.GetBytes(pBytes + "\0"));
        }

        public void WriteUInt(IntPtr pOffset, uint pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes));
        }

        public void WriteUInt(bool AddToImageAddress, int pOffset, uint pBytes)
        {
            this.WriteMem(pOffset, BitConverter.GetBytes(pBytes), AddToImageAddress);
        }

        public void WriteUInt(string Module, int pOffset, uint pBytes)
        {
            this.WriteMem(IntPtr.Add(this.DllImageAddress(Module), pOffset), BitConverter.GetBytes(pBytes));
        }

        // Nested Types
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 2035711,
            CreateThread = 2,
            DupHandle = 64,
            QueryInformation = 1024,
            SetInformation = 512,
            Synchronize = 1048576,
            Terminate = 1,
            VMOperation = 8,
            VMRead = 16,
            VMWrite = 32
        }
    }
}