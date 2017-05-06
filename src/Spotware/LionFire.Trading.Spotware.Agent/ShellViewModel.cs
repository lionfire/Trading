using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LionFire.Trading.Spotware.Agent
{
    public enum CAlgoKind
    {
        Robot,
        Indicator
    }
    public enum ReleaseMode
    {
        Debug,
        Release
    }

    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {


        #region BotName

        public string BotName
        {
            get { return botName; }
            set
            {
                if (botName == value) return;
                botName = value;
                NotifyOfPropertyChange(() => BotName);
            }
        }
        private string botName = "LionTrifecta";

        #endregion


        #region Kind

        public CAlgoKind Kind
        {
            get { return cAlgoKind; }
            set
            {
                if (cAlgoKind == value) return;
                cAlgoKind = value;
                NotifyOfPropertyChange(() => Kind);
            }
        }
        private CAlgoKind cAlgoKind;

        #endregion

        #region CAlgoSources

        public string CAlgoSources
        {
            get { return cAlgoSources; }
            set
            {
                if (cAlgoSources == value) return;
                cAlgoSources = value;
                NotifyOfPropertyChange(() => CAlgoSources);
            }
        }
        private string cAlgoSources = @"C:\Users\ja\OneDrive\Documents\cAlgo\Sources\Robots\";

        #endregion

        #region ReleaseMode

        public ReleaseMode ReleaseMode
        {
            get { return releaseMode; }
            set
            {
                if (releaseMode == value) return;
                releaseMode = value;
                NotifyOfPropertyChange(() => ReleaseMode);
            }
        }
        private ReleaseMode releaseMode = ReleaseMode.Release;

        public string ReleaseModeString
        {
            get
            {
                return releaseModeString ?? ReleaseMode.ToString();
            }
        }
        private string releaseModeString = null;

        #endregion

        #region Derived
        public string Dir => cAlgoSources.TrimEnd('\\') + $"\\{botName}\\{botName}";

        public string CSProjPath => System.IO.Path.Combine(Dir, botName) + ".csproj";

        public string ReferencesSourceDir => $"C:\\Src\\Trading.Proprietary\\LionFire.Trading.cTrader\\bin\\{ReleaseMode}";


        #endregion

        public ShellViewModel()
        {
        }

        public void Go()
        {
            if (File.Exists(CSProjPath))
            {
                var bak = CSProjPath + ".bak";
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(CSProjPath, bak);
            }

            List<string> strings = new List<string>();

            var head = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{A40D5F9C-806D-4D6C-9C1B-346313426459}</ProjectGuid>
    <ProjectTypeGuids>{DD87C1B2-3799-4CA2-93B6-5288EE928820};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>cAlgo</RootNamespace>
    <AssemblyName>{botName}</AssemblyName>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <TargetFrameworkProfile>Client</TargetFrameworkProfile>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""System.Xml.Linq"" />
    <Reference Include=""System.Data.DataSetExtensions"" />
    <Reference Include=""System.Data"" />
    <Reference Include=""System.Xml"" />
    <Reference Include=""cAlgo.API, Version=1.0.0.0, Culture=neutral, PublicKeyToken=3499da3018340880, processorArchitecture=MSIL"">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>..\..\..\..\API\cAlgo.API.dll</HintPath>
    </Reference>";

            var footer = @"
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""{botName}.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name=""BeforeBuild"">
  </Target>
  <Target Name=""AfterBuild"">
  </Target>
  -->
</Project>

";

            var reference = @"<Reference Include=""{filename}"">
      <HintPath>{ReferencesSourceRelativeDir}\{filename}.dll</HintPath>
    </Reference>";

            var pathAscent = @"..\..\..\..\..\..\..\..\..";
            var ReferencesSourceRelativeDir = pathAscent + ReferencesSourceDir.Substring(2);

            StringBuilder referencesXml = new StringBuilder();

            strings.Add(head);

            DoReplace(ref head);
            DoReplace(ref footer);


            using (var tw = new StreamWriter(new FileStream(CSProjPath, FileMode.Create)))
            {
                tw.Write(head);

                foreach (var filename in Directory.GetFiles(ReferencesSourceDir, "*.dll").Select(s => Path.GetFileNameWithoutExtension(s)))
                {
                    tw.Write(reference
                        .Replace("{ReferencesSourceRelativeDir}", ReferencesSourceRelativeDir)
                        .Replace("{filename}", filename)
                        );
                }

                tw.Write(footer);
            }
        }

        private void DoReplace(ref string s)
        {
            s = s.Replace("{botName}", botName);
            s = s.Replace("{ReleaseMode}", ReleaseModeString);
        }
    }
}