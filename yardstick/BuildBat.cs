using System;
using System.IO;
using System.Text.RegularExpressions;

namespace yardstick
{
    public class BuildBat
    {
        // Set up the path to the bat file in memory as well as the arguments for the program
        private readonly string _batPath = @"bench.bat";
        private readonly string _cbArgs = "-g_CinebenchCpuXTest=true -g_acceptDisclaimer=true >> text.txt";

        public void Build(string path, BenchChoice benchChoice)
        {
            // If they didn't include the .exe file, add it to the path
            if (!new Regex(".*Cinebench.exe$", RegexOptions.IgnoreCase).IsMatch(path))
                path += @"\Cinebench.exe";

            // If the path given exists, continue, otherwise break out
            if (File.Exists(path))
            {
                // If the bat file doesn't exist, create and close it
                if (!File.Exists(_batPath))
                    File.Create(_batPath).Close();

                // try
                // {
                    switch (benchChoice)
                    {
                        case BenchChoice.Cinebench:
                            // Write to the bat file with the path and the arguments
                            File.WriteAllText(_batPath, "\"" + path + "\" " + _cbArgs);
                            break;
                        default:
                            Console.WriteLine("Unexpected Error");
                            break;
                    }
                // }
                // Catch all exceptions since I don't want to figure out what exceptions might be thrown at the moment
                // catch (Exception e)
                // {
                //     Console.WriteLine(e);
                // }
            }
            else
                Console.WriteLine(path + " does not exist");
        }
    }

    public enum BenchChoice
    {
        Cinebench
    }
}