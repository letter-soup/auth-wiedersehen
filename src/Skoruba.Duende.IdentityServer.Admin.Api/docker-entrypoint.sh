#!/bin/sh
set -eu

envsubst < identitydata.json.template > identitydata.json
envsubst < identityserverdata.json.template > identityserverdata.json

exec ./Skoruba.Duende.IdentityServer.Admin.Api "$@"
