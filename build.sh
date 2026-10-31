#!/bin/bash

# Directory to monitor for changes
dir="./Source"
MOD=$(basename $PWD)

# Define the path to your solution file
solutionPath="Source/${MOD}.sln"

# Define an array of configurations that build against net472 and share one restore.
# RimWorld 1.7 builds against net10.0 and must restore separately (see build()).
configurations=("v1.2" "v1.3" "v1.4" "v1.5" "v1.6")

# RimWorld install roots. 1.7 is the Waylith / .NET 10 engine.
rimworld_root="/rimworld"
rimworld_1_7="${rimworld_root}/1.7-steam"

function require_game_assemblies() {
    if [ -f "${rimworld_root}/1.7-steam/Assembly-CSharp.dll" ]; then
        # RimWorld 1.7 (Waylith) keeps its game assemblies in the install root.
        return 0
    fi

    echo "Missing ${rimworld_root}/1.7-steam/Assembly-CSharp.dll." >&2
    echo "Install RimWorld 1.7 into ${rimworld_1_7} and try again." >&2
    return 1
}

require_game_assemblies || exit 1

dotnet restore "$solutionPath"
restore_status=$?
if [ $restore_status -ne 0 ]; then
    echo "dotnet restore failed (exit ${restore_status})." >&2
    exit $restore_status
fi

function sync_mod() {
    # Copy over the mod directory.
    rsync -a ${MOD} ${rimworld_root}/1.2/Mods/

    # Copy over and reformat the README.
    cp README.md ${rimworld_root}/1.2/Mods/${MOD}
    unix2dos ${rimworld_root}/1.2/Mods/${MOD}/README.md

    rm -rf ${rimworld_root}/1.3/Mods/${MOD}
    rm -rf ${rimworld_root}/1.4/Mods/${MOD}
    rm -rf ${rimworld_root}/1.5/Mods/${MOD}
    rm -rf ${rimworld_root}/1.6/Mods/${MOD}
    rm -rf ${rimworld_1_7}/Mods/${MOD}

    cp -af ${rimworld_root}/1.2/Mods/${MOD} ${rimworld_root}/1.3/Mods
    cp -af ${rimworld_root}/1.2/Mods/${MOD} ${rimworld_root}/1.4/Mods
    cp -af ${rimworld_root}/1.2/Mods/${MOD} ${rimworld_root}/1.5/Mods
    cp -af ${rimworld_root}/1.2/Mods/${MOD} ${rimworld_root}/1.6/Mods
    cp -af ${rimworld_root}/1.2/Mods/${MOD} ${rimworld_1_7}/Mods
}

function build_1_7() {
    # RimWorld 1.7 targets net10.0, so its restore overwrites obj/project.assets.json
    # and a --no-restore build of the other framework fails with NETSDK1005.
    # Build it with restore, after the net472 jobs have finished.
    echo "Building for configuration: v1.7"
    if ! dotnet build "$solutionPath" --configuration "Release v1.7"; then
        echo "The v1.7 build failed. Skipping sync_mod." >&2
        return 1
    fi
}

function build() {
    require_game_assemblies || return 1

    rm -rf ${rimworld_root}/1.2/Mods/${MOD}

    # Loop through each configuration and build it
    build_pids=()
    build_configs=()
    for config in "${configurations[@]}"; do
        echo "Building for configuration: $config"
        dotnet build --no-restore "$solutionPath" --configuration "Release $config" &
        build_pids+=($!)
        build_configs+=("$config")
    done

    # Do NOT use a bare `wait` here: it reaps the children, and a later
    # `wait "$pid"` then fails with "not a child of this shell", which would
    # abort the build before the v1.7 job ever runs. Waiting on each pid in
    # turn blocks until all jobs are done and preserves their exit statuses.
    # Skip sync_mod if any of the net472 builds failed.
    for i in "${!build_pids[@]}"; do
        if ! wait "${build_pids[$i]}"; then
            echo "Release ${build_configs[$i]} failed. Skipping sync_mod." >&2
            return 1
        fi
    done

    build_1_7 || return 1

    sync_mod

    echo "All builds completed!"
}

build || exit 1
