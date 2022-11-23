#**[ Application Arguments: User ]*******************************
#*
#* These arguments can be changed or exposed via command line
#*
#****************************************************************
param (
    [string] $ElasticVersion        = '8.4.3',
    [string] $RhinoVersion          = 'v2022.11.6.1-preview',
    [string] $SelenoidQuota         = '5',
    [string] $ServerIp              = '192.168.1.13',
    [string] $ElasticPassword       = 'qawsed1!',
    [string] $KibanaPassword        = 'qawsed1!',
    [int]    $RhinoHttpPort         = 9000,
    [int]    $RhinoHttpsPort        = 9001,
    [bool]   $DeployElastic         = $true
)

#**[ Deployment: Rhino ]*****************************************
#*
#* Rhino API deployment including all services
#*
#****************************************************************
$elasticVersion  = $ElasticVersion
$rhinoVersion    = $RhinoVersion
$selenoidQuota   = $SelenoidQuota
$serverIp        = $ServerIp
$elasticPassword = $ElasticPassword
$kibanaPassword  = $KibanaPassword
$rhinoHttpPort   = $RhinoHttpPort
$rhinoHttpsPort  = $RhinoHttpsPort
$deployElastic   = $DeployElastic

Write-Host "Set-Parameter -ElasticVersion  = $($elasticVersion)"
Write-Host "Set-Parameter -ElasticPassword = $($elasticPassword)"
Write-Host "Set-Parameter -KibanaPassword  = $($kibanaPassword)"
Write-Host "Set-Parameter -RhinoVersion    = $($rhinoVersion)"
Write-Host "Set-Parameter -SelenoidQuota   = $($selenoidQuota)"
Write-Host "Set-Parameter -ServerIp        = $($serverIp)"

#**[ Application Arguments: Volume Root ]**********************
#*
#* Gets and normalize the volume root based the OS type.
#*
#**************************************************************
$volumesRoot = [string]::Empty
$partition   = [string]::Empty
$deployRoot  = [string]::Empty
#
# Normalize for Windows/Linux
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
    $volumesRoot = '/mnt/c'
    $deployRoot  = $volumesRoot
}

if($volumesRoot -notmatch '^\/') {
    $volumesRoot = '/' + $volumesRoot
    Write-Host 'Get-volumesRoot = ' + $volumesRoot
}

#**[ Application Arguments: System & Volumes ]*******************
#*
#* These arguments CANNOT be changed or exposed via command line.
#*
#****************************************************************
#
# Path
$rhinoPath       = [System.IO.Path]::Combine($deployRoot, 'Rhino')
$deploymentPath  = [System.IO.Path]::Combine($rhinoPath, 'DockerVolumes')
$selenoidPath    = [System.IO.Path]::Combine($deploymentPath, 'Selenoid')
$downloadsPath   = [System.IO.Path]::Combine($selenoidPath, 'Downloads')
$uploadsPath     = [System.IO.Path]::Combine($selenoidPath, 'Uploads')
$pulsePath       = [System.IO.Path]::Combine($selenoidPath, 'Pulse')
$elasticPath     = [System.IO.Path]::Combine($deploymentPath, 'Elastic')
#
# Volumes
$downloadsVolume = '$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Download'
$uploadsVolume   = '$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Uploads'
$pulseVolume     = '$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Pulse'
$certs           = '$[volumesRoot]/Rhino/DockerVolumes/Elastic/Certs'
$esdata01        = '$[volumesRoot]/Rhino/DockerVolumes/Elastic/Data01'
$esdata02        = '$[volumesRoot]/Rhino/DockerVolumes/Elastic/Data02'
$esdata03        = '$[volumesRoot]/Rhino/DockerVolumes/Elastic/Data03'
$kibanadata      = '$[volumesRoot]/Rhino/DockerVolumes/Elastic/KibanaData'
#
# Normalize Elastic Volumes for Linux
if($env:OS -notmatch '(?i)windows') {
    $certs      = 'certs'
    $esdata01   = 'esdata01'
    $esdata02   = 'esdata02'
    $esdata03   = 'esdata03'
    $kibanadata = 'kibanadata'
}

