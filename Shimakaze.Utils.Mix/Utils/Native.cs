using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Shimakaze.Utils.Mix.Utils
{
    internal static class Native
    {
        static Native()
        {
            NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, DllImportResolver);
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr result;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Xbox:
                case PlatformID.Win32S:
                case PlatformID.WinCE:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    if (Environment.Is64BitProcess)
                    {
                        if (NativeLibrary.TryLoad($"{libraryName}-x64.dll", out result))
                            return result;
                    }
                    else
                    {
                        if (NativeLibrary.TryLoad($"{libraryName}.dll", out result))
                            return result;
                    }
                    break;
                case PlatformID.Other:
                case PlatformID.Unix:
                    if (NativeLibrary.TryLoad($"{libraryName}.so", out result))
                        return result;
                    break;
                case PlatformID.MacOSX:
                    if (NativeLibrary.TryLoad($"{libraryName}.dylib", out result))
                        return result;
                    break;
                default:
                    break;
            }
            return NativeLibrary.Load(libraryName);
        }
       
        [DllImport("Shimakaze.Utils.Mix.Native", EntryPoint = "get_blowfish_key")]
        public static extern void GetBlowfishKey(byte[] s, byte[] d);

    }
}