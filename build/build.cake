#tool "nuget:?package=GitVersion.CommandLine"
#addin nuget:?package=Cake.Coverlet
#tool nuget:?package=Codecov
#addin nuget:?package=Cake.Codecov
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool
#addin nuget:?package=Cake.Sonar

var target = Argument("target", "Default");

var buildPath = Directory("./build-artifacts");
var publishPath = buildPath + Directory("publish");
var appPublishPath = publishPath + Directory("app");
var clientPublishPath = publishPath + Directory("client");
var releasePath = buildPath + Directory("release");
var clientPath = buildPath + Directory("client");
var testPath = buildPath + Directory("test");
var codeGenPath = buildPath + Directory("code-gen");

string version, branch;

var codecovToken = EnvironmentVariable("CODECOV_TOKEN");
var sonarCloudToken = EnvironmentVariable("SONARCLOUD_TOKEN");

Task("__Clean")
  .Does(() => {
      CleanDirectories(new DirectoryPath[]{
        buildPath
      });
      CleanDirectories("../source/**/bin");
      CleanDirectories("../source/**/obj");
      CleanDirectories("../tests/**/bin");
      CleanDirectories("../tests/**/obj");
      CleanDirectories("../source/TrekkingForCharity.Api.Client/Executors/Commands");
      CleanDirectories("../source/TrekkingForCharity.Api.Client/Executors/CommandResults");

      CreateDirectory(publishPath);
      CreateDirectory(releasePath);
      CreateDirectory(testPath);
      CreateDirectory(codeGenPath);
      CreateDirectory("../source/TrekkingForCharity.Api.Client/Executors/Commands");
      CreateDirectory("../source/TrekkingForCharity.Api.Client/Executors/CommandResults");
  });
Task("__Versioning")
  .Does(() => {
    var gitVersion = GitVersion();
    version = gitVersion.NuGetVersion;
    branch = gitVersion.BranchName;
    
    var files = GetFiles("../source/**/*.csproj");
    foreach (var file in files) {
      Information(file);
      XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", "1.0.0");
      XmlPoke(file, "/Project/PropertyGroup/FileVersion", version);
      XmlPoke(file, "/Project/PropertyGroup/Version", version);
    }
    if (AppVeyor.IsRunningOnAppVeyor) {
      GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true, 
        OutputType = GitVersionOutput.BuildServer
      });
    }
  });
Task("__NugetRestore")
  .Does(() => {
      DotNetCoreRestore("../TrekkingForCharity.Api.sln");
  });

Task("__GenerateApiFiles")
	.Does(() => {
		var settings = new DotNetCoreRunSettings {
         Configuration = "Release",        
     };

     DotNetCoreRun("../source/TrekkingForCharity.Api.CodeGeneration/TrekkingForCharity.Api.CodeGeneration.csproj", MakeAbsolute(codeGenPath).ToString(), settings);


     CopyFiles("./build-artifacts/code-gen/commands/*.cs", "../source/TrekkingForCharity.Api.Client/Executors/Commands");
     CopyFiles("./build-artifacts/code-gen/command-results/*.cs", "../source/TrekkingForCharity.Api.Client/Executors/CommandResults");
	});

Task("__Test")
  .Does(() => {
    var testFilePath = MakeAbsolute(File("./build-artifacts/test/xunit-report.xml"));
    
    var testSettings = new DotNetCoreTestSettings {
      Configuration = "Release",
      Logger = string.Format("trx;LogFileName={0}", MakeAbsolute(File("./build-artifacts/test/xunit-report.xml")))
    };

    var coveletSettings = new CoverletSettings {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.opencover,
        CoverletOutputDirectory = Directory("./build-artifacts/test/"),
        CoverletOutputName = "opencover.xml"
    };

    DotNetCoreTest("../tests/TrekkingForCharity.Api.Tests/TrekkingForCharity.Api.Tests.csproj", testSettings, coveletSettings);

    if (AppVeyor.IsRunningOnAppVeyor) {
      AppVeyor.UploadTestResults(testFilePath, AppVeyorTestResultsType.XUnit);
    }
  });

Task("__Publish")
  .Does(() => {
    
    var msbuildSettings = new MSBuildSettings {
      Verbosity = Verbosity.Minimal,
      ToolVersion = MSBuildToolVersion.VS2017,
      Configuration = "Release",
      PlatformTarget = PlatformTarget.MSIL
    };
    msbuildSettings.WithProperty("OutDir", MakeAbsolute(appPublishPath).ToString());
    
    MSBuild("../source/TrekkingForCharity.Api.App/TrekkingForCharity.Api.App.csproj", msbuildSettings);

    msbuildSettings = new MSBuildSettings {
      Verbosity = Verbosity.Minimal,
      ToolVersion = MSBuildToolVersion.VS2017,
      Configuration = "Release",
      PlatformTarget = PlatformTarget.MSIL
    };
    msbuildSettings.WithProperty("OutDir", MakeAbsolute(clientPublishPath).ToString());
    
    MSBuild("../source/TrekkingForCharity.Api.Client/TrekkingForCharity.Api.Client.csproj", msbuildSettings);

  });
Task("__Package")
  .Does(() => {
      Zip(appPublishPath, releasePath + File("TrekkingForCharity.Api.App.zip"));   

      CopyFiles("../source/TrekkingForCharity.Api.Client/bin/Release/*.nupkg", releasePath);

      if (AppVeyor.IsRunningOnAppVeyor) {
        AppVeyor.UploadArtifact(releasePath + File("TrekkingForCharity.Api.App.zip"));        
      }
  });
Task("__ProcessDataForThirdParties")
  .Does(() => {
    if (AppVeyor.IsRunningOnAppVeyor) {
      var settings = new SonarBeginSettings() {
        Url = "https://sonarcloud.io",
        Key = "t4c-api",
        Login = sonarCloudToken,        
        Verbose = true,
        Organization = "trekking-for-charity",
        OpenCoverReportsPath = MakeAbsolute(File("./build-artifacts/test/opencover.xml")).ToString(),
        Branch = branch
      };
      Sonar(ctx => ctx.DotNetCoreMSBuild("../TrekkingForCharity.Api.sln"), settings);

      Codecov("./build-artifacts/test/opencover.xml", codecovToken);
    }
  });


Teardown(context => {
  var files = GetFiles("../source/**/*.csproj");
  foreach (var file in files) {
    Information("Resetting version info for: " + file.ToString());
    XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", "1.0.0");
    XmlPoke(file, "/Project/PropertyGroup/FileVersion", "1.0.0");
    XmlPoke(file, "/Project/PropertyGroup/Version", "1.0.0");
  }
});  

Task("Build")
  .IsDependentOn("__Clean")
  .IsDependentOn("__Versioning")
  .IsDependentOn("__NugetRestore")
  .IsDependentOn("__GenerateApiFiles")
  .IsDependentOn("__Test")
  .IsDependentOn("__Publish")
  .IsDependentOn("__Package")
  .IsDependentOn("__ProcessDataForThirdParties")
  ;

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);


