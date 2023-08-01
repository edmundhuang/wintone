using System;
using System.Runtime.InteropServices;

namespace WintoneLib.Core.Helpers
{
    public static class DLLHelper
    {
        [DllImport("Kernel32.dll")]
        public static extern IntPtr LoadLibrary(string path);

        [DllImport("Kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        public static Delegate LoadFunction<T>(IntPtr hModule, string functionName)
        {
            IntPtr functionAddress = GetProcAddress(hModule, functionName);
            if (functionAddress.ToInt64() == 0)
            {
                return null;
            }
            return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
        }
    }
}
