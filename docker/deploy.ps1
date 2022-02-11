#┌─[ Application Arguments ]────────────────────────────────────
#│
#└──────────────────────────────────────────────────────────────
$partition   = [string]::Empty
$windowsPath = [string]::Empty
$deployRoot  = [string]::Empty
$volumesRoot = [string]::Empty

if($env:OS -match '(?i)windows') {
    $volumesRoot = '/C'
    $partition   = "$($volumesRoot.Substring(1, 1))"
    
    if($volumesRoot.Length -lt 3) {
        $deployRoot = "$($partition):/"
    }
    else {
        $deployRoot = [System.IO.Path]::Combine("$($partition):/", "$($volumesRoot.Substring(2, $volumesRoot.Length - 1))")
    }
}
else {
    $volumesRoot = '/app'
    $deployRoot  = $volumesRoot
}

if($volumesRoot -notmatch '^\/') {
    $volumesRoot = '/' + $volumesRoot
}

#┌─[ Docker Images ]────────────────────────────────────────────
#│             
#│ The images for deploy support application for Selenoid.
#│
#│ NOTE: if you add another browser image, you also need to add it
#│       to the jsonConfiguration or it will not take affect.
#│
#│ You can add more images, please check the images list
#│ http://aerokube.com/images/latest/#_browser_image_information
#└──────────────────────────────────────────────────────────────
$images = @(
    'selenoid/vnc_chrome:95.0',
    'selenoid/vnc_chrome:96.0',
    'browsers/edge:95.0',
    'browsers/edge:96.0',
    'selenoid/vnc_firefox:94.0',
    'selenoid/vnc_firefox:95.0',
    'selenoid/vnc_opera:81.0',
    'selenoid/vnc_opera:82.0',
    'selenoid/video-recorder:latest-release'
)

#┌─[ Browsers ]─────────────────────────────────────────────────
#│             
#│ Configures the list of browsers that will be available
#│ for Selenoid.
#│
#│ You can add more images, please check the images list
#│ http://aerokube.com/images/latest/#_browser_image_information
#└──────────────────────────────────────────────────────────────
$browsersJson = '{
    "chrome": {
        "default": "latest",
        "versions": {
            "95.0": {
                "image": "selenoid/vnc_chrome:95.0",
                "port": "4444",
                "path": "/"
            },
            "latest": {
                "image": "selenoid/vnc_chrome:96.0",
                "port": "4444",
                "path": "/"
            }
        }
    },
    "MicrosoftEdge": {
        "default": "latest",
        "versions": {
            "95.0": {
                "image": "browsers/edge:95.0",
                "port": "4444",
                "path": "/"
            },
            "latest": {
                "image": "browsers/edge:96.0",
                "port": "4444",
                "path": "/"
            }
        }
    },
    "firefox": {
        "default": "latest",
        "versions": {
            "94.0": {
                "image": "selenoid/vnc_firefox:94.0",
                "port": "4444",
                "path": "/wd/hub"
            },
            "latest": {
                "image": "selenoid/vnc_firefox:95.0",
                "port": "4444",
                "path": "/wd/hub"
            }
        }
    },
    "opera": {
        "default": "latest",
        "versions": {
            "81.0": {
                "image": "selenoid/vnc_opera:81.0",
                "port": "4444",
                "path": "/"
            },
            "latest": {
                "image": "selenoid/vnc_opera:82.0",
                "port": "4444",
                "path": "/"
            }
        }
    }
}'

#┌─[ Docker Compose ]───────────────────────────────────────────
#│             
#│ Configures the docker-compose.yml file that will be used to
#│ deploy the entire project including network configuration.
#│
#│ Services:
#│ 1. selenoid   : Selenoid back-end
#│ 2. selenoid-ui: Selenoid front-end
#│ 3. openproject: A real application that can be used for training
#│ 4. rhino      : Rhino API engine
#│
#│ The driver binaries value that will be used by Rhino configurations
#│ http://selenoid:4444
#└──────────────────────────────────────────────────────────────
$dockerCompose = 'version: ''3''
networks:
  selenoid:
    name: selenoid

services:
  selenoid:
    networks:
      selenoid: null
    image: aerokube/selenoid:latest-release
    volumes:
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid:/etc/selenoid"
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Video:/opt/selenoid/video"
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Logs:/opt/selenoid/logs"
      - "/var/run/docker.sock:/var/run/docker.sock"
    environment:
      - OVERRIDE_VIDEO_OUTPUT_DIR=$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Video
    command: ["-conf", "/etc/selenoid/browsers.json", "-video-output-dir", "/opt/selenoid/video", "-log-output-dir", "/opt/selenoid/logs", "-container-network", "selenoid"]
    ports:
      - "4444:4444"

  selenoid-ui:
    image: "aerokube/selenoid-ui"
    networks:
      selenoid: null
    depends_on:
      - selenoid
    ports:
      - "8080:8080"
    command: ["--selenoid-uri", "http://selenoid:4444"]

  rhino:
    image: rhinoapi/rhino-agent:v2022.1.9.1-production
    networks:
      selenoid: null
    ports:
      - 9001:9001
      - 9000:9000
    volumes:
      - $[volumesRoot]/Rhino/DockerVolumes/Outputs:/app/Outputs
      - $[volumesRoot]/Rhino/DockerVolumes/Data:/app/Data
      - $[volumesRoot]/Rhino/DockerVolumes/Plugins:/app/Plugins'.Replace('$[volumesRoot]', $volumesRoot)

# setup
$rhinoPath      = [System.IO.Path]::Combine($deployRoot, 'Rhino')
$deploymentPath = [System.IO.Path]::Combine($rhinoPath, 'DockerVolumes')
$selenoidPath   = [System.IO.Path]::Combine($deploymentPath, 'Selenoid')

# create files & folder
New-Item -ItemType Directory -Path $selenoidPath -Verbose -Force
Set-Location $rhinoPath

Set-Content -Value $browsersJson  -Verbose -Force -Path "$([System.IO.Path]::Combine($selenoidPath, 'browsers.json'))"
Set-Content -Value $dockerCompose -Verbose -Force -Path "$([System.IO.Path]::Combine($rhinoPath, 'docker-compose.yml'))"

# pull images
$images | ForEach-Object { docker pull $_ }

# deploy compose
docker-compose -p rhino up