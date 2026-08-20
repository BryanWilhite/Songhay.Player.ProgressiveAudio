#!/bin/bash

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Setting location to $SCRIPT_ROOT...";
cd $SCRIPT_ROOT

echo "Running rsync with default assets..."
rsync -r --delete-after \
    --checksum \
    --links \
    --progress \
    --stats \
    --exclude .gitkeep \
    ../../Songhay.Publications.KinteSpace/src/b-roll/audio-p/wwwroot/ \
    ../Songhay.Player.ProgressiveAudio.Client/wwwroot/
