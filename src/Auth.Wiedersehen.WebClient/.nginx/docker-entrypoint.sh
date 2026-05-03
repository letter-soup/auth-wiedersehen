#!/bin/sh
set -e

NO_ENV_FILE=''
export AW_API_URI
export AW_CLIENT_ID
export AW_REDIRECT_URI

# The first argument is empty, to let the script use env vars instead of the .env file
/scripts/generate-env.sh \
  "$NO_ENV_FILE" \
  /etc/nginx/conf.d/default.conf.template \
  /etc/nginx/conf.d/default.conf

for f in /usr/share/nginx/html/assets/*.js; do
  /scripts/generate-env.sh "$NO_ENV_FILE" "$f" "$f.tmp" && mv "$f.tmp" "$f"
done

exec nginx -g 'daemon off;'
