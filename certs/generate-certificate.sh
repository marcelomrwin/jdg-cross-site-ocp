#!/bin/bash

set -e
set -x

WORK_DIR="$PWD"
CA_CRT="${WORK_DIR}/ca.crt"
CA_KEY="${WORK_DIR}/ca.key"
CA_KEYSTORE="${WORK_DIR}/ca-keystore.p12"

CA_PASS="secretCaPassword"
CA_KEYSTORE_PASS="caSecret"

KEYSTORE_PASS="secret"

C="ES"
S="Valencia"
L="Valencia"
O="infinispan.org"
OU="infinispan-server"
CN="dg"

function fail() {
    echo $@
    exit 1
}

function failIfFileExists() {
    [ -f "${1}" ] && fail "${1} already exists"
}

function generate_ca_keystore() {
    echo "Generating CA. cert=${CA_CRT} key=${CA_KEY}"
    if [[ -f "${CA_CRT}" && -f "${CA_KEY}" ]] ; then
        echo "Certificate and key already exist. Skipping"
    else
        openssl req -new -x509 -keyout ${CA_KEY} -out ${CA_CRT} -passout pass:${CA_PASS} -subj "/C=${C}/ST=${S}/L=${L}/O=${O}/CN=${CN}"
    fi
    if [ -f "${CA_KEYSTORE}" ]; then
        echo "CA keystore already exist. Skipping"
    else
        keytool -keystore ${CA_KEYSTORE} -alias CARoot -import -file ${CA_CRT} -storepass ${CA_KEYSTORE_PASS} -noprompt -trustcacerts -storetype pkcs12
    fi
}

function generate_signed_certificate() {
    local alias=$1
    local crt="${WORK_DIR}/${alias}.crt"
    local signed_crt="${WORK_DIR}/${alias}-signed.crt"
    local keystore="${WORK_DIR}/${alias}-keystore.p12"
    local keystore_pass="${KEYSTORE_PASS}"

    if [ ! -f "${crt}" ] ; then
        echo "Generating certificate ${crt}"
        keytool -genkey -alias ${alias} -keyalg RSA -keystore ${keystore} -keysize 2048 -storetype pkcs12 -storepass ${keystore_pass} -noprompt -dname "CN=${alias}, OU=${OU}, O=${O}, L=${L}, S=${S}, C=${C}"
        keytool -keystore ${keystore} -alias ${alias} -certreq -file "${crt}" -storepass ${keystore_pass} -storetype pkcs12
    fi

    if [ ! -f "${signed_crt}" ] ;  then
        echo "Sign server certificate. ca_cert=${CA_CRT} ca_key=${CA_KEY} server_cert=${crt} signed_cert=${signed_crt}"
        openssl x509 -req -CA ${CA_CRT} -CAkey ${CA_KEY} -in "${crt}" -out "${signed_crt}" -days 365 -CAcreateserial -passin pass:${CA_PASS}
    fi

    echo "Import signed certificate"
    keytool -keystore ${keystore} -alias CARoot -import -file ${CA_CRT} -storepass ${keystore_pass} -noprompt -trustcacerts -storetype pkcs12
    keytool -keystore ${keystore} -alias ${alias} -import -file ${signed_crt} -storepass ${keystore_pass} -storetype pkcs12
}

generate_ca_keystore
generate_signed_certificate dg
