// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace eely.gitmoh
{
    internal class Config
    {
        public const string CONFIG_FILE_NAME = ".gitmoh";
        public const string COMMON_ROOT = "common-root";
        public const string MODULES = "modules";

        public Dictionary<string, ModuleDefinition> ModuleDict { get; protected set; }
        public DirectoryInfo RootRepoDir { get; protected set; }
        public string UserGlobalConfigFileName => 
            field ??= GetUserGlobalConfigFileName();
        public string RepoConfigFileName =>
            Path.Combine(RootRepoDir.FullName, CONFIG_FILE_NAME);

        public string CommonRoot { get; set; }

        public Config(string rootRepositoryDir)
        {
            RootRepoDir = new DirectoryInfo(rootRepositoryDir);
            ModuleDict = new Dictionary<string, ModuleDefinition>();
        }

        public bool LoadRepoConfig()
        {
            if (!File.Exists(RepoConfigFileName))
                return false;
            Load(RepoConfigFileName);
            return true;
        }

        public void DeleteUserGlobalConfigFile()
        {
            File.Delete(UserGlobalConfigFileName);
        }

        public bool LoadUserGlobalConfig()
        {
            if (!File.Exists(UserGlobalConfigFileName))
                return false;
            Load(UserGlobalConfigFileName);
            return true;
        }

        public void SaveUserGlobalConfig() =>
            Save(UserGlobalConfigFileName);

        public void SaveRepoConfig() =>
            Save(RepoConfigFileName);

        private void Save(string configFileName)
        {
            YamlMappingNode rootNode = new YamlMappingNode();
            YamlMappingNode modulesNode = new YamlMappingNode();
            rootNode.Add(COMMON_ROOT, CommonRoot);

            rootNode.Add(MODULES, modulesNode);
            foreach (var module in ModuleDict.Values)
                module.ToYaml(modulesNode);
            YamlDocument doc = new YamlDocument(rootNode);            
            YamlStream str = new YamlStream(doc);

            using (StreamWriter sw = new StreamWriter(File.Create(UserGlobalConfigFileName)))
                str.Save(sw, false);
        }

        private void Load(string configFileName)
        {
            if (!File.Exists(configFileName))
                throw new GitMohException("Unable to find gitmoh config file");
            Console.WriteLine($"Found gitmoh config file at '{configFileName}'");

            YamlStream str = new YamlStream();
            using (StreamReader sr = new StreamReader(configFileName))
                str.Load(sr);

            YamlMappingNode root = str.Documents[0].RootNode as YamlMappingNode;
            CommonRoot = root.GetMappingStringValueOrDefault(COMMON_ROOT);

            YamlMappingNode modulesNode = root.GetMappingOrDefault(MODULES);
            if (modulesNode != null)
            {
                foreach (var node in modulesNode.Children)
                {
                    string moduleName = (node.Key as YamlScalarNode).Value;
                    ModuleDefinition newModule = new ModuleDefinition(RootRepoDir, moduleName, node.Value as YamlMappingNode);
                    if (String.IsNullOrEmpty(newModule.CommonPath))
                        newModule.CommonPath = Path.Combine(CommonRoot, moduleName); 
                   ModuleDict.Add(moduleName, newModule);
                }
            }
        }

        private string GetUserGlobalConfigFileName()
        {
            // Get last folder name
            string mainName = GetLastFolderNameLower(RootRepoDir.FullName);
            // Get the hash of the full folder name
            byte[] folderNameHash = System.Security.Cryptography.SHA256.HashData(UTF8Encoding.UTF8.GetBytes(RootRepoDir.FullName.ToLower()));
            string shortHash = BitConverter.ToString(folderNameHash).Replace("-", string.Empty).Substring(0, 6);
            string globalConfigFileName = $"{mainName}_{shortHash}";

            // Ensure global path
            string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), CONFIG_FILE_NAME);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                File.WriteAllText(Path.Combine(folderName, "_readme.txt"), "User configuration file of gitmoh - Git MOduleHelper. https://eely.eu");
            }

            return Path.Combine(folderName, globalConfigFileName);
        }

        public static string GetLastFolderNameLower(string path) =>
             path.Replace('\\', '/').Split('/').Last().ToLower();
    }
}
