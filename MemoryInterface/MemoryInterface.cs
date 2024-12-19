using System.Diagnostics;
using System.Runtime.InteropServices;


namespace MemoryInterface
{
    public class MemoryInterface
    {
        // Windows-provided Constants
        private static int PROCESS_VM_READ = 0x0010;
        private static int PROCESS_VM_WRITE = 0x0020;
        private static int PROCESS_VM_OPERATION = 0x0008;
        private static int PAGE_EXECUTE_READWRITE = 0x40;

        // Handle to the process, if zero, we are not connected
        private IntPtr processHandle = IntPtr.Zero;
        private IntPtr mainModuleBaseAddress = IntPtr.Zero;

        /**
         * Memory Interface:
         * Create a new interface with MemoryInterface(string processName) and then use it to read/write memory with ease.
         * 
         **/
        public MemoryInterface(string processName)
        {
            Process[] matchingProcesses = Process.GetProcessesByName(processName);
            if (matchingProcesses.Length > 0)
            {
                Process target = matchingProcesses[0];
                // if we find a matching process to the name, we open a handle with all the permissions we need
                processHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, target.Id);
                ProcessModule targetModule = target.MainModule;
                if (targetModule != null)
                {
                    mainModuleBaseAddress = targetModule.BaseAddress;
                }
            }
        }

        // simple way to check if we found a process / if we've already closed the handle
        public bool IsOpen
        {
            get
            {
                return processHandle != IntPtr.Zero;
            }
        }

        public IntPtr BaseAddress
        {
            get
            {
                return mainModuleBaseAddress;
            }
        }

        // ensures we are not leaving handles open amateurishly. ensure you call close after you're done with the memory interface
        public void Close()
        {
            if(IsOpen)
            {
                CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
            }
        }

        // tries to unlock page for read/write/execute. only normally needed to edit code. returns true on success, false on fail
        public bool UnlockPage(IntPtr lpAddress)
        {
            if(IsOpen)
            {
                int oldProtect = 0;
                return VirtualProtectEx(processHandle, lpAddress, 1, PAGE_EXECUTE_READWRITE, ref oldProtect);
            }
            return false;
        }


        // reads size bytes from address into databuffer. returns true on success, false on fail
        public bool ReadBytes(IntPtr address, int size, byte[] dataBuffer)
        {
            if(IsOpen)
            {
                int bytesRead = 0;
                if (ReadProcessMemory(processHandle, address, dataBuffer, size, ref bytesRead))
                {
                    return true;
                }
            }

            return false;
        }

        // read 1 byte from address into readByte. returns true on success, false on fail
        public bool ReadByte(IntPtr address, ref byte readByte)
        {
            byte[] buffer = new byte[1];
            bool result = ReadBytes(address, 1, buffer);
            if(result)
            {
                readByte = buffer[0];
            }
            return result;
        }

        // read 1 short from address into readShort. returns true on success, false on fail
        public bool ReadShort(IntPtr address, ref short readShort)
        {
            byte[] buffer = new byte[2];
            bool result = ReadBytes(address, 2, buffer);
            if (result)
            {
                readShort = BitConverter.ToInt16(buffer);
            }
            return result;
        }

        // read 1 int from address into readInt. returns true on success, false on fail
        public bool ReadInt(IntPtr address, ref int readInt)
        {
            byte[] buffer = new byte[4];
            bool result = ReadBytes(address, 4, buffer);
            if(result)
            {
                readInt = BitConverter.ToInt32(buffer);
            }
            return result;
        }

        // read 1 long from address into readLong. returns true on success, false on fail
        public bool ReadLong(IntPtr address, ref long readLong)
        {
            byte[] buffer = new byte[8];
            bool result = ReadBytes(address, 8, buffer);
            if(result)
            {
                readLong = BitConverter.ToInt64(buffer);
            }
            return result;
        }

        // read 1 float from address into readFloat. returns true on success, false on fail
        public bool ReadFloat(IntPtr address, ref float readFloat)
        {
            byte[] buffer = new byte[4];
            bool result = ReadBytes(address, 4, buffer);
            if(result)
            {
                readFloat = BitConverter.ToSingle(buffer);
            }
            return result;
        }

        // read 1 double from address into readDouble. returns true on success, false on fail
        public bool ReadDouble(IntPtr address, ref double readDouble)
        {
            byte[] buffer = new byte[8];
            bool result = ReadBytes(address, 8, buffer);
            if(result)
            {
                readDouble = BitConverter.ToDouble(buffer);
            }
            return result;
        }

        // reads null-term'ed strings from address into readString. returns true on success, false on fail
        public bool ReadString(IntPtr address, ref string readString)
        {
            List<char> chars = new List<char>();
            int i = 0;
            byte currentByte = 0;
            do
            {
                if (!ReadByte(address + i++, ref currentByte))
                {
                    return false;
                }
                chars.Add((char) currentByte);
            } while (currentByte != 0);
            readString = new string(chars.ToArray());
            return true;
        }

        // attempts to write bytes to address. returns true on success, false on fail
        public bool WriteBytes(IntPtr address, byte[] data)
        {
            if(IsOpen)
            {
                int bytesWritten = 0;
                return WriteProcessMemory(processHandle, address, data, data.Length, ref bytesWritten);
            }
            return false;
        }

        // writes a byte to address. returns true on success, false on fail
        public bool WriteByte(IntPtr address, byte value)
        {
            return WriteBytes(address, new byte[] {value});
        }

        // writes a short to address. returns true on success, false on fail
        public bool WriteShort(IntPtr address, short value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        // writes an int to address. returns true on success, false on fail
        public bool WriteInt(IntPtr address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        // writes a long to address. returns true on success, false on fail
        public bool WriteLong(IntPtr address, long value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        // writes a float to address. returns true on success, false on fail
        public bool WriteFloat(IntPtr address, float value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        // writes a double to address. returns true on success, false on fail
        public bool WriteDouble(IntPtr address, double value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        // writes a string to address. returns true on success, false on fail
        public bool WriteString(IntPtr address, string value)
        {
            char[] chars = value.ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for(int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)chars[i];
            }

            return WriteBytes(address, bytes);
        }

        // DLL Imports used for memory access

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessID);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flNewProtected, ref int lpFlOldProtect);
    }
}
