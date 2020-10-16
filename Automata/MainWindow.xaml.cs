using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using Path = System.IO.Path;

namespace Automata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Serializable]
    public partial class MainWindow : Window
    {

        private string mWatchPath = @"F:\Development\Tools-Automata-Scripts" ;
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private Assembly mScriptsAssembly = null;
        private AppDomain mScriptsDomain = null;
        public MainWindow()
        {
            InitializeComponent();
            InitializeFileSystemWatcher();
            InitializeAppDomain();
        }

        private void InitializeAppDomain()
        {
            FileInfo fInfoDll = new FileInfo("Scripts.dll");
            FileInfo fInfoPdb = new FileInfo("Scripts.pdb");
            if (fInfoDll.Exists)
            {
                try
                {
                    //fInfoDll.Delete();
                    AppDomainSetup setup = new AppDomainSetup();
                    setup.ApplicationBase = System.Environment.CurrentDirectory;
                    Evidence evidence = AppDomain.CurrentDomain.Evidence;
                    //setup.PrivateBinPath = Path.GetDirectoryName(fInfoDll.FullName);
                    mScriptsDomain = AppDomain.CreateDomain("Automata Scripts Domain", evidence, setup);

                    Type proxyType = typeof(DomainProxy);
                    DomainProxy proxy = (DomainProxy)mScriptsDomain.CreateInstanceFromAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);

                    proxy.LoadDll(fInfoDll.FullName);
                    string retVal = proxy.GetStringTest();

                    MessageBox.Show(retVal);
                }
                catch(FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }
        }


        private void InitializeFileSystemWatcher()
        {
            
            watcher.Path = mWatchPath;

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.FileName
                                    | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*.cs";

            // Add event handlers.
            watcher.Changed += OnChanged;
            //watcher.Created += OnChanged;
            //watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;


            
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            
        }

        object mutex = new object();
        private void OnChanged(object sender, FileSystemEventArgs e)
        {

            lock (mutex)
            {
                if (mScriptsDomain != null)
                {
                    AppDomain.Unload(mScriptsDomain);
                    mScriptsAssembly = null;
                    mScriptsDomain = null;
                    GC.Collect();
                    
                }

                Thread.Sleep(5000);
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();

                //parameters.ReferencedAssemblies.Add("")
                parameters.GenerateInMemory = false;

                parameters.IncludeDebugInformation = true;

                parameters.GenerateExecutable = false;

                parameters.OutputAssembly = "Scripts.dll";

                parameters.WarningLevel = 3;

                //parameters.TempFiles = new TempFileCollection(".", true);

                DirectoryInfo dInfo = new DirectoryInfo(mWatchPath);
                String[] paths = dInfo.GetFiles("*.cs", SearchOption.AllDirectories).Select(o => o.FullName).ToArray();

                CompilerResults result = provider.CompileAssemblyFromFile(parameters, paths);


                var errors = result.Errors;
                if (errors.HasErrors)
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error);
                    }
                }
                else
                {
                    InitializeAppDomain();
                }
            }
        }
    }
}
