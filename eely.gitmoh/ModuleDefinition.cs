// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace eely.gitmoh
{
    internal class ModuleDefinition
    {
        private const string SUBMODULE_PATH = "submodule_path";
        private const string COMMON_PATH = "common-path";

        public string Name { get; init; }
        public string SubmodulePath { get; init; }
        public DirectoryInfo SubmoduleDir { get; init; }
        public string CommonPath
        {
            get; set
            {
                field = value;
                if (value == null)
                    CommonDir = null;
                else
                    CommonDir = new DirectoryInfo(Path.GetFullPath(Path.Combine(RootRepoDir.FullName, CommonPath)));
            }
        }
        public DirectoryInfo CommonDir { get; private set; }
        public DirectoryInfo RootRepoDir { get; init; }

        public ModuleDefinition(DirectoryInfo rootRepoDir, string name, string submodulePath, string commonPath)
        {
            RootRepoDir = rootRepoDir;
            Name = name;
            SubmodulePath = submodulePath;
            SubmoduleDir = new DirectoryInfo(Path.GetFullPath(Path.Combine(rootRepoDir.FullName, SubmodulePath)));
            CommonPath = commonPath;
        }

        public ModuleDefinition(DirectoryInfo rootRepoDir, string name, YamlMappingNode mapping)
        {
            RootRepoDir = rootRepoDir;
            Name = name;
            foreach (KeyValuePair<YamlNode, YamlNode> itemsKvp in mapping)
            {
                string key = (itemsKvp.Key as YamlScalarNode).Value;
                string value = (itemsKvp.Value as YamlScalarNode).Value;

                if (key == SUBMODULE_PATH)
                {
                    SubmodulePath = value;
                    SubmoduleDir = new DirectoryInfo(Path.GetFullPath(Path.Combine(rootRepoDir.FullName, SubmodulePath)));
                }
                else if (key == COMMON_PATH)
                {
                    CommonPath = value;
                    CommonDir = new DirectoryInfo(Path.GetFullPath(Path.Combine(rootRepoDir.FullName, CommonPath)));
                }
                else
                    throw new NotImplementedException($"Invalid key '{key}'");
            }
        }

        public void ToYaml(YamlMappingNode yNode)
        {
            YamlMappingNode node = new YamlMappingNode();
            node.Add(SUBMODULE_PATH, SubmodulePath);
            node.Add(COMMON_PATH, CommonPath);
            yNode.Add(Name, node);
        }
    }
}
