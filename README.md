# dotnet-csproj-cleaner
### Workaround for .NET Core compilation issue "Duplicate 'Content' items were included"

If you build .NET Core project under Linux you can face the issue:

> /usr/share/dotnet/sdk/1.0.4/Sdks/Microsoft.NET.Sdk/build/Microsoft.NET.Sdk.DefaultItems.targets(188,5): error : **Duplicate 'Content' items were included. The .NET SDK includes 'Content' items from your project directory by default.** You can either remove these items from your project file, or set the 'EnableDefaultContentItems' property to 'false' if you want to explicitly include them in your project file. For more information, see https://aka.ms/sdkimplicititems. The duplicate items were: ........ [your.csproj]

Possible solutions:
1) Set EnableDefaultCompileItems=false and add every file to solution manually (additional chance to make mistake)
2) Remove all entries under 'wwwroot' folder manually before build

Main issue what these projects build *without any warnings* on Windows and fails during build on server with Linux. It's terrible and require additional developers attention.

Some related discussion about reasons for this strange behaviour can be found [here](https://www.bountysource.com/issues/45821398-duplicate-item-detection-fails-to-identify-duplicates-differing-by-vs)

### Solution

Our quick and dirty solution for this issue: create simple program what will remove duplicated entries under Linux as first step during our continuous integration build pipeline. Because this simple tool may be useful for other peoples we decide to share it under MIT license.

Usage:
```bash
dotnet dotnet-csproj-cleaner.dll path/to/ProjectFile.csproj
```

This simple tool will remove all content entries with paths starts from 'wwwwroot' folder (beacause these files already included by default configuration).
After that build under Linux will be possible without any additional manual changes. We use it in Docker container during our Jenkins build pipeline.

## Docker
Few usage examples:

#### Process single project file
```bash
docker run -v ${PWD}:/src -v /var/lib/dotnet-csproj-cleaner:/cleaner -w /src microsoft/dotnet:1.1-runtime \
  dotnet /cleaner/dotnet-csproj-cleaner.dll YourProjectFile.csproj
```

#### Process all project files in folder recursively
```bash
docker run -v ${PWD}:/src -v /var/lib/dotnet-csproj-cleaner:/cleaner -w /src microsoft/dotnet:1.1-runtime \
  find . -name '*.csproj' -exec dotnet /cleaner/dotnet-csproj-cleaner.dll {} \;
```
