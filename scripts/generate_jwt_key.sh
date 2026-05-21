#!/usr/bin/env bash
# Generates a 64-byte base64 JWT signing key and prints it to stdout
set -euo pipefail
head -c 48 /dev/urandom | base64 | tr -d '\n'
echo
