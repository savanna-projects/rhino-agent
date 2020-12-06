[Home](../Home.md 'Home')  

# Deployment - Docker
10/19/2020 - 5 minutes to read

## In This Article
* [Prerequisites](#prerequisites)
* [Deploy and Run](#deploy-and-run)

## Prerequisites
* Docker Engine up and running. For more information about how to download and install Docker Engine check https://docs.docker.com/engine/install/.

## Deploy and Run
1. Run the following command from a command shell (bash, CMD, etc.)  

```
docker run -p 9000:9000 -p 9001:9001 rhinoapi/rhino-agent
```  

The following is expected:
```
Now listening on: https://localhost:9001
Now listening on: http://localhost:9000
Application started. Press Ctrl+C to shut down.
```  

## Next Steps
[Next Step: Server Settings](./ServerSettings.md 'ServerSettings')