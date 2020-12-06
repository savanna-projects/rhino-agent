[Home](../Home.md 'Home')  

# Deployment - Process Host
10/19/2020 - 20 minutes to read

## In This Article
* [Supported OS](#supported-os)
* [Requirements](#requirements)
* [Configure SSL Certificate](#configure-ssl-certificate)
* [Run as Process](#run-as-process)  

## Supported OS
> Rhino API runs on the latest .NET Core version.  

Please read [here](https://dotnet.microsoft.com/platform/support/policy) for OS support matrix.

## Requirements
1. Latest .NET Core installed, read [here](https://dotnet.microsoft.com/download/dotnet/current) for more information about how to download and install.
2. Development or other SSL certificate installed. This is optional and only required if you are working against HTTPS sites or if you want secured connections.

## Configure SSL Certificate
> These steps are relevant only for MAC OS and Windows.  

1. Install .NET Core SDK.
2. Open command line as administrator.
3. Run the following commands (approve any dialog if appears):
```
dotnet --info
dotnet dev-certs https --trust
```

## Run as Process
1. Download the latest [Rhino Agent](https://github.com/savanna-projects/rhino-agent/releases) ZIP file.
2. Extract the file and place the extracted folder under the location you want to hold Rhino Agent (i.e. C:\Rhino\Agent).
3. Navigate into the folder in which you have extracted Rhino Agent.
4. Run the following command:
```
dotnet Rhino.Agent.dll
```

The following is expected:
```
Now listening on: https://localhost:9001
Now listening on: http://localhost:9000
Application started. Press Ctrl+C to shut down.
```

## Next Steps
[Next Step: Server Settings](./ServerSettings.md 'ServerSettings')

## See Also
See also [HTTPS Development Certificate on Windows & Mac OS](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-3.1&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)  
See also [Configuring HTTPS in ASP.NET Core Across Different Platforms](https://devblogs.microsoft.com/aspnet/configuring-https-in-asp-net-core-across-different-platforms/)