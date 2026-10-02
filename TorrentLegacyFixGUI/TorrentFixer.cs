using System;
using System.IO;
using SoulsFormats;
using SoulsFormats.Cryptography;

namespace TorrentLegacyFixGUI
{
    public class TorrentFixer
    {
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


        public string Patch(string regulationPath)
        {
            try
            {
                string backupPath = regulationPath + ".torrentbackup";

                string donorPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "vanilla-regulation.bin"
                );


                string npcDefPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "NpcParam.xml"
                );

                string rideDefPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "RideParam.xml"
                );


                if (!File.Exists(donorPath))
                    return "ERROR: vanilla-regulation.bin not found.";

                if (!File.Exists(npcDefPath) ||
                    !File.Exists(rideDefPath))
                {
                    return "ERROR: PARAM definition files missing.";
                }


                if (File.Exists(backupPath))
                {
                    return
                        "ERROR: Backup already exists.\r\n\r\n" +
                        backupPath;
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


                int added = 0;


                foreach (int id in rideIds)
                {
                    if (rideParam[id] == null &&
                        donorRide[id] != null)
                    {
                        rideParam.Rows.Add(
                            new PARAM.Row(donorRide[id])
                        );

                        added++;
                    }
                }


                foreach (int id in npcIds)
                {
                    if (npcParam[id] == null &&
                        donorNpc[id] != null)
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