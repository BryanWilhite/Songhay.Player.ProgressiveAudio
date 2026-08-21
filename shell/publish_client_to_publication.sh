#!/bin/bash

set -euo pipefail

# Ensure we are running inside a Git repository
if ! git rev-parse --is-inside-work-tree &>/dev/null; then
    echo "Error: this script must run in a Git repo. Exiting..." >&2
    exit 1
fi

# Check for any uncommitted changes (staged or unstaged)
if [ -n "$(git status --porcelain)" ]; then
    echo "Error: uncommitted repo changes found. Please commit or stash them first. Exiting..." >&2
    exit 1
fi

echo "Git repository is clean. Proceeding..."

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

client_project_name="Songhay.Player.ProgressiveAudio.Client"
publication_project_name="Songhay.Publications.KinteSpace"

base_href="/b-roll/audio-p/"
index_path="../$client_project_name/wwwroot/index.html"

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

echo "renaming base.href in Blazor index.html..."

pwsh -c "(Get-Content $index_path) -replace '<base href=\"/\">', '<base href=\"$base_href\">' | Set-Content $index_path"

echo "deleting existing file at publish target..."
rm -r "../$client_project_name/bin/Release/net10.0/publish"

echo "publishing Blazor project to default location..."

dotnet publish \
    "../$client_project_name/$client_project_name.fsproj" \
    --configuration:Release \
    -p:CompressionEnabled=false \
    /property:GenerateFullPaths=true \
    /consoleloggerparameters:NoSummary \
    --runtime linux-x64

echo "running rsync from default Blazor publish location to local S3 mirror..."

rsync_from="../$client_project_name/bin/Release/net10.0/publish/wwwroot/"
rsync_to="../../$publication_project_name/app-staging$base_href"

rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    "$rsync_from" "$rsync_to"

echo "Rolling back any repo changes..."

git reset --hard HEAD && git clean -fd

echo "Script is finished. Make sure to double check the base.href of the Blazor index.html file."
