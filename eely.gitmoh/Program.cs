// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("eely.gitmoh.test")]

namespace eely.gitmoh
{
    public class Program
    {
        private ArgumentList _argList;
        private string _rootRepoFolder;

        // dotnet publish -r win-x64 -c Release
        // dotnet publish -r linux-arm64 -c Release
        public static void Main(string[] args)
        {
            PrintHeadline();
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            try
            {
                new Program().Execute(args);
            }
            catch (GitMohException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        internal void Execute(string[] args)
        {
            _argList = ArgumentList.Create(args);

            string path = _argList.GetDoubleDashArg("path");
            string command = _argList.FirstOrDefault((x) => x.LeadingDashes == 0)?.Key;
            bool adHoc = _argList.GetDoubleDashArg("adHoc") != null;
            string commonPath = _argList.GetDoubleDashArg("commonPath");
            string commonRoot = _argList.GetDoubleDashArg("commonRoot");
            string moduleName = _argList.GetDoubleDashArg("module");

            _rootRepoFolder = GitExecuter.GetGitRootFolder(path ?? Directory.GetCurrentDirectory());

            if (command == "init")
            {
                Init();
            }
            else if (command == "status")
            {
                PrintStatistics(path);
            }
            else if (command == "toLink")
            {
                foreach (ModuleDefinition modDef in LoadModuleDefinitions(moduleName, commonPath, commonRoot, adHoc))
                {
                    ModuleHelper mh = new ModuleHelper(modDef);
                    mh.SwitchToLink();
                }
            }
            else if (command == "toSubmodule")
            {
                foreach (var modDef in LoadModuleDefinitions(moduleName, null, null, adHoc, true))
                {
                    ModuleHelper mh = new ModuleHelper(modDef);
                    mh.SwitchToSubmodule();
                }
            }
            else
                Console.Error.WriteLine($"Unknown command '{command}'");
        }

        private IEnumerable<ModuleDefinition> LoadModuleDefinitions(string moduleName = null, string commonPath = null, string commonRoot = null, bool adHoc = false, bool allowEmptyPathes = false)
        {
            Config cfg = null;

            if (moduleName == null)
                throw new GitMohException("Module name must be given or '*' for all modules.");

            // Load config
            if (!adHoc)
            {
                // Get Config
                cfg = new Config(_rootRepoFolder);
                if (!cfg.LoadRepoConfig() && !cfg.LoadUserGlobalConfig())
                {
                    Console.WriteLine("No config found, using AdHoc-Mode");
                    adHoc = true;
                }
            }

            if (adHoc)
            {
                if (!allowEmptyPathes && commonPath != null && commonRoot != null)
                    throw new GitMohException("Parameter 'commonRoot' and 'commonPath' can't be set together in 'AdHoc' mode.");
                if (!allowEmptyPathes && commonPath == null && commonRoot == null)
                    throw new GitMohException("Parameter 'commonRoot' or 'commonPath' must be set in 'AdHoc' mode.");
                if (!allowEmptyPathes && commonRoot == null && moduleName == "*")
                    throw new GitMohException("Parameter 'commonRoot' must be set if module is '*' in 'AdHoc' mode.");

                GitExecuter ge = new GitExecuter(_rootRepoFolder);

                foreach (string module in ge.GetSubmoduleList().Where(x => moduleName == "*" || x == moduleName))
                {
                    string effModuleName = Config.GetLastFolderNameLower(module);
                    string effComPath = null;

                    if (!allowEmptyPathes)
                        effComPath = commonPath ?? Path.Combine(commonRoot, effModuleName);
                    yield return new ModuleDefinition(new DirectoryInfo(_rootRepoFolder), effModuleName, module, effComPath);
                }
            }
            else
            {
                foreach (var kvp in cfg.ModuleDict.Where(x => moduleName == "*" || x.Key == moduleName))
                    yield return kvp.Value;
            }
        }

        private bool Init()
        {
            GitExecuter ge = new GitExecuter(_rootRepoFolder);
            List<string> submoduleList = ge.GetSubmoduleList();

            if (submoduleList.Count == 0)
            {
                Console.Error.WriteLine("No submodules found, unable to initialize");
                return false;
            }

            bool useRepo = _argList.GetDoubleDashArg("repo") != null;

            Config config = new Config(_rootRepoFolder);

            if (useRepo)
                config.LoadRepoConfig();
            else
                config.LoadUserGlobalConfig();

            foreach (string subModule in submoduleList)
            {
                string subModuleName = Config.GetLastFolderNameLower(subModule);
                if (config.ModuleDict.Values.Any(x => x.Name == subModuleName))
                {
                    Console.WriteLine($"Module '{subModuleName}' already existing in config file. Skipping.");
                    continue;
                }
                Console.WriteLine($"Creating default entry for module '{subModuleName}'.");

                ModuleDefinition modDef = new ModuleDefinition(config.RootRepoDir, subModuleName, subModule, $"../commons/{subModuleName}");
                config.ModuleDict.Add(subModuleName, modDef);
            }

            if (useRepo)
            {
                config.SaveRepoConfig();
                Console.WriteLine($"Wrote config to '{config.RepoConfigFileName}'. Please check content!");
            }
            else
            {
                config.SaveUserGlobalConfig();
                Console.WriteLine($"Wrote config to '{config.UserGlobalConfigFileName}'. Please check content!");
            }
            return true;
        }

        private static void PrintStatistics(string configPath)
        {
            Config config = new Config(configPath);

            Console.WriteLine();
            Console.WriteLine("Status:");

            foreach (ModuleDefinition module in config.ModuleDict.Values)
            {
                ModuleHelper mh = new ModuleHelper(module);
                Console.Write($" {module.Name} -");

                ModuleStatus stat = mh.Status;

                if (stat == ModuleStatus.NotExisting)
                    Console.WriteLine($" Not Existing");
                else if (stat == ModuleStatus.Deleted)
                    Console.WriteLine($" Deleted");
                else if (stat == ModuleStatus.Junctioned)
                    Console.WriteLine($" Junction");
                else if (stat == ModuleStatus.SymLinked)
                    Console.WriteLine($" Linked");
                else if (stat == ModuleStatus.LocalwMod)
                    Console.WriteLine($" local with modifications");
                else if (stat == ModuleStatus.LocalwoMod)
                    Console.WriteLine($" local without modifications");
                else
                    throw new Exception("Invalid module status");
            }
        }

        private static void PrintHeadline()
        {
            Console.WriteLine($"gitMoH - git MOduleHelper V{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}, https://www.eely.eu");
        }

        private static void PrintHelp()
        {
            Console.WriteLine(" (c)2025-26 by Joerg Plenert, Voerde. Licensed under GPLv3.");
            Console.WriteLine("Switches Submodules to (sym)linked/junctioned folders and back.");
            Console.WriteLine("Usage:");
            Console.WriteLine(" gitmoh <Command> [<Options>]");
            Console.WriteLine("Commands:");
            Console.WriteLine(" status         Outputs the current status of all submodules");
            Console.WriteLine(" init           Initializes a configuration");
            Console.WriteLine("   --repo       config file will be created/updated in the current repository.");
            Console.WriteLine("   --user       config file will be created/updated in the user profile (default).");
            Console.WriteLine(" toLink         Changes a submodule to a linked path");
            Console.WriteLine("  --module      Name of the module");
            Console.WriteLine("  --adHoc       Use 'AdHoc' mode without configuration.");
            Console.WriteLine("  --commonPath  Specific common path of a module (AdHoc-only)");
            Console.WriteLine("  --commonRoot  Root path of common modules (AdHoc-only)");
            Console.WriteLine(" toSubmodule  Changes a linked path back to a submodule");
            Console.WriteLine("  --module    Name of the module");
            Console.WriteLine("  --adHoc     Use 'AdHoc' mode without configuration.");
        }
    }
}