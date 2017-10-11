var target = Argument("target", "Default");

var buildPath = Directory("./build-artifacts");
var publishPath = buildPath + Directory("publish");
var appPublishPath = publishPath + Directory("app");
var clientPublishPath = publishPath + Directory("client");
var releasePath = buildPath + Directory("release");
var clientPath = buildPath + Directory("client");
var testPath = buildPath + Directory("test");


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
Task("__NugetRestore")
  .Does(() => {
      DotNetCoreRestore("../TrekkingForCharity.Api.sln");
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

Task("Build")
  .IsDependentOn("__Clean")
  .IsDependentOn("__NugetRestore")
  .IsDependentOn("__Publish")
  ;

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);