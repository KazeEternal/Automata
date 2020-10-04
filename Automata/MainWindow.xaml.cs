using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Automata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.ApplicationBase = fInfoDll.FullName;
                mScriptsDomain = AppDomain.CreateDomain("Scripts", null, setup);

                //using (FileStream assemblyStream = new FileStream(fInfoDll.FullName, FileMode.Open))
                //using (FileStream symbolsStream = new FileStream(fInfoPdb.FullName, FileMode.Open))
                //{
                //    byte[] bufferDll = new byte[assemblyStream.Length];
                //    byte[] bufferPdb = new byte[symbolsStream.Length];

                //    assemblyStream.Read(bufferDll, 0, (int)assemblyStream.Length);
                //    symbolsStream.Read(bufferPdb, 0, (int)assemblyStream.Length);

                //    mScriptsAssembly = mScriptsDomain.Load(bufferDll,bufferPdb);
                //    //mScriptsAssembly = mScriptsDomain.Load(AssemblyName.GetAssemblyName(fInfo.FullName));
                //    //mScriptsAssembly = Assembly.LoadFrom("Scripts.dll");

                //    // Get the type to use.
                //    Type myType = mScriptsAssembly.GetType("Test.TestClass");
                //    // Get the method to call.
                //    MethodInfo myMethod = myType.GetMethod("ReturnAString");
                //    // Create an instance.
                //    //object obj = Activator.CreateInstance(myType);
                //    // Execute the method.
                //    //myMethod.Invoke(obj, null);
                //    string retVal = (string)myMethod.Invoke(null, null);
                //    MessageBox.Show(retVal);
                //} 

                mScriptsAssembly = mScriptsDomain.Load(AssemblyName.GetAssemblyName(fInfoDll.FullName));
                //mScriptsAssembly = Assembly.LoadFrom("Scripts.dll");

                // Get the type to use.
                Type myType = mScriptsAssembly.GetType("Test.TestClass");
                // Get the method to call.
                MethodInfo myMethod = myType.GetMethod("ReturnAString");
                // Create an instance.
                //object obj = Activator.CreateInstance(myType);
                // Execute the method.
                //myMethod.Invoke(obj, null);
                string retVal = (string)myMethod.Invoke(null, null);
                MessageBox.Show(retVal);
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
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;


            
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if(mScriptsAssembly != null)
            {
                AppDomain.Unload(mScriptsDomain);
                mScriptsAssembly = null;
                mScriptsDomain = null;
            }

            Thread.Sleep(5000);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            //parameters.ReferencedAssemblies.Add("")
            parameters.GenerateInMemory = true;

            parameters.IncludeDebugInformation = true;

            parameters.GenerateExecutable = false;

            parameters.OutputAssembly = "Scripts.dll";

            parameters.WarningLevel = 3;

            parameters.TempFiles = new TempFileCollection(".", true);

            DirectoryInfo dInfo = new DirectoryInfo(mWatchPath);
            String[] paths = dInfo.GetFiles("*.cs", SearchOption.AllDirectories).Select(o => o.FullName).ToArray(); 
            
            CompilerResults result = provider.CompileAssemblyFromFile(parameters,  paths);

            

            var errors = result.Errors;
            if (errors.HasErrors)
            {
                foreach(var error in errors)
                {
                    Console.Error.WriteLine(error);
                }
            }
            else
            {
                Type t = result.CompiledAssembly.GetType("Test.TestClass");

                MethodInfo mInfo = t.GetMethod("ReturnAString");
                object retVal = mInfo.Invoke(null, null);
                MessageBox.Show(retVal.ToString());
            }
        }
    }
}
