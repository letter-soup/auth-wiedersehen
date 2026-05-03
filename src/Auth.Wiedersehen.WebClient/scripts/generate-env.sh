#!/bin/sh
set -e
ENV_FILE="${1:-.env}"
# POSIX sh requires a path prefix to source a file from the current directory
case "$ENV_FILE" in
  /*|./*) : ;;
  *) ENV_FILE="./$ENV_FILE" ;;
esac
set -a
. "$ENV_FILE"
set +a
envsubst '${AW_API_URI} ${AW_CLIENT_ID} ${AW_REDIRECT_URI}' \
  < src/env.ts.template \
  > src/env.ts
