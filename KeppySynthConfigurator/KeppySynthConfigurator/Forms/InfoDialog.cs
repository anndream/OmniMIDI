﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Management;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;

namespace KeppySynthConfigurator
{
    public partial class InfoDialog : Form
    {
        // Funcs

        private RegistryKey CurrentVerKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false);
        private FileVersionInfo Driver = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory + "\\keppysynth\\keppysynth.dll");
        private FileVersionInfo BASS = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory + "\\keppysynth\\bass.dll");
        private FileVersionInfo BASSMIDI = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory + "\\keppysynth\\bassmidi.dll");
        private String License = Environment.SystemDirectory + "\\keppysynth\\license.txt";

        private DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        // Dialog

        public InfoDialog(Int32 mode)
        {
            InitializeComponent();
            if (mode == 0)
                StartPosition = FormStartPosition.CenterParent;
            else
                StartPosition = FormStartPosition.CenterScreen;
        }

        private void InfoDialog_Load(object sender, EventArgs e)
        {
            ComputerInfo CI = new ComputerInfo();
            String Version = String.Format("{0}.{1}.{2}", Driver.FileMajorPart, Driver.FileMinorPart, Driver.FileBuildPart);
            VerLabel.Text = String.Format("Keppy's Synthesizer {0}\n\nCopyright Ⓒ 2013 - {1}\nKaleidonKep99, Kode54 && Mudlord", Version, DateTime.Now.Year.ToString());
            DriverVer.Text = String.Format("{0} (Bugfix {1})", Version, Driver.FilePrivatePart);
            BASSVer.Text = String.Format("{0} (Revision {1})", BASS.FileVersion, BASS.FilePrivatePart);
            BASSMIDIVer.Text = String.Format("{0} (Revision {1})", BASSMIDI.FileVersion, BASSMIDI.FilePrivatePart);
            CompiledOn.Text = GetLinkerTime(Assembly.GetExecutingAssembly(), TimeZoneInfo.Utc).ToString();

            if (Environment.Is64BitOperatingSystem == true) // If OS is 64-bit, show "64-bit"
            { 
                WinName.Text = String.Format("{0} ({1})", CI.OSFullName, "64-bit");
            }
            else // Else, show "32-bit"
            {
                WinName.Text = String.Format("{0} ({1})", CI.OSFullName, "32-bit");
            }

            if (Environment.OSVersion.Version.Major == 10) // If OS is Windows 10, get UBR too
            {
                WinVer.Text = String.Format("{0}.{1}.{2} (Update Build Revision {3})",
                   Environment.OSVersion.Version.Major.ToString(), Environment.OSVersion.Version.Minor.ToString(),
                   Environment.OSVersion.Version.Build.ToString(), CurrentVerKey.GetValue("UBR", 0).ToString());
            }
            else // Else, give normal version number
            {
                WinVer.Text = String.Format("{0}.{1}.{2}",
                   Environment.OSVersion.Version.Major.ToString(), Environment.OSVersion.Version.Minor.ToString(),
                   Environment.OSVersion.Version.Build.ToString());
            }
        }

        private void OKClose_Click(object sender, EventArgs e)
        {
            CurrentVerKey.Close();
            Close();
        }

        private void GitHubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(GitHubLink.Text);
        }

        private void LicenseFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Keppy's Synthesizer\\license.txt");
        }

        private string CPUArch(int Value)
        {
            if (Value == 0)
                return "x86";
            else if (Value == 6)
                return "IA64";
            else if (Value == 9)
                return "x64";
            else
                return "N/A";
        }

        private void CTC_Click(object sender, EventArgs e)
        {
            ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT * FROM CIM_Processor");
            ManagementObjectSearcher mosGPU = new ManagementObjectSearcher("SELECT * FROM CIM_VideoController");

            String cpubit = "32";
            Int32 cpuclock = 0;
            String cpumanufacturer = "Unknown";
            String cpuname = "Unknown";
            String gpuchip = "Unknown";
            String gpuname = "Unknown";
            String gpuver = "N/A";
            String gpuvram = "0";
            String enclosure = "Unknown";
            String Frequency = "";
            Int32 coreCount = 0;

            // Get CPU info
            foreach (ManagementObject moProcessor in mosProcessor.Get())
            {
                cpuclock = int.Parse(moProcessor["maxclockspeed"].ToString());
                cpubit = CPUArch(int.Parse(moProcessor["Architecture"].ToString()));
                cpuname = moProcessor["name"].ToString();
                cpumanufacturer = moProcessor["manufacturer"].ToString();
                coreCount += int.Parse(moProcessor["NumberOfCores"].ToString());
            }
            // Get GPU info
            foreach (ManagementObject moGPU in mosGPU.Get())
            {
                try
                {
                    gpuchip = moGPU["VideoProcessor"].ToString();
                    gpuname = moGPU["Name"].ToString();
                    gpuvram = (long.Parse(moGPU["AdapterRAM"].ToString()) / 1048576).ToString();
                    gpuver = moGPU["DriverVersion"].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            if (cpuclock < 1000)
                Frequency = String.Format("{0}MHz", cpuclock);
            else
                Frequency = String.Format("{0}GHz", ((float)cpuclock / 1000).ToString("0.00"));

            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Keppy's Synthesizer Information Dialog\n\n", DriverVer.Text));
            sb.Append("== Driver info =================================================\n");
            sb.Append(String.Format("Driver version: {0}\n", DriverVer.Text));
            sb.Append(String.Format("BASS version: {0}\n", BASSVer.Text));
            sb.Append(String.Format("BASSMIDI version: {0}\n", BASSMIDIVer.Text));
            sb.Append(String.Format("Compiled on: {0}\n\n", CompiledOn.Text));
            sb.Append("== Windows installation info ===================================\n");
            sb.Append(String.Format("Name: {0}\n", WinName.Text));
            sb.Append(String.Format("Version: {0}\n\n", WinVer.Text));
            sb.Append("== Computer info ===============================================\n");
            sb.Append(String.Format("Processor: {0} ({1})\n", cpuname, cpubit));
            sb.Append(String.Format("Processor info: {1} cores and {2} threads, running at {3}\n", cpumanufacturer, coreCount, Environment.ProcessorCount, Frequency));
            sb.Append(String.Format("Graphics card: {0}\n", gpuchip));
            sb.Append(String.Format("Graphics card info: {0}MB VRAM, driver version {1}\n\n", gpuvram, gpuver));
            sb.Append("================================================================\n");
            sb.Append(String.Format("End of info. Got them on {0}.", DateTime.Now.ToString()));
            Clipboard.SetText(sb.ToString());
            sb = null;

            MessageBox.Show("Copied to clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CFU_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                Functions.CheckForUpdates(true, false);
            }
            else
            {
                Functions.CheckForUpdates(false, false);
            }
        }
    }
}
