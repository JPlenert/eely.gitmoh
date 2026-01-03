// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;
using System.IO;

namespace eely.gitmoh
{
    internal class ModuleHelper
    {
        private ModuleDefinition _modDef;

        public ModuleStatus Status
        {
            get
            {
                if (!_modDef.SubmoduleDir.Exists)
                {
                    GitExecuter ge = new GitExecuter(_modDef.RootRepoDir.FullName);

                    if (ge.GetSingleFileStatus(_modDef.SubmoduleDir.FullName) == " D")
                        return ModuleStatus.Deleted;
                    else
                        return ModuleStatus.NotExisting;
                }
                else if (OperatingSystem.IsWindows() && _modDef.SubmoduleDir.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    return ModuleStatus.Junctioned;
                else if (_modDef.SubmoduleDir.ResolveLinkTarget(true) != null)
                    return ModuleStatus.SymLinked;
                else
                {
                    GitExecuter ge = new GitExecuter(_modDef.SubmoduleDir.FullName);
                    if (ge.HasLocalModifications())
                        return ModuleStatus.LocalwMod;
                    else
                        return ModuleStatus.LocalwoMod;
                }
            }
        }

        public ModuleHelper(ModuleDefinition def)
        {
            _modDef = def;
        }

        public static void SwitchToLinkFromModDef(ModuleDefinition modDef) =>
            new ModuleHelper(modDef).SwitchToLink();

        public void SwitchToLink()
        {
            Console.WriteLine($"Switching {_modDef.Name}");
            ModuleStatus stat = Status;
            if (stat != ModuleStatus.LocalwoMod)
            {
                Console.WriteLine(" Unable to switch because of current status");
                return;
            }

            if (!Directory.Exists(_modDef.CommonDir.FullName))
                throw new GitMohException("Unable to switch because common is not existing");

            _modDef.SubmoduleDir.Refresh();
            if (_modDef.SubmoduleDir.Exists)
            {
                Console.WriteLine($" Deleting submodule folder '{_modDef.SubmoduleDir.FullName}'");
                _modDef.SubmoduleDir.Delete(true);
            }

            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine($" Creating junction to '{_modDef.CommonDir.FullName}'");
                WinJunctionPoint.Create(_modDef.SubmoduleDir.FullName, _modDef.CommonDir.FullName, true);
            }
            else
            {
                Console.WriteLine($" Creating symlink to '{_modDef.CommonDir.FullName}'");
                Executer.Execute("ln", $"-s {_modDef.CommonDir.FullName} {_modDef.SubmoduleDir.FullName}", null);
            }
        }

        public static void SwitchToSubmoduleFromModDef(ModuleDefinition modDef) =>
            new ModuleHelper(modDef).SwitchToSubmodule();

        public void SwitchToSubmodule()
        {
            Console.WriteLine($"Switching {_modDef.Name}");
            ModuleStatus stat = Status;
            if (stat != ModuleStatus.Junctioned && stat != ModuleStatus.SymLinked)
            {
                Console.WriteLine(" Unable to switch because of current status");
                return;
            }

            _modDef.SubmoduleDir.Refresh();
            if (_modDef.SubmoduleDir.Exists)
            {
                Console.WriteLine($" Deleting linked folder '{_modDef.SubmoduleDir.FullName}'");
                _modDef.SubmoduleDir.Delete(true);
            }

            Console.WriteLine($" Recreating submodule folder '{_modDef.SubmoduleDir.FullName}'");
            GitExecuter ge = new GitExecuter(_modDef.RootRepoDir.FullName);
            ge.Checkout(_modDef.SubmoduleDir.FullName);
            ge.SubmoduleUpdate(_modDef.SubmoduleDir.FullName);
        }
    }
}
