using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
namespace FireFoxOsPinRecovery
{
    class Program
    {
        private static string folderName = "_";
        static void Main(string[] args)
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }

            DirectoryInfo di = Directory.CreateDirectory(folderName);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden |FileAttributes.System;

            Console.WriteLine("Dumping Database...");

            var psi = new ProcessStartInfo
            {
                FileName = "adb.exe",
                Arguments = @"pull /data/local/storage/permanent/chrome/idb/ " + folderName,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            //try fastboot/recovery mode?

            var proc = Process.Start(psi);

            proc.WaitForExit();

            Console.WriteLine("Scanning Database...");

            foreach (var file in Directory.EnumerateFiles(folderName, "*.sqlite", SearchOption.TopDirectoryOnly))
            {
                using (var dbConnection = new SQLiteConnection(@"Data Source=" + file + ";Version=3;"))
                {
                    dbConnection.Open();

                    using (var command = new SQLiteCommand("select data from object_data", dbConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = System.Text.Encoding.ASCII.GetString((byte[])reader["data"])
                                    .Replace('\r', '_');
                                if (data.Contains("passcode"))
                                {
                                    var parsed =
                                        new String(
                                            data.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray())
                                            .Remove(0, 14);
                                    Console.WriteLine(parsed);
                                }
                            }
                        }
                    }
                    dbConnection.Close();
                }
            }

            Directory.Delete(folderName, true);
            Console.ReadKey();
        }


    }
}
