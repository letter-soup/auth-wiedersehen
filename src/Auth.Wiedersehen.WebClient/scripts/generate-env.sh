#!/bin/sh
set -e
ENV_FILE="${1:-}"
TEMPLATE="${2:-src/env.ts.template}"
OUTPUT="${3:-src/env.ts}"

if [ -n "$ENV_FILE" ]; then
  case "$ENV_FILE" in
    /*|./*) : ;;
    *) ENV_FILE="./$ENV_FILE" ;;
  esac
  set -a
  . "$ENV_FILE"
  set +a
fi

envsubst '${AW_API_URI} ${AW_CLIENT_ID} ${AW_REDIRECT_URI}' < "$TEMPLATE" > "$OUTPUT"
