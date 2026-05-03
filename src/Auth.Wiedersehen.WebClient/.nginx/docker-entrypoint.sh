#!/bin/sh
set -e

export AW_API_URL

envsubst '${AW_API_URL}' \
  < /etc/nginx/conf.d/default.conf.template \
  > /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
