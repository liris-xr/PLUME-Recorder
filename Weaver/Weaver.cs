using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using UnityEditor;
using UnityEditor.Compilation;

namespace PLUME.Weaver
{
    [InitializeOnLoad]
    public class Weaver
    {
        private static Weaver _instance;
        
        private readonly WeaverModule[] _modules;
        
        private Assembly[] _assemblies;
        
        static Weaver()
        {
            _instance = new Weaver();
        }
        
        public Weaver()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var modulesType = executingAssembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(WeaverModule)) && !t.IsAbstract);
            _modules = modulesType.Select(Activator.CreateInstance).Cast<WeaverModule>().ToArray();
            
            UpdateAssemblies();
        }

        [MenuItem("PLUME/Weave Assemblies")]
        public static void WeaveAssemblies()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
            AssetDatabase.Refresh();
        }

        private void UpdateAssemblies()
        {
            _assemblies = CompilationPipeline.GetAssemblies();
        }

        private void OnAssemblyCompilationFinished(string assemblyRelativePath, CompilerMessage[] messages)
        {
            UpdateAssemblies();
            
            var assemblyDirectory = Path.GetDirectoryName(assemblyRelativePath);
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyRelativePath);

            // Exclude itself
            if (assemblyName == System.Reflection.Assembly.GetExecutingAssembly().GetName().Name)
                return;

            if (assemblyDirectory == null || !assemblyDirectory.StartsWith("Library\\ScriptAssemblies"))
                return;

            WeaveAssembly(assemblyRelativePath);
        }

        private void WeaveAssembly(string assemblyPath)
        {
            var readerParameters = GetReaderParameters(assemblyPath);
            var writerParameters = GetWriterParameters();

            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyStream, readerParameters);

            foreach (var module in _modules)
            {
                module.VisitAssembly(assemblyDefinition);
            }

            assemblyDefinition.Write(writerParameters);
        }

        private static ReaderParameters GetReaderParameters(string assemblyPath)
        {
            return new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = new AssemblyResolver(assemblyPath),
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            };
        }

        private static WriterParameters GetWriterParameters()
        {
            return new WriterParameters
            {
                WriteSymbols = true,
                SymbolWriterProvider = new PdbWriterProvider()
            };
        }
    }
}