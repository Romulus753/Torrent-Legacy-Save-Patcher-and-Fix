using System;
using System.IO;
using System.Runtime.InteropServices;
using SoulsFormats;
using SoulsFormats.Cryptography;

namespace TorrentLegacyFixGUI
{
    public class TorrentFixer
    {
        public string InitializeGameFolder(string gameFolder)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameFolder) ||
                    !Directory.Exists(gameFolder))
                {
                    return "ERROR: The selected folder does not exist.";
                }

                string exePath =
                    Path.Combine(gameFolder, "eldenring.exe");

                string regulationPath =
                    Path.Combine(gameFolder, "regulation.bin");

                string oodlePath =
                    Path.Combine(gameFolder, "oo2core_6_win64.dll");

                if (!File.Exists(exePath))
                {
                    return
                        "ERROR: This does not appear to be the Elden Ring Game folder.\r\n\r\n" +
                        "eldenring.exe was not found.\r\n\r\n" +
                        "Selected folder:\r\n" +
                        gameFolder +
                        "\r\n\r\n" +
                        "Path checked:\r\n" +
                        exePath +
                        "\r\n\r\n" +
                        "Please select the folder containing eldenring.exe.";
                }

                if (!File.Exists(regulationPath))
                {
                    return
                        "ERROR: This does not appear to be a complete Elden Ring Game folder.\r\n\r\n" +
                        "regulation.bin was not found.";
                }

                if (!File.Exists(oodlePath))
                {
                    return
                        "ERROR: The required Elden Ring Oodle library was not found.\r\n\r\n" +
                        "Missing:\r\n" +
                        "oo2core_6_win64.dll";
                }

                if (Oodle.Oodle6Ptr == IntPtr.Zero)
                {
                    IntPtr oodleHandle =
                        System.Runtime.InteropServices.NativeLibrary.Load(oodlePath);

                    if (oodleHandle == IntPtr.Zero)
                    {
                        return
                            "ERROR: The Elden Ring Oodle library could not be loaded.";
                    }

                    Oodle.Oodle6Ptr = oodleHandle;
                }

                return
                    "SUCCESS: Elden Ring installation detected.\r\n\r\n" +
                    "eldenring.exe found\r\n" +
                    "regulation.bin found\r\n" +
                    "Oodle library found\r\n\r\n" +
                    "The files in your Elden Ring Game folder will NOT be modified.";
            }
            catch (Exception ex)
            {
                return
                    "ERROR: Could not initialize the Elden Ring Game folder.\r\n\r\n" +
                    ex.Message;
            }
        }

        public string Inspect(string regulationPath)
        {
            try
            {
                string result = "Torrent status:\r\n\r\n";

                BND4 regulation =
                    RegulationDecryptor.DecryptERRegulation(regulationPath);

                BinderFile? npcFile = regulation.Files.Find(f =>
                    f.Name.EndsWith(
                        "NpcParam.param",
                        StringComparison.OrdinalIgnoreCase));

                BinderFile? rideFile = regulation.Files.Find(f =>
                    f.Name.EndsWith(
                        "RideParam.param",
                        StringComparison.OrdinalIgnoreCase));

                if (npcFile == null || rideFile == null)
                {
                    return "ERROR: Required PARAM files not found.";
                }

                PARAM npcParam = PARAM.Read(npcFile.Bytes);
                PARAM rideParam = PARAM.Read(rideFile.Bytes);

                int[] rideIds =
                {
                    80020,
                    80030,
                    80040,
                    80050
                };

                int[] npcIds =
                {
                    80020000,
                    80030000,
                    80040000,
                    80050000
                };

                foreach (int id in rideIds)
                {
                    result +=
                        $"RideParam {id}: " +
                        $"{(rideParam[id] == null ? "MISSING" : "PRESENT")}\r\n";
                }

                result += "\r\n";

                foreach (int id in npcIds)
                {
                    result +=
                        $"NpcParam {id}: " +
                        $"{(npcParam[id] == null ? "MISSING" : "PRESENT")}\r\n";
                }

                result += "\r\n";

                bool needsFix = false;

                foreach (int id in rideIds)
                {
                    if (rideParam[id] == null)
                        needsFix = true;
                }

                foreach (int id in npcIds)
                {
                    if (npcParam[id] == null)
                        needsFix = true;
                }

                result += needsFix
                    ? "This regulation appears to need the Torrent fix."
                    : "Torrent fix already appears installed.";

                return result;
            }
            catch (Exception ex)
            {
                return "ERROR:\r\n" + ex.Message;
            }
        }

        public string Patch(string regulationPath, string donorPath)
        {
            try
            {
                string backupPath = regulationPath + ".torrentbackup";

                string npcDefPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "NpcParam.xml"
                );

                string rideDefPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "RideParam.xml"
                );

                string targetFullPath = Path.GetFullPath(regulationPath);
                string donorFullPath = Path.GetFullPath(donorPath);

                if (string.Equals(
                    targetFullPath,
                    donorFullPath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return
                        "ERROR:\r\n\r\n" +
                        "The vanilla regulation.bin cannot be the same file as the regulation being patched.\r\n\r\n" +
                        "Please select a separate legacy or modded regulation.bin.";
                }

                if (!File.Exists(donorPath))
                {
                    return
                        "ERROR: The current vanilla regulation.bin could not be found.";
                }

                if (!File.Exists(npcDefPath) ||
                    !File.Exists(rideDefPath))
                {
                    return "ERROR: PARAM definition files missing.";
                }

                if (File.Exists(backupPath))
                {
                    return
                        "ERROR: A backup already exists.\r\n\r\n" +
                        "Torrent Legacy Fix will not overwrite an existing backup:\r\n\r\n" +
                        backupPath +
                        "\r\n\r\n" +
                        "If you are retrying after a failed patch, you may delete the existing " +
                        ".torrentbackup file and try again.\r\n\r\n" +
                        "Only delete the backup if you are sure you no longer need it to restore " +
                        "your original regulation.bin.";
                }

                File.Copy(regulationPath, backupPath);

                BND4 regulation =
                    RegulationDecryptor.DecryptERRegulation(regulationPath);

                BND4 donor =
                    RegulationDecryptor.DecryptERRegulation(donorPath);

                BinderFile? npcFile = regulation.Files.Find(f =>
                    f.Name.EndsWith(
                        "NpcParam.param",
                        StringComparison.OrdinalIgnoreCase));

                BinderFile? rideFile = regulation.Files.Find(f =>
                    f.Name.EndsWith(
                        "RideParam.param",
                        StringComparison.OrdinalIgnoreCase));

                BinderFile? donorNpcFile = donor.Files.Find(f =>
                    f.Name.EndsWith(
                        "NpcParam.param",
                        StringComparison.OrdinalIgnoreCase));

                BinderFile? donorRideFile = donor.Files.Find(f =>
                    f.Name.EndsWith(
                        "RideParam.param",
                        StringComparison.OrdinalIgnoreCase));

                if (npcFile == null ||
                    rideFile == null ||
                    donorNpcFile == null ||
                    donorRideFile == null)
                {
                    return "ERROR: Required PARAM files missing.";
                }

                PARAM npcParam = PARAM.Read(npcFile.Bytes);
                PARAM rideParam = PARAM.Read(rideFile.Bytes);

                PARAM donorNpc = PARAM.Read(donorNpcFile.Bytes);
                PARAM donorRide = PARAM.Read(donorRideFile.Bytes);

                PARAMDEF npcDef =
                    PARAMDEF.XmlDeserialize(npcDefPath);

                PARAMDEF rideDef =
                    PARAMDEF.XmlDeserialize(rideDefPath);

                npcParam.ApplyParamdefCarefully(npcDef);
                rideParam.ApplyParamdefCarefully(rideDef);

                donorNpc.ApplyParamdefCarefully(npcDef);
                donorRide.ApplyParamdefCarefully(rideDef);

                int[] rideIds =
                {
                    80020,
                    80030,
                    80040,
                    80050
                };

                int[] npcIds =
                {
                    80020000,
                    80030000,
                    80040000,
                    80050000
                };

                foreach (int id in rideIds)
                {
                    if (donorRide[id] == null)
                    {
                        return
                            "ERROR:\r\n\r\n" +
                            $"The current vanilla regulation.bin is missing RideParam {id}.\r\n\r\n" +
                            "Make sure your Elden Ring installation is up to date.";
                    }
                }

                foreach (int id in npcIds)
                {
                    if (donorNpc[id] == null)
                    {
                        return
                            "ERROR:\r\n\r\n" +
                            $"The current vanilla regulation.bin is missing NpcParam {id}.\r\n\r\n" +
                            "Make sure your Elden Ring installation is up to date.";
                    }
                }

                int added = 0;

                foreach (int id in rideIds)
                {
                    if (rideParam[id] == null)
                    {
                        rideParam.Rows.Add(
                            new PARAM.Row(donorRide[id])
                        );

                        added++;
                    }
                }

                foreach (int id in npcIds)
                {
                    if (npcParam[id] == null)
                    {
                        npcParam.Rows.Add(
                            new PARAM.Row(donorNpc[id])
                        );

                        added++;
                    }
                }

                rideFile.Bytes = rideParam.Write();
                npcFile.Bytes = npcParam.Write();

                RegulationDecryptor.EncryptERRegulation(
                    regulationPath,
                    regulation
                );

                BND4 verify =
                    RegulationDecryptor.DecryptERRegulation(regulationPath);

                BinderFile? verifyNpcFile = verify.Files.Find(f =>
                    f.Name.EndsWith(
                        "NpcParam.param",
                        StringComparison.OrdinalIgnoreCase));

                BinderFile? verifyRideFile = verify.Files.Find(f =>
                    f.Name.EndsWith(
                        "RideParam.param",
                        StringComparison.OrdinalIgnoreCase));

                if (verifyNpcFile == null || verifyRideFile == null)
                {
                    return "FAILED: Verification could not find PARAM files.";
                }

                PARAM verifyNpc = PARAM.Read(verifyNpcFile.Bytes);
                PARAM verifyRide = PARAM.Read(verifyRideFile.Bytes);

                if (verifyNpc[80020000] == null ||
                    verifyNpc[80030000] == null ||
                    verifyNpc[80040000] == null ||
                    verifyNpc[80050000] == null ||
                    verifyRide[80020] == null ||
                    verifyRide[80030] == null ||
                    verifyRide[80040] == null ||
                    verifyRide[80050] == null)
                {
                    return "FAILED: Patch verification failed.";
                }

                return
                    "SUCCESS!\r\n\r\n" +
                    "Torrent fix installed and verified.\r\n\r\n" +
                    $"Added {added} Torrent rows.\r\n\r\n" +
                    "Backup created:\r\n" +
                    backupPath;
            }
            catch (Exception ex)
            {
                return
                    "FAILED:\r\n\r\n" +
                    ex.ToString();
            }
        }

        public string RestoreBackup(string regulationPath)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(regulationPath);

                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    return
                        "ERROR:\r\n\r\n" +
                        "Your regulation.bin is set to read only.\r\n\r\n" +
                        "Right-click the file, select Properties, temporarily disable Read-only, and try again.";
                }

                string backupPath = regulationPath + ".torrentbackup";

                if (!File.Exists(backupPath))
                {
                    return "ERROR: No backup file found.";
                }

                File.Copy(
                    backupPath,
                    regulationPath,
                    true
                );

                return
                    "SUCCESS!\r\n\r\n" +
                    "Original regulation restored.";
            }
            catch (Exception ex)
            {
                return
                    "FAILED:\r\n\r\n" +
                    ex.ToString();
            }
        }
    }
}