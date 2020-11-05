printf '%s\n' \
        '[ req ]' \
        'default_bits       = 2048' \
        'default_keyfile    = https.key' \
        'distinguished_name = req_distinguished_name' \
        'req_extensions     = req_ext' \
        'x509_extensions    = v3_ca' \
        \
        '[ req_distinguished_name ]' \
        'commonName = Rhino API HTTPS development certificate' \
        \
        '[ req_ext ]' \
        'subjectAltName = @alt_names' \
        \
        '[ v3_ca ]' \
        'subjectAltName   = @alt_names' \
        'basicConstraints = critical, CA:false' \
        'keyUsage = keyCertSign, cRLSign, digitalSignature, keyEncipherment' \
        \
        '[ alt_names ]' \
        'DNS.1   = localhost' \
        'DNS.2   = 127.0.0.1' > https.config && \
openssl req -x509 -subj '/commonName=Rhino API HTTPS development certificate' -nodes -days 365000 -newkey rsa:2048 -keyout https.key -out https.crt -config https.config && \
openssl pkcs12 -export -out https.pfx -inkey https.key -in https.crt -passin pass:pin -passout pass:pot