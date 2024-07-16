#pragma warning disable 0436

#define GENERATE_PROJECT_FILES

using Sharpmake;
using System;
using System.IO;

namespace SharpmakeBuild
{
    public class GlobalConfig
    {
        public static string RootPath = "";
        public static string ExecutePath = "";
        public static string OutputPath = "";
        public static string IntermediatePath = "";

        public static ITarget[] Targets = new ITarget[]{ 
#if GENERATE_PROJECT_FILES
            new Target(
                Platform.anycpu,
                DevEnv.vs2022,
                Optimization.Release,
                OutputType.Dll,
                Blob.NoBlob,
                BuildSystem.MSBuild,
                DotNetFramework.net6_0
            )
#endif
        };
    }

    [Generate]
    public class CommonProject : CSharpProject
    {
        public string IntermediatePath = GlobalConfig.IntermediatePath;

        public CommonProject()
        {
            AddTargets(GlobalConfig.Targets);
            
            IsFileNameToLower = false;
            IsTargetFileNameToLower = false;
            DependenciesCopyLocal = DependenciesCopyLocalTypes.Default;
            CustomProperties.Add("Deterministic", "true");
            CustomProperties.Add("Features", "strict");
            CustomProperties.Add("InvariantGlobalization", "true");
            CustomProperties.Add("AppendTargetFrameworkToOutputPath", "false");
        }
        
        [Configure(), ConfigurePriority(1)]
        public virtual void ConfigureAll(Configuration conf, Target target)
        {
            conf.ProjectFileName = "[project.Name]";
            conf.ProjectPath = "[project.IntermediatePath]/ProjectFiles/[project.Name]";
            conf.TargetPath = GlobalConfig.OutputPath;
            conf.IntermediatePath = "[project.IntermediatePath]/ProjectFiles/[project.Name]";
            
            conf.Options.Add(Options.CSharp.AllowUnsafeBlocks.Enabled);
            conf.Options.Add(Options.CSharp.LanguageVersion.CSharp10);
            conf.Options.Add(Options.CSharp.DebugType.None);
            conf.Options.Add(Options.CSharp.DefaultConfiguration.Release);

            conf.ReferenceOutputAssembly = true;
        }
    }

    [Generate]
    public class SharpmakeCore : CommonProject
    {
        public SharpmakeCore()
        {
            Name = "SharpmakeCore";
            AssemblyName = "Sharpmake";

            SourceRootPath = Path.Combine(GlobalConfig.RootPath, "Sharpmake");
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake/Properties"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake/Analyzer"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake/BuildContext"));
        }
        
        public override void ConfigureAll(Configuration conf, Target target)
        {
            base.ConfigureAll(conf, target);
            
            conf.Output = Configuration.OutputType.DotNetClassLibrary;
            
            conf.ReferencesByNuGetPackage.Add("Microsoft.Win32.Registry", "5.0.0");
            conf.ReferencesByNuGetPackage.Add("Microsoft.VisualStudio.Setup.Configuration.Interop", "3.9.2164");
            conf.ReferencesByNuGetPackage.Add("Microsoft.CodeAnalysis.CSharp", "4.10.0");
            conf.ReferencesByNuGetPackage.Add("Basic.Reference.Assemblies.Net60", "1.7.2");
        }
    }
    
    [Generate]
    public class SharpmakeGenerators : CommonProject
    {
        public SharpmakeGenerators()
        {
            Name = "SharpmakeGenerators";
            AssemblyName = "Sharpmake.Generators";
    
            SourceRootPath = Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators");
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators/Properties"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators/Apple"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators/FastBuild"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators/Generic"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Generators/VisualStudio"));
        }
        
        public override void ConfigureAll(Configuration conf, Target target)
        {
            base.ConfigureAll(conf, target);
            
            conf.Output = Configuration.OutputType.DotNetClassLibrary;
            
            conf.AddPublicDependency<SharpmakeCore>(target);
        }
    }
    
