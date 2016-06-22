using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace winbuilder {
    class Program {

        public static string[] SOURCE_EXTENSIONS = { ".c", ".cc", ".cpp" };

        static void Main(string[] args) {

            string mainSource = "NULL";
            string sourceDir = "NULL";
            string[] includeDirs;
            string[] libpathsDirs;
            string[] libs;
            string executableDir = "NULL";
            string objectDir = "NULL";
            string subsystem = "CONSOLE";

            Console.WriteLine("Opening build file.");

            string line;
            System.IO.StreamReader buildfile = new System.IO.StreamReader("config.bf");
            List<string> tempIncludeDirs = new List<string>();
            List<string> tempLibpathDirs = new List<string>();
            List<string> tempLibs = new List<string>();
            while ((line = buildfile.ReadLine()) != null) {
                if(!string.IsNullOrWhiteSpace(line) && line[0] != '#') {
                    string[] sline = line.Split(':');
                    switch (sline[0]) {
                        case "SRC_PATH": {
                            Console.WriteLine("SRC_PATH: {0}", sline[1]);
                            sourceDir = sline[1];
                            break;
                        }
                        case "MAIN_SRC": {
                            Console.WriteLine("MAIN_SRC: {0}", sline[1]);
                            mainSource = sline[1];
                            break;
                        }
                        case "INC_PATH": {
                            Console.WriteLine("Add INC_PATH: {0}", sline[1]);
                            tempIncludeDirs.Add(sline[1]);
                            break;
                        }
                        case "EXE_PATH": {
                            Console.WriteLine("EXE_PATH: {0}", sline[1]);
                            executableDir = sline[1];
                            break;
                        }
                        case "OBJ_PATH": {
                            Console.WriteLine("OBJ_PATH: {0}", sline[1]);
                            objectDir = sline[1];
                            break;
                        }
                        case "LIB_PATH": {
                            Console.WriteLine("Add LIB_PATH: {0}", sline[1]);
                            tempLibpathDirs.Add(sline[1]);
                            break;
                        }
                        case "LIB": {
                            Console.WriteLine("Add LIB: {0}", sline[1]);
                            tempLibs.Add(sline[1]);
                            break;
                        }
                        case "SUB_SYS": {
                            Console.WriteLine("SUB_SYS: {0}", sline[1]);
                            subsystem = sline[1];
                            break;
                        }
                        default : {
                            Console.WriteLine("Failed to parse line: '{0}'.", line);
                            break;
                        }
                    }
                }

            }
            includeDirs = tempIncludeDirs.ToArray();
            libpathsDirs = tempLibpathDirs.ToArray();
            libs = tempLibs.ToArray();

            if (mainSource.Equals("NULL")
                || sourceDir.Equals("NULL")
                || includeDirs.Length == 0
                || libpathsDirs.Length == 0
                || libs.Length == 0
                || executableDir.Equals("NULL")
                || objectDir.Equals("NULL")) {
                Console.WriteLine("ERROR VARIABLE NOT SET IN BUILD FILE.");
                throw new FileLoadException();
            }


            buildfile.Close();

            string compileCommand = "cl /EHsc /MD " + mainSource;

            if (Directory.Exists(sourceDir)) { 
                string[] sources = GetSources(sourceDir);
                foreach (string source in sources) {
                    FileInfo info = new FileInfo(source);
                    Console.WriteLine("Found: '{0}', {1} bytes.", source, info.Length);
                    
                    if (source.Equals(mainSource))
                        continue;
                    compileCommand = compileCommand + " " + source;
                }
                compileCommand = compileCommand + " /Fe.\\" + executableDir;
                compileCommand = compileCommand + " /Fo.\\" + objectDir;
                foreach (string includeDir in includeDirs)
                    compileCommand = compileCommand + " /I.\\" + includeDir;

                compileCommand = compileCommand + " /link";
                foreach (string libPathDir in libpathsDirs)
                    compileCommand = compileCommand + " /LIBPATH:.\\" + libPathDir;

                foreach (string lib in libs)
                    compileCommand = compileCommand + " " + lib;
                compileCommand = compileCommand + " /SUBSYSTEM:" + subsystem;
                
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/C w:\\shell.bat & " + compileCommand;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                Console.WriteLine("{0}", output);
            } else {
                Console.WriteLine("'{0}' source directory not found.", args[0]);
            }
            Console.ReadKey();
        }
        public static string[] GetSources(string targetDirectory) {

            List<string> sources = new List<string>();
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries) { 
                if(SOURCE_EXTENSIONS.Contains(Path.GetExtension(fileName)))
                    sources.Add(fileName);
            }

            return sources.ToArray();

        }
    }
}
