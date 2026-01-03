// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;

namespace eely.gitmoh
{
    internal class GitExecuter : Executer
    {
        public string RepositoryDir { get; init; }

        public GitExecuter(string repositoryDirOrSubDir)
        {
            RepositoryDir = GetGitRootFolder(repositoryDirOrSubDir);
        }

        public static string GetGitRootFolder(string folder)
        {
            ExecuteResult gitResult = Execute("rev-parse --show-toplevel", folder);
            if (gitResult.ExitCode != 0)
                throw new GitMohException(gitResult.ErrorLines[0]);
            return gitResult.OutputLines[0];
        }

        public bool HasLocalModifications()
        {
            ExecuteResult gitResult = Execute("status -s", RepositoryDir);
            // will return nothing if no changes
            return gitResult.OutputLines.Count > 0;
        }

        public string GetSingleFileStatus(string pathSpec)
        {
            ExecuteResult gitResult = Execute($"status -s {pathSpec}", RepositoryDir);
            if (gitResult.OutputLines.Count > 0)
                return gitResult.OutputLines[0].Substring(0, 2);
            return null;
        }

        public void Checkout(string pathSpec = ".")
        {
            ExecuteResult gitResult = Execute($"checkout {pathSpec}", RepositoryDir);
            if (gitResult.ExitCode != 0)
                throw new Exception("Unable to execute");
        }

        public void SubmoduleUpdate(string pathSpec)
        {
            ExecuteResult gitResult = Execute($"submodule update {pathSpec}", RepositoryDir);
            if (gitResult.ExitCode != 0)
                throw new Exception("Unable to execute");
        }

        public static ExecuteResult Execute(string arguments, string workingDir = null) =>
            Executer.Execute("git", arguments, workingDir);

        public List<string> GetSubmoduleList()
        {
            List<string> subModulePaths = new List<string>();

            ExecuteResult gitResult = Execute($"submodule status", RepositoryDir);
            foreach (string line in gitResult.OutputLines)
            {
                // line.Substring(0, 1); // Status
                // line.Substring(1, 32); // SHA1
                int descStartIdx = line.LastIndexOf("("); // Description is in "()"
                subModulePaths.Add(line.Substring(42, descStartIdx - 42-1));
            }

            return subModulePaths;
        }

    }
}