    [Generate]
    public class SharpmakeCommonPlatforms : CommonProject
    {
        public SharpmakeCommonPlatforms()
        {
            Name = "SharpmakeCommonPlatforms";
            AssemblyName = "Sharpmake.CommonPlatforms";
    
            SourceRootPath = Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms");
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms/Properties"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms/Apple"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms/Android"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms/Linux"));
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Platforms/Sharpmake.CommonPlatforms/Windows"));
        }
        
        public override void ConfigureAll(Configuration conf, Target target)
        {
            base.ConfigureAll(conf, target);
            
            conf.Output = Configuration.OutputType.DotNetClassLibrary;
            
            conf.AddPublicDependency<SharpmakeCore>(target);
            conf.AddPublicDependency<SharpmakeGenerators>(target);
        }
    }
    
    [Generate]
    public class SharpmakeApplication : CommonProject
    {
        public SharpmakeApplication()
        {
            Name = "SharpmakeApplication";
            AssemblyName = "Sharpmake.Application";
    
            SourceRootPath = Path.Combine(GlobalConfig.RootPath, "Sharpmake.Application");
            AdditionalSourceRootPaths.Add(Path.Combine(GlobalConfig.RootPath, "Sharpmake.Application/Properties"));
            
            CustomProperties.Add("ServerGarbageCollection", "true");
        }
        
        public override void ConfigureAll(Configuration conf, Target target)
        {
            base.ConfigureAll(conf, target);
            
            conf.Output = Configuration.OutputType.DotNetConsoleApp;
            
            conf.AddPublicDependency<SharpmakeCore>(target);
            conf.AddPublicDependency<SharpmakeGenerators>(target);
            conf.AddPublicDependency<SharpmakeCommonPlatforms>(target);
        }
    }
    
    [Generate]
    public class SharpmakeBuild : CommonProject
    {
        public SharpmakeBuild()
        {
            Name = "SharpmakeBuild";
            AssemblyName = "Sharpmake.Build";
    
            SourceRootPath = Path.Combine(GlobalConfig.RootPath, "Sharpmake.Build");
        }
        
        public override void ConfigureAll(Configuration conf, Target target)
        {
            base.ConfigureAll(conf, target);
            
            conf.Output = Configuration.OutputType.DotNetConsoleApp;
            
            conf.AddPublicDependency<SharpmakeCore>(target);
            conf.AddPublicDependency<SharpmakeGenerators>(target);
            conf.AddPublicDependency<SharpmakeCommonPlatforms>(target);
        }
    }

#if GENERATE_PROJECT_FILES
    [Generate]
    public class Solution : CSharpSolution
    {
        public Solution()
        {
            AddTargets(GlobalConfig.Targets);
            
            Name = "Sharpmake";
            IsFileNameToLower = false;
        }

        [Configure(), ConfigurePriority(1)]
        public virtual void ConfigureAll(Configuration conf, Target target)
        {
            conf.SolutionFileName = "[solution.Name]";
            conf.SolutionPath = GlobalConfig.RootPath;
            
            conf.AddProject<SharpmakeCore>(target);
            conf.AddProject<SharpmakeGenerators>(target);
            conf.AddProject<SharpmakeCommonPlatforms>(target);
            conf.AddProject<SharpmakeApplication>(target);
            conf.AddProject<SharpmakeBuild>(target);
        }
    }
#endif
}

#if GENERATE_PROJECT_FILES
public static class EntryPoint
{
    [Main]
    public static void SharpmakeMain(Arguments arguments)
    {
        ConfigureRootDirectory();
        
        arguments.Generate<SharpmakeBuild.Solution>();
    }

    private static void ConfigureRootDirectory()
    {
        FileInfo fileInfo = Util.GetCurrentSharpmakeFileInfo();
        if (fileInfo.DirectoryName != null)
        {
            SharpmakeBuild.GlobalConfig.ExecutePath = Util.SimplifyPath(fileInfo.DirectoryName);
            SharpmakeBuild.GlobalConfig.RootPath = Path.Combine(SharpmakeBuild.GlobalConfig.ExecutePath, "../");
            SharpmakeBuild.GlobalConfig.OutputPath = Path.Combine(SharpmakeBuild.GlobalConfig.RootPath, "Bin/");
            SharpmakeBuild.GlobalConfig.IntermediatePath = Path.Combine(SharpmakeBuild.GlobalConfig.RootPath, "Intermediate");
        }
    }
}
#endif