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

string version;

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

      CreateDirectory(publishPath);
      CreateDirectory(releasePath);
      CreateDirectory(testPath);
  });
Task("__Versioning")
  .Does(() => {
    var gitVersion = GitVersion();
    version = gitVersion.NuGetVersion;
    
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

  });
Task("__Package")
  .Does(() => {
      Zip(appPublishPath, releasePath + File("TrekkingForCharity.Api.App.zip"));      
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
        Organization = "trekking-for-charity"
      };
      Sonar(ctx => ctx.DotNetCoreMSBuild("../TrekkingForCharity.Api.sln"), settings);

      Codecov("coverage.xml", codecovToken);
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
  .IsDependentOn("__Test")
  .IsDependentOn("__Publish")
  .IsDependentOn("__Package")
  .IsDependentOn("__ProcessDataForThirdParties")
  ;

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);


