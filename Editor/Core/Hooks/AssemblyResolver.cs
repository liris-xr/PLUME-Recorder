using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;

namespace PLUME.Editor.Core.Hooks
{
    public class AssemblyResolver : DefaultAssemblyResolver
    {
        public AssemblyResolver(string assemblyRelativePath)
        {
            var asm = CompilationPipeline.GetAssemblies().First(x => x.outputPath == assemblyRelativePath);

            foreach (var str in GetAssemblyDependencies(asm))
            {
                AddSearchDirectory(str);
            }
            
            // Include DLLs such as UnityEngine.dll and UnityEditor.dll
            AddSearchDirectory(Path.Join(Path.GetDirectoryName(EditorApplication.applicationPath), "/Data/Managed"));
        }

        private static HashSet<string> GetAssemblyDependencies(Assembly asm)
        {
            HashSet<string> dependencies = new()
            {
                Path.GetDirectoryName(asm.outputPath),
                InternalEditorUtility.GetEngineCoreModuleAssemblyPath()
            };

            foreach (var references in asm.compiledAssemblyReferences)
            {
                var directory = Path.GetDirectoryName(references);
                dependencies.Add(directory);
            }

            return dependencies;
        }
    }
}