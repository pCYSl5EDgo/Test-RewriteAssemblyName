using System;
using System.Collections.Generic;
using Mono.Cecil;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MicroBatchFramework;
using Microsoft.Extensions.Hosting;

namespace post
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder().RunBatchEngineAsync<Runner>(args);
        }
    }

    public class Runner : BatchBase
    {
        private static string[] InterpretDirectory(string directory) => directory.StartsWith('@') ? File.ReadAllLines(directory.Substring(1)) : new[] { directory };

        [Command("rewrite", "Edit the DLLs")]
        public int Rewrite
        (
            [Option(0)] string directory
            //,[Option(1)] string outputDirectory
        )
        {
            var directories = InterpretDirectory(directory);

            var list = Collect(directories, true);

            var oldName = new List<string>(list.Count);

            for (var i = 0; i < list.Count; i++)
            {
                var name = new string(Enumerable.Repeat('a', i + 2).ToArray());
                oldName.Add(list[i].Name.Name);
                list[i].Name.Name = name;
            }

            foreach (var assemblyDefinition in list)
            {
                assemblyDefinition.Write();
            }
/*
            for (int i = 0; i < list.Count; i++)
            {
                string ext = ".dll";
                if (oldName[i] == "E")
                    ext = ".exe";
                list[i].Write(Path.Combine(outputDirectory, ));
            }*/

            return 0;
        }

        private static List<AssemblyDefinition> Collect(string[] directories, bool isReadWrite)
        {
            var list = new List<AssemblyDefinition>(directories.Length * 2);
            var readerParameters = new ReaderParameters()
            {
                AssemblyResolver = new DefaultAssemblyResolver(),
                ReadWrite = isReadWrite,
            };
            foreach (var directory in directories)
            {
                foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                {
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(file, readerParameters);
                    list.Add(assemblyDefinition);
                }
            }
            return list;
        }
    }
}
