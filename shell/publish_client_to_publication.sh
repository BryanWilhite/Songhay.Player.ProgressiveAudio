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

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

echo "renaming base.href in Blazor index.html..."

base_href="/b-roll/audio-p/"
index_path="../Songhay.Player.ProgressiveAudio.Client/wwwroot/index.html"

pwsh -c "(Get-Content $index_path) -replace '<base href=\"/\">', '<base href=\"$base_href\">' | Set-Content $index_path"

echo "publishing Blazor project to default location..."

dotnet publish \
    -o ../Songhay.Player.ProgressiveAudio.Client/Songhay.Player.ProgressiveAudio.Client.fsproj \
    --configuration:Release \
    -p:CompressionEnabled=false \
    /property:GenerateFullPaths=true \
    /consoleloggerparameters:NoSummary \
    --runtime linux-x64

echo "running rsync from default Blazor publish location to local S3 mirror..."

rsync_from="../Songhay.Player.ProgressiveAudio.Client/bin/Release/net10.0/publish/wwwroot/"
rsync_to="../../Songhay.Publications.KinteSpace/app-staging$base_href"

rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    "$rsync_from" "$rsync_to"

echo "Rolling back any repo changes..."

git reset --hard HEAD && git clean -fd

echo "Script is finished. Make sure to double check the base.href of the Blazor index.html file."
