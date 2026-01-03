// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace eely.gitmoh
{
    internal class Executer
    {
        public class ExecuteResult
        {
            public List<string> OutputLines = new List<string>();
            public List<string> ErrorLines = new List<string>();
            public int ExitCode;
        }

        public static ExecuteResult Execute(string file, string arguments, string workingDir = null)
        {
            ExecuteResult result = new ExecuteResult();

            Process process = new Process();
            process.StartInfo.FileName = file;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            if (workingDir != null)
                process.StartInfo.WorkingDirectory = workingDir;
            if (!process.Start())
            {
                result.ExitCode = int.MinValue;
                return result;
            }

            process.OutputDataReceived += (sender, e) => { if (!String.IsNullOrEmpty(e.Data)) result.OutputLines.Add(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (!String.IsNullOrEmpty(e.Data)) result.ErrorLines.Add(e.Data); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            result.ExitCode = process.ExitCode;

            return result;
        }
    }
}