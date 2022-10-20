[![.NET](https://github.com/aimenux/NuGetCli/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/aimenux/NuGetCli/actions/workflows/ci.yml)

# NuGetCli
```
Providing a net global tool to upload/download nuget packages
```

> In this repo, i m building a global tool that allows to upload/download nuget packages.
>
> The tool is based on multiple sub commmands :
> - Use sub command `Upload` to upload nuget packages
> - Use sub command `Download` to download nuget packages

>
> To run the tool, type commands :
> - `NuGetCli -h` to show help
> - `NuGetCli -s` to show settings
> - `NuGetCli Upload -u [url]` to upload nuget packages from directory to nuget feed
> - `NuGetCli Upload -u [url] -k [apikey]` to upload nuget packages from directory to nuget feed
> - `NuGetCli Upload -u [url] -d [directory]` to upload nuget packages from directory to nuget feed
> - `NuGetCli Download -f [file]` to download nuget packages from nuget feed to directory
> - `NuGetCli Download -f [file] -d [directory]` to download nuget packages from nuget feed to directory
> - `NuGetCli Download -n [name] -v [version]` to download nuget package from nuget feed to directory
> - `NuGetCli Download -n [name] -v [version] -d [directory]` to download nuget package from nuget feed to directory
>
>
> To install global tool from a local source path, type commands :
> - `dotnet tool install -g --configfile .\nugets\local.config NuGetCli --version "*-*" --ignore-failed-sources`
>
> To install global tool from [nuget source](https://www.nuget.org/packages/NuGetCli), type these command :
> - For stable version : `dotnet tool install -g NuGetCli --ignore-failed-sources`
> - For prerelease version : `dotnet tool install -g NuGetCli --version "*-*" --ignore-failed-sources`
>
> To uninstall global tool, type these command :
> - `dotnet tool uninstall -g NuGetCli`
>
>

**`Tools`** : vs22, net 6.0, command-line, spectre-console