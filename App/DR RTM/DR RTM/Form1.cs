using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ReadWriteMemory;

namespace DR_RTM // Sup nerd hope you enjoy this, btw i ripped the connect part from an old project i had
{
    public partial class Form1 : Form
    {
        Process[] processname;

        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        private static unsafe extern Boolean WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int nSize, void* lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        static extern Int32 CloseHandle(IntPtr hObject);

        static public IntPtr MemoryOpen(int ProcessID)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, ProcessID);
            return hProcess;
        }
        unsafe public void Write(uint mAddress, byte[] Buffer, int ProcessID)
        {
            if (MemoryOpen(ProcessID) == (IntPtr)0x00000000)
            {

                return;
            }
            if (!WriteProcessMemory(MemoryOpen(ProcessID), mAddress, Buffer, Buffer.Length, null))
            {

                return;
            }

        }


            private void Form1_Load(object sender, EventArgs e)
        {
            // Null
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processname = Process.GetProcessesByName("DeadRising"); // This just used to test the connection to the game
            if (processname.Length == 0)
            {
                MessageBox.Show("The game process was not detected!\nPlease open the game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "Failed";
            }
            else
            {
                MessageBox.Show("Connected!", "Connection Sucessfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
                label1.Text = "Connected";
            }
        }

    }
}
// GL on the App - Sweed