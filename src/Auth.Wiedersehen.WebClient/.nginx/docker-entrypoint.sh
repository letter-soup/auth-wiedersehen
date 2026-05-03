#!/bin/sh
set -e

export AW_API_URI
export AW_CLIENT_ID
export AW_REDIRECT_URI

envsubst '${AW_API_URI}' \
  < /etc/nginx/conf.d/default.conf.template \
  > /etc/nginx/conf.d/default.conf

for f in /usr/share/nginx/html/assets/*.js; do
  envsubst '${AW_API_URI} ${AW_CLIENT_ID} ${AW_REDIRECT_URI}' < "$f" > "$f.tmp" && mv "$f.tmp" "$f"
done

exec nginx -g 'daemon off;'