#**[ Docker Images ]********************************************
#*             
#* The images for deploy support application for Selenoid.
#*
#* NOTE: if you add another browser image, you also need to add it
#*       to the jsonConfiguration or it will not take affect.
#*
#* You can add more images, please check the images list
#* http://aerokube.com/images/latest/#_browser_image_information
#***************************************************************
$images = @(
    'selenoid/vnc_chrome:96.0',
    'selenoid/vnc_chrome:97.0',
    'selenoid/vnc_chrome:99.0',
    'selenoid/vnc_chrome:100.0',
    'browsers/edge:97.0',
    'browsers/edge:98.0',
    'browsers/edge:99.0',
    'browsers/edge:100.0',
    'selenoid/vnc_firefox:96.0',
    'selenoid/vnc_firefox:97.0',
    'selenoid/vnc_firefox:98.0',
    'selenoid/vnc_firefox:99.0',
    'selenoid/vnc_opera:83.0',
    'selenoid/vnc_opera:84.0',
    'selenoid/vnc_opera:85.0',
    'selenoid/video-recorder:latest-release'
)

#**[ Browsers ]*************************************************
#*             
#* Configures the list of browsers that will be available
#* for Selenoid.
#*
#* You can add more images, please check the images list
#* http://aerokube.com/images/latest/#_browser_image_information
#***************************************************************
$browsersJson = '{
    "chrome": {
        "default": "latest",
        "versions": {
            "96.0": {
                "image": "selenoid/vnc_chrome:96.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "97.0": {
                "image": "selenoid/vnc_chrome:97.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "99.0": {
                "image": "selenoid/vnc_chrome:99.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "latest": {
                "image": "selenoid/vnc_chrome:100.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            }
        }
    },
    "MicrosoftEdge": {
        "default": "latest",
        "versions": {
            "97.0": {
                "image": "browsers/edge:97.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "98.0": {
                "image": "browsers/edge:98.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "99.0": {
                "image": "browsers/edge:99.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "latest": {
                "image": "browsers/edge:100.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            }
        }
    },
    "firefox": {
        "default": "latest",
        "versions": {
            "96.0": {
                "image": "selenoid/vnc_firefox:96.0",
                "port": "4444",
                "path": "/wd/hub",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "97.0": {
                "image": "selenoid/vnc_firefox:97.0",
                "port": "4444",
                "path": "/wd/hub",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "98.0": {
                "image": "selenoid/vnc_firefox:98.0",
                "port": "4444",
                "path": "/wd/hub",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "latest": {
                "image": "selenoid/vnc_firefox:99.0",
                "port": "4444",
                "path": "/wd/hub",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            }
        }
    },
    "opera": {
        "default": "latest",
        "versions": {
            "81.0": {
                "image": "selenoid/vnc_opera:83.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "82.0": {
                "image": "selenoid/vnc_opera:84.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            },
            "latest": {
                "image": "selenoid/vnc_opera:85.0",
                "port": "4444",
                "path": "/",
                "volumes": [
                    "$[downloadsVolume]:/home/selenium/Downloads",
                    "$[uploadsVolume]:/home/selenium/Uploads",
                    "$[pulseVolume]:/home/selenium/pulse"
                ]
            }
        }
    }
}'.Replace('$[downloadsVolume]', $downloadsVolume).Replace('$[uploadsVolume]', $uploadsVolume).Replace('$[pulseVolume]', $pulseVolume).Replace('$[volumesRoot]', $volumesRoot)

#**[ Docker Compose ]*******************************************
#*             
#* Configures the docker-compose.yml file that will be used to
#* deploy the entire project including network configuration.
#*
#* Services:
#* 1. selenoid   : Selenoid backend
#* 2. selenoid-ui: Selenoid front end
#* 3. openproject: A real application that can be used for training
#* 4. rhino      : Rhino API engine
#*
#* The driver binaries value that will be used by Rhino configurations
#* http://selenoid:4444
#***************************************************************
$envFile ='
# Password for the ''elastic'' user (at least 6 characters)
ELASTIC_PASSWORD=$[elasticPassword]

# Password for the ''kibana_system'' user (at least 6 characters)
KIBANA_PASSWORD=$[kibanaPassword]

# Version of Elastic products
STACK_VERSION=$[elasticVersion]

# Set the cluster name
CLUSTER_NAME=docker-cluster

# Set to ''basic'' or ''trial'' to automatically start the 30-day trial
LICENSE=basic
#LICENSE=trial

# Port to expose Elasticsearch HTTP API to the host
ES_PORT=9200
#ES_PORT=127.0.0.1:9200

# Port to expose Kibana to the host
KIBANA_PORT=5601
#KIBANA_PORT=80

# Increase or decrease based on the available host memory (in bytes)
MEM_LIMIT=1073741824

# Project namespace (defaults to the current folder name if not set)
COMPOSE_PROJECT_NAME=elastic'.Replace("$[elasticPassword]", $elasticPassword).Replace("$[kibanaPassword]", $kibanaPassword).Replace("$[elasticVersion]", $elasticVersion)

$dockerComposeRhino   = 'version: ''3''
networks:
  rhino:
    name: rhino

services:
  selenoid:
    networks:
      rhino: null
    image: aerokube/selenoid:latest-release
    volumes:
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid:/etc/selenoid"
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Video:/opt/selenoid/video"
      - "$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Logs:/opt/selenoid/logs"
      - "/var/run/docker.sock:/var/run/docker.sock"
    environment:
      - OVERRIDE_VIDEO_OUTPUT_DIR=$[volumesRoot]/Rhino/DockerVolumes/Selenoid/Video
    command: ["-conf", "/etc/selenoid/browsers.json", "-video-output-dir", "/opt/selenoid/video", "-log-output-dir", "/opt/selenoid/logs", "-container-network", "rhino", "-timeout", "1h", "-limit", "$[selenoidQuota]"]
    ports:
      - "4444:4444"

  selenoid-ui:
    image: "aerokube/selenoid-ui"
    networks:
      rhino: null
    depends_on:
      - selenoid
    ports:
      - "8080:8080"
    command: ["--selenoid-uri", "http://selenoid:4444"]

  openproject:
    image: openproject/community:11
    networks:
      rhino: null
    environment:
      - SECRET_KEY_BASE=secret
    ports:
      - 8085:80

  rhino:
    image: rhinoapi/rhino-agent:$[rhinoVersion]
    networks:
      rhino: null
    ports:
      - $[rhinoHttpsPort]:9001
      - $[rhinoHttpPort]:9000
    volumes:
      - $[volumesRoot]/Rhino/DockerVolumes/Outputs:/app/Outputs
      - $[volumesRoot]/Rhino/DockerVolumes/Data:/app/Data
      - $[volumesRoot]/Rhino/DockerVolumes/Plugins:/app/Plugins'.Replace('$[rhinoVersion]', $rhinoVersion).Replace('$[selenoidQuota]', $selenoidQuota).Replace('$[volumesRoot]', $volumesRoot).Replace('$[rhinoHttpPort]', $rhinoHttpPort).Replace('$[rhinoHttpsPort]', $rhinoHttpsPort)

$dockerComposeElastic = 'version: ''3''
services:
  setup:
    image: docker.elastic.co/elasticsearch/elasticsearch:${STACK_VERSION}
    volumes:
      - $[certs]:/usr/share/elasticsearch/config/certs
    user: "0"
    command: >
      bash -c ''
        if [ x${ELASTIC_PASSWORD} == x ]; then
          echo "Set the ELASTIC_PASSWORD environment variable in the .env file";
          exit 1;
        elif [ x${KIBANA_PASSWORD} == x ]; then
          echo "Set the KIBANA_PASSWORD environment variable in the .env file";
          exit 1;
        fi;
        if [ ! -f certs/ca.zip ]; then
          echo "Creating CA";
          bin/elasticsearch-certutil ca --silent --pem -out config/certs/ca.zip;
          unzip config/certs/ca.zip -d config/certs;
        fi;
        if [ ! -f certs/certs.zip ]; then
          echo "Creating certs";
          echo -ne \
          "instances:\n"\
          "  - name: es01\n"\
          "    dns:\n"\
          "      - es01\n"\
          "      - localhost\n"\
          "    ip:\n"\
          "      - 127.0.0.1\n"\
          "      - $[serverIp]\n"\
          "  - name: es02\n"\
          "    dns:\n"\
          "      - es02\n"\
          "      - localhost\n"\
          "    ip:\n"\
          "      - 127.0.0.1\n"\
          "      - $[serverIp]\n"\
          "  - name: es03\n"\
          "    dns:\n"\
          "      - es03\n"\
          "      - localhost\n"\
          "    ip:\n"\
          "      - 127.0.0.1\n"\
          "      - $[serverIp]\n"\
          > config/certs/instances.yml;
          bin/elasticsearch-certutil cert --silent --pem -out config/certs/certs.zip --in config/certs/instances.yml --ca-cert config/certs/ca/ca.crt --ca-key config/certs/ca/ca.key;
          unzip config/certs/certs.zip -d config/certs;
        fi;
        echo "Setting file permissions"
        chown -R root:root config/certs;
        find . -type d -exec chmod 750 \{\} \;;
        find . -type f -exec chmod 640 \{\} \;;
        echo "Waiting for Elasticsearch availability";
        until curl -s --cacert config/certs/ca/ca.crt https://es01:9200 | grep -q "missing authentication credentials"; do sleep 30; done;
        echo "Setting kibana_system password";
        until curl -s -X POST --cacert config/certs/ca/ca.crt -u elastic:${ELASTIC_PASSWORD} -H "Content-Type: application/json" https://es01:9200/_security/user/kibana_system/_password -d "{\"password\":\"${KIBANA_PASSWORD}\"}" | grep -q "^{}"; do sleep 10; done;
        echo "All done!";
      ''

    healthcheck:
      test: ["CMD-SHELL", "[ -f config/certs/es01/es01.crt ]"]
      interval: 1s
      timeout: 5s
      retries: 120

  es01:
    depends_on:
      setup:
        condition: service_healthy
    image: docker.elastic.co/elasticsearch/elasticsearch:${STACK_VERSION}
    volumes:
      - $[certs]:/usr/share/elasticsearch/config/certs
      - $[esdata01]:/usr/share/elasticsearch/data
    ports:
      - ${ES_PORT}:9200
    environment:
      - node.name=es01
      - cluster.name=${CLUSTER_NAME}
      - cluster.initial_master_nodes=es01,es02,es03
      - discovery.seed_hosts=es02,es03
      - ELASTIC_PASSWORD=${ELASTIC_PASSWORD}
      - bootstrap.memory_lock=true
      - xpack.security.enabled=true
      - xpack.security.http.ssl.enabled=true
      - xpack.security.http.ssl.key=certs/es01/es01.key
      - xpack.security.http.ssl.certificate=certs/es01/es01.crt
      - xpack.security.http.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.http.ssl.verification_mode=certificate
      - xpack.security.transport.ssl.enabled=true
      - xpack.security.transport.ssl.key=certs/es01/es01.key
      - xpack.security.transport.ssl.certificate=certs/es01/es01.crt
      - xpack.security.transport.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.transport.ssl.verification_mode=certificate
      - xpack.license.self_generated.type=${LICENSE}
    mem_limit: ${MEM_LIMIT}
    ulimits:
      memlock:
        soft: -1
        hard: -1
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -s --cacert config/certs/ca/ca.crt https://localhost:9200 | grep -q ''missing authentication credentials''",
        ]
      interval: 10s
      timeout: 10s
      retries: 120

  es02:
    depends_on:
      - es01
    image: docker.elastic.co/elasticsearch/elasticsearch:${STACK_VERSION}
    volumes:
      - $[certs]:/usr/share/elasticsearch/config/certs
      - $[esdata02]:/usr/share/elasticsearch/data
    environment:
      - node.name=es02
      - cluster.name=${CLUSTER_NAME}
      - cluster.initial_master_nodes=es01,es02,es03
      - discovery.seed_hosts=es01,es03
      - bootstrap.memory_lock=true
      - xpack.security.enabled=true
      - xpack.security.http.ssl.enabled=true
      - xpack.security.http.ssl.key=certs/es02/es02.key
      - xpack.security.http.ssl.certificate=certs/es02/es02.crt
      - xpack.security.http.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.http.ssl.verification_mode=certificate
      - xpack.security.transport.ssl.enabled=true
      - xpack.security.transport.ssl.key=certs/es02/es02.key
      - xpack.security.transport.ssl.certificate=certs/es02/es02.crt
      - xpack.security.transport.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.transport.ssl.verification_mode=certificate
      - xpack.license.self_generated.type=${LICENSE}
    mem_limit: ${MEM_LIMIT}
    ulimits:
      memlock:
        soft: -1
        hard: -1
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -s --cacert config/certs/ca/ca.crt https://localhost:9200 | grep -q ''missing authentication credentials''",
        ]
      interval: 10s
      timeout: 10s
      retries: 120

  es03:
    depends_on:
      - es02
    image: docker.elastic.co/elasticsearch/elasticsearch:${STACK_VERSION}
    volumes:
      - $[certs]:/usr/share/elasticsearch/config/certs
      - $[esdata03]:/usr/share/elasticsearch/data
    environment:
      - node.name=es03
      - cluster.name=${CLUSTER_NAME}
      - cluster.initial_master_nodes=es01,es02,es03
      - discovery.seed_hosts=es01,es02
      - bootstrap.memory_lock=true
      - xpack.security.enabled=true
      - xpack.security.http.ssl.enabled=true
      - xpack.security.http.ssl.key=certs/es03/es03.key
      - xpack.security.http.ssl.certificate=certs/es03/es03.crt
      - xpack.security.http.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.http.ssl.verification_mode=certificate
      - xpack.security.transport.ssl.enabled=true
      - xpack.security.transport.ssl.key=certs/es03/es03.key
      - xpack.security.transport.ssl.certificate=certs/es03/es03.crt
      - xpack.security.transport.ssl.certificate_authorities=certs/ca/ca.crt
      - xpack.security.transport.ssl.verification_mode=certificate
      - xpack.license.self_generated.type=${LICENSE}
    mem_limit: ${MEM_LIMIT}
    ulimits:
      memlock:
        soft: -1
        hard: -1
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -s --cacert config/certs/ca/ca.crt https://localhost:9200 | grep -q ''missing authentication credentials''",
        ]
      interval: 10s
      timeout: 10s
      retries: 120

  kibana:
    depends_on:
      es01:
        condition: service_healthy
      es02:
        condition: service_healthy
      es03:
        condition: service_healthy
    image: docker.elastic.co/kibana/kibana:${STACK_VERSION}
    volumes:
      - $[certs]:/usr/share/kibana/config/certs
      - $[kibanadata]:/usr/share/kibana/data
    ports:
      - ${KIBANA_PORT}:5601
    environment:
      - SERVERNAME=kibana
      - ELASTICSEARCH_HOSTS=https://es01:9200
      - ELASTICSEARCH_USERNAME=kibana_system
      - ELASTICSEARCH_PASSWORD=${KIBANA_PASSWORD}
      - ELASTICSEARCH_SSL_CERTIFICATEAUTHORITIES=config/certs/ca/ca.crt
    mem_limit: ${MEM_LIMIT}
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -s -I http://localhost:5601 | grep -q ''HTTP/1.1 302 Found''",
        ]
      interval: 10s
      timeout: 10s
      retries: 120

volumes:
  certs:
    driver: local
  esdata01:
    driver: local
  esdata02:
    driver: local
  esdata03:
    driver: local
  kibanadata:
    driver: local'.Replace('$[certs]', $certs).Replace('$[esdata01]', $esdata01).Replace('$[esdata02]', $esdata02).Replace('$[esdata03]', $esdata03).Replace('$[kibanadata]', $kibanadata).Replace('$[serverIp]', $serverIp).Replace('$[volumesRoot]', $volumesRoot)

# create files & folders
New-Item -ItemType Directory -Path $rhinoPath      -Verbose -Force
New-Item -ItemType Directory -Path $deploymentPath -Verbose -Force
New-Item -ItemType Directory -Path $elasticPath    -Verbose -Force
New-Item -ItemType Directory -Path $selenoidPath   -Verbose -Force
New-Item -ItemType Directory -Path $downloadsPath  -Verbose -Force
New-Item -ItemType Directory -Path $uploadsPath    -Verbose -Force
New-Item -ItemType Directory -Path $pulsePath      -Verbose -Force

Set-Location $rhinoPath -Verbose

Set-Content -Value $browsersJson         -Verbose -Force -Path "$([System.IO.Path]::Combine($selenoidPath, 'browsers.json'))"
Set-Content -Value $dockerComposeRhino   -Verbose -Force -Path "$([System.IO.Path]::Combine($rhinoPath, 'docker-compose-rhino.yml'))"
Set-Content -Value $dockerComposeElastic -Verbose -Force -Path "$([System.IO.Path]::Combine($rhinoPath, 'docker-compose-elastic.yml'))"
Set-Content -Value $envFile              -Verbose -Force -Path "$([System.IO.Path]::Combine($rhinoPath, '.env'))"

# pull images
$images | ForEach-Object { docker pull $_ } -Verbose

# deploy rhino
Invoke-Command { docker compose -f docker-compose-rhino.yml -p rhino up -d }

# deploy elastic
if($env:OS -notmatch '(?i)windows') {
    Invoke-Command { sysctl -w vm.max_map_count=262144 }
}
else {
    Invoke-Command { wsl -d docker-desktop sysctl -w vm.max_map_count=262144 }
}
if($deployElastic) {
    Invoke-Command { docker compose -f docker-compose-elastic.yml -p elastic up -d }
}