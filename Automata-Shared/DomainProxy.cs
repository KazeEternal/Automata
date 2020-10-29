using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Automata
{
    public class DomainProxy : MarshalByRefObject
    {
        private Assembly mScriptsAssembly = null;
        public bool LoadDll(string path)
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            //mScriptsAssembly = System.Reflection.Assembly.Load(bytes);
            mScriptsAssembly = Assembly.LoadFile(path);
            return true;
        }

        public string GetStringTest()
        {
            Type myType = mScriptsAssembly.GetType("Test.TestClass");
            // Get the method to call.
            MethodInfo myMethod = myType.GetMethod("ReturnAString");

            return (string)((string)myMethod.Invoke(null, null)).Clone();
            
        }
    }
}
