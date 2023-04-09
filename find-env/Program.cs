using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#pragma warning disable CA1416

namespace find_env
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var versionString = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion
                    .ToString();

                Console.WriteLine($"find-env v{versionString}");
                Console.WriteLine("-------------");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  find-env conda");
                return;
            }

            // var machineEnvs = GetMachineEnvs();
            // Console.WriteLine("---------------Machine Envs---------------");
            // foreach (var item in machineEnvs)
            // {
            //     Console.WriteLine($"{item.Name}");
            //     Console.WriteLine($"\t{item.Raw}");
            //     Console.WriteLine($"\t{item.Expand}");
            //     Console.WriteLine();
            // }
            //
            // var userEnvs = GetUserEnvs();
            // Console.WriteLine("---------------User Envs---------------");
            // foreach (var item in userEnvs)
            // {
            //     Console.WriteLine($"{item.Name}");
            //     Console.WriteLine($"\t{item.Raw}");
            //     Console.WriteLine($"\t{item.Expand}");
            //     Console.WriteLine();
            // }

            // var pathExt = machineEnvs
            //     .First(e => e.Name.Equals("PATHEXT", StringComparison.InvariantCulture))
            //     .Expand;
            var pathExt = Environment.GetEnvironmentVariable("PATHEXT");

            Find(Scope.Machine, Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine),
                pathExt, args[0]);
            Find(Scope.User, Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User), pathExt,
                args[0]);
            //Console.ReadKey();
        }

        static void Find(Scope scope, string path, string pathExt, string name)
        {
            var sw = Stopwatch.StartNew();
            var pathExts = pathExt.Split(";", StringSplitOptions.RemoveEmptyEntries);
            var pathDirs = path.Split(";", StringSplitOptions.RemoveEmptyEntries);
            foreach (var pathDir in pathDirs)
            {
                foreach (var ext in pathExts)
                {
                    var full = name.Contains(".") ? name : name + ext;
                    var f = Directory.GetFiles(pathDir, full, SearchOption.TopDirectoryOnly);
                    if (f.Length != 0)
                    {
                        Console.WriteLine($"[{scope}] Find {full} in {pathDir}");
                    }
                }
            }

            sw.Stop();
            Console.WriteLine($"[{scope}] Search completed({sw.ElapsedMilliseconds}ms)");
        }

        // static List<EnvItem> GetUserEnvs()
        // {
        //     const string environmentKeyPath = @"Environment";
        //     var result = new List<EnvItem>();
        //     using RegistryKey environmentKey = Registry.CurrentUser.OpenSubKey(environmentKeyPath);
        //     Debug.Assert(environmentKey != null);
        //     foreach (var name in environmentKey.GetValueNames())
        //     {
        //         object rawValue = environmentKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames);
        //         object expandValue = environmentKey.GetValue(name, "", RegistryValueOptions.None);
        //         result.Add(new EnvItem(Scope.User, name, rawValue?.ToString(), expandValue?.ToString()));
        //     }
        //
        //     return result;
        // }
        //
        // static List<EnvItem> GetMachineEnvs()
        // {
        //     const string environmentKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        //     var result = new List<EnvItem>();
        //     using RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey(environmentKeyPath);
        //     Debug.Assert(environmentKey != null);
        //     foreach (var name in environmentKey.GetValueNames())
        //     {
        //         object rawValue = environmentKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames);
        //         object expandValue = environmentKey.GetValue(name, "", RegistryValueOptions.None);
        //         result.Add(new EnvItem(Scope.Machine, name, rawValue?.ToString(), expandValue?.ToString()));
        //     }
        //
        //     return result;
        // }
        //
        // public record EnvItem(Scope Scope, string Name, string Raw, string Expand);

        public enum Scope
        {
            Unknown,
            User,
            Machine,
        }
    }
}