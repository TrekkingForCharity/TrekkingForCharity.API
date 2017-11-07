#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");

var buildPath = Directory("./build-artifacts");
var publishPath = buildPath + Directory("publish");
var appPublishPath = publishPath + Directory("app");
var clientPublishPath = publishPath + Directory("client");
var releasePath = buildPath + Directory("release");
var clientPath = buildPath + Directory("client");
var testPath = buildPath + Directory("test");

string version;

Task("__Clean")
  .Does(() => {
      CleanDirectories(new DirectoryPath[]{
        buildPath
      });
      CleanDirectories("../**/bin");
      CleanDirectories("../**/obj");

      CreateDirectory(publishPath);
      CreateDirectory(releasePath);
      CreateDirectory(clientPath);
      CreateDirectory(testPath);
  });
Task("__Versioning")
  .Does(() => {
    var gitVersion = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true
    });
    version = gitVersion.NuGetVersion;

    var files = GetFiles("../source/**/*.csproj");
    foreach (var file in files) {
      Information(file);
      XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", "1.0.0");
      XmlPoke(file, "/Project/PropertyGroup/FileVersion", version);
      XmlPoke(file, "/Project/PropertyGroup/Version", version);
    }
  });
Task("__NugetRestore")
  .Does(() => {
      DotNetCoreRestore("../TrekkingForCharity.Api.sln");
  });
Task("__Test")
  .Does(() => {
var projectFiles = GetFiles("../tests/**/*.csproj");
    foreach(var file in projectFiles)
    {
        DotNetCoreTest(file.FullPath);
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

    var settings = new DotNetCorePublishSettings {
         Configuration = "Release",
         OutputDirectory = clientPublishPath
     };
    
     DotNetCorePublish("../source/TrekkingForCharity.Api.Client/TrekkingForCharity.Api.Client.csproj", settings);    
  });
Task("__Package")
  .Does(() => {
      Zip("./publish/app", releasePath + File("TrekkingForCharity.Api.App.zip"));
      MoveFileToDirectory("../source/TrekkingForCharity.Api.Client/bin/Release/TrekkingForCharity.Api.Client." + version +".nupkg", clientPath);
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
  ;

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);


