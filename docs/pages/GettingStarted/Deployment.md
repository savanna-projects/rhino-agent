# Deployment
Rhino API runs .NET Core 3.1.

## Supported OS
Please read [here](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md) for OS support matrix.

## Requierments
1. .NET Core 3.1 installed, read [here](https://dotnet.microsoft.com/download/dotnet/current) for more information about how to download and install.
2. Development or other SSL certificate installed. This is optional and only required if you are working against HTTPS sites or if you want secured connections.

## Configure SSL Certificate (Windows & Mac OS)
1. Install .NET Core SDK.
2. Open command line as administrator.
3. Run the following commands:
```
dotnet --info
dotnet dev-certs https --trust
```

See also [HTTPS Development Certificate on Windows & Mac OS](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-3.1&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)  
See also [Configuring HTTPS in ASP.NET Core Across Different Platforms](https://devblogs.microsoft.com/aspnet/configuring-https-in-asp-net-core-across-different-platforms/)

## Deploy Rhino Server (on Process)
1. Download the latest [Rhino Agent](https://github.com/savanna-projects/rhino-agent/releases) ZIP file.
2. Extract the file and place the extracted folder under the location you want to hold Rhino Widget (i.e. C:\Rhino\Widget).
3. Navigate into the folder in which you have extracted Rhino Agent.
4. Run the following command:
```
dotnet Rhino.Agent.dll
```

The following is expected:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```