using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace client
{
    class ramcpu
    {
        public static int cpu()
        {
            int cputotal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "(Get-CimInstance -Class Win32_Processor).LoadPercentage",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                cputotal = (int)Convert.ToDouble(output);
                proc.Close();
            }
            else
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    proc.StartInfo.Arguments = "-c \" ps -A -o %cpu | awk '{s+=$1} END {print s}'\"";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    proc.StartInfo.Arguments = "-c \" " + "grep 'cpu ' /proc/stat | awk '{usage=($2+$4)*100/($2+$4+$5)} END {print usage}'" + "\"";
                }
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                cputotal = (int)Convert.ToDouble(output);
                proc.Close();
            }
            return cputotal;
        }
        public static int ram()
        {
            int ramtotal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var proc1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "Get-CIMInstance Win32_OperatingSystem | Select FreePhysicalMemory|%{$_.FreePhysicalMemory/1024}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc1.Start();
                string output1 = proc1.StandardOutput.ReadToEnd();
                ramtotal = (int)Convert.ToDouble(output1);
                proc1.Close();
            }
            else
            {
                var proc1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    proc1.StartInfo.Arguments = "-c \" top -l1 | awk '/PhysMem/ {print int($6)}'\"";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    proc1.StartInfo.Arguments = "-c \" " + "free | awk 'FNR == 3 {print$4/1024}'" + "\"";
                }
                proc1.Start();
                string output1 = proc1.StandardOutput.ReadToEnd();
                ramtotal = (int)Convert.ToDouble(output1);
                proc1.Close();
            }
            return ramtotal;
        }
    }
}
