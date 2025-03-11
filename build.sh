#!/bin/bash

# Check if inotifywait is installed
if [ -z "$(which inotifywait)" ]; then
    echo "Install the inotify-tools package and try again."
    exit 1
fi

MOD=$(basename $PWD)

rm -r /rimworld/1.2/Mods/${MOD}

function build() {
    cp -af ComaDrug /rimworld/1.2/Mods/
    cp -af LICENSE* README.md /rimworld/1.2/Mods/${MOD}
    cp -af /rimworld/1.2/Mods/${MOD} /rimworld/1.3/Mods
    cp -af /rimworld/1.2/Mods/${MOD} /rimworld/1.4/Mods
    cp -af /rimworld/1.2/Mods/${MOD} /rimworld/1.5/Mods
}

build

# Watch for changes to .cs files in the directory and subdirectories
inotifywait --recursive --monitor --format "%e %w%f" \
    --event close_write,move,create,delete ./ComaDrug \
    --include '\.xml$' |
    while read changed; do
        echo "Detected change in $changed"
        build
    done

