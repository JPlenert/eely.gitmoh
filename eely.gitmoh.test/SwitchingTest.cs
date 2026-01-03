using System.IO;

namespace eely.gitmoh.test
{
    [TestClass]
    public sealed class SwitchingTest
    {
        DirectoryInfo tempDir;
        DirectoryInfo r1;
        DirectoryInfo s1;
        DirectoryInfo s2;
        DirectoryInfo commonDir;
        DirectoryInfo cs1;
        DirectoryInfo cs2;

        private void CreateSingleRepo(DirectoryInfo repoDir, string fileName, string initialFileContent)
        {
            GitExecuter.Execute("init .", repoDir.FullName);
            File.WriteAllText(Path.Combine(repoDir.FullName, fileName), initialFileContent);
            GitExecuter.Execute("add .", repoDir.FullName);
            GitExecuter.Execute("commit -m\"Initial\"", repoDir.FullName);
        }

        [TestInitialize]
        public void RepoCreate()
        {
            tempDir = Directory.CreateTempSubdirectory("eely.gitmoh");
            r1 = tempDir.CreateSubdirectory("r1");
            CreateSingleRepo(r1, "r1.txt", "Test of r1");
            s1 = tempDir.CreateSubdirectory("s1");
            CreateSingleRepo(s1, "s1.txt", "Test of s1");
            s2 = tempDir.CreateSubdirectory("s2");
            CreateSingleRepo(s2, "s2.txt", "Test of s2");

            // Adding submodules
            GitExecuter.Execute("submodule add ../s1", r1.FullName);
            GitExecuter.Execute("submodule add ../s2", r1.FullName);
            GitExecuter.Execute("add .", r1.FullName);
            GitExecuter.Execute("commit -m\"Submods added\"", r1.FullName);

            // Clone central repos
            GitExecuter.Execute("clone ./s1 commons/s1", tempDir.FullName);
            GitExecuter.Execute("clone ./s2 commons/s2", tempDir.FullName);
            commonDir = new DirectoryInfo(Path.Combine(tempDir.FullName, "commons"));
            cs1 = new DirectoryInfo(Path.Combine(commonDir.FullName, "s1"));
            cs2 = new DirectoryInfo(Path.Combine(commonDir.FullName, "s2"));

            // Make change to the central, so we can see the difference while switching
            File.WriteAllText(Path.Combine(cs1.FullName, "changed_c1.txt"), "changed c1");
            File.WriteAllText(Path.Combine(cs2.FullName, "changed_c2.txt"), "changed c2");
        }

        [TestCleanup]
        public void RepoCleanup()
        {
            try
            {
                tempDir.Delete(true);
            }
            catch { }

            try
            {
                Config cfg = new Config(r1.FullName);
                cfg.DeleteUserGlobalConfigFile();
            }
            catch { }
        }

        [TestMethod]
        public void Switch_Submodule_ImplicitAdHoc_CommonPath_Single()
        {
            Program prg = new Program();

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s1", $"--commonPath=\"{cs1.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s2", $"--commonPath=\"{cs2.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_ImplicitAdHoc_CommonRoot_Single()
        {
            Program prg = new Program();

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s1", $"--commonRoot=\"{commonDir.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s2", $"--commonRoot=\"{commonDir.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_ImplicitAdHoc_CommonPath_All()
        {
            Program prg = new Program();

            Assert.Throws<GitMohException>(() => prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*", $"--commonPath=\"{cs1.FullName}\""]));            
        }

        [TestMethod]
        public void Switch_Submodule_ImplicitAdHoc_CommonRoot_All()
        {
            Program prg = new Program();

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*", $"--commonRoot=\"{commonDir.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_ImplicitAdHoc_CommonRoot_SingleAll()
        {
            Program prg = new Program();

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s1", $"--commonRoot=\"{commonDir.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*", $"--commonRoot=\"{commonDir.FullName}\""]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_Config_TakeAffect()
        {
            Program prg = new Program();

            prg.Execute(["init", $"--path=\"{r1.FullName}\""]);
            Config cfg = new Config(r1.FullName);
            cfg.LoadUserGlobalConfig();
            Assert.HasCount(2, cfg.ModuleDict);

            // Invalid the config
            cfg.ModuleDict["s1"].CommonPath += "invalid";
            cfg.ModuleDict["s2"].CommonPath += "invalid";
            cfg.SaveUserGlobalConfig();

            Assert.Throws<GitMohException>(() => prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*", $"--commonRoot=\"{commonDir.FullName}\""]));
        }

        [TestMethod]
        public void Switch_Submodule_Config_CommonPath_Single()
        {
            Program prg = new Program();

            prg.Execute(["init", $"--path=\"{r1.FullName}\""]);

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_Config_CommonPath_All()
        {
            Program prg = new Program();

            prg.Execute(["init", $"--path=\"{r1.FullName}\""]);

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_Config_CommonRoot_Single()
        {
            Program prg = new Program();

            prg.Execute(["init", $"--path=\"{r1.FullName}\""]);

            // Modify config to user common root
            Config cfg = new Config(r1.FullName);
            cfg.LoadUserGlobalConfig();
            cfg.CommonRoot = "../commons";
            cfg.ModuleDict["s1"].CommonPath = null;
            cfg.ModuleDict["s2"].CommonPath = null;
            cfg.SaveUserGlobalConfig();

            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s1"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=s2"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }

        [TestMethod]
        public void Switch_Submodule_Config_CommonRoot_All()
        {
            Program prg = new Program();

            prg.Execute(["init", $"--path=\"{r1.FullName}\""]);

            // Modify config to user common root
            Config cfg = new Config(r1.FullName);
            cfg.LoadUserGlobalConfig();
            cfg.CommonRoot = "../commons";
            cfg.ModuleDict["s1"].CommonPath = null;
            cfg.ModuleDict["s2"].CommonPath = null;
            cfg.SaveUserGlobalConfig();


            prg.Execute(["toLink", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));

            prg.Execute(["toSubmodule", $"--path=\"{r1.FullName}\"", "--module=*"]);
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s1", "changed_c1.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(r1.FullName, "s2", "changed_c2.txt")));
        }
    }
}
