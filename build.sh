#!/usr/bin/env bash
# Build the game from the command line, without opening the Unity editor UI.
#
# Usage:
#   ./build.sh            # Linux build (default)
#   ./build.sh linux
#   ./build.sh windows    # requires the "Windows Build Support (Mono)" module
#   ./build.sh all
#
# Output: Builds/<Platform>/  (+ a zip next to it)
# Override the editor path with UNITY_PATH=/path/to/Unity ./build.sh
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT/RPG"
BUILDS="$ROOT/Builds"
LOG="$BUILDS/build.log"
TARGET="${1:-linux}"

# Locate the Unity editor matching ProjectSettings/ProjectVersion.txt
VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt")"
UNITY="${UNITY_PATH:-$HOME/Unity/Hub/Editor/$VERSION/Editor/Unity}"

if [[ ! -x "$UNITY" ]]; then
    echo "Unity $VERSION not found at: $UNITY" >&2
    echo "Install it via Unity Hub or set UNITY_PATH." >&2
    exit 1
fi

case "$TARGET" in
    linux)   METHOD="BuildScript.BuildLinux";   PLATFORMS=(Linux) ;;
    windows) METHOD="BuildScript.BuildWindows"; PLATFORMS=(Windows) ;;
    all)     METHOD="BuildScript.BuildAll";     PLATFORMS=(Linux Windows) ;;
    *) echo "Unknown target '$TARGET' (expected: linux | windows | all)" >&2; exit 1 ;;
esac

if [[ -f "$PROJECT/Temp/UnityLockfile" ]]; then
    echo "The project seems to be open in the Unity editor. Close it first." >&2
    exit 1
fi

mkdir -p "$BUILDS"
echo "Unity   : $UNITY"
echo "Project : $PROJECT"
echo "Target  : $TARGET"
echo "Log     : $LOG"
echo

"$UNITY" \
    -batchmode -nographics -quit \
    -projectPath "$PROJECT" \
    -executeMethod "$METHOD" \
    -buildOutput "$BUILDS" \
    -logFile "$LOG" \
    || { echo; echo "Build failed. Last lines of the log:"; tail -n 40 "$LOG"; exit 1; }

echo "Build succeeded."
for platform in "${PLATFORMS[@]}"; do
    dir="$BUILDS/$platform"
    [[ -d "$dir" ]] || continue
    zip="$BUILDS/EldenPixel-$platform.zip"
    rm -f "$zip"
    (cd "$BUILDS" && zip -qr "$(basename "$zip")" "$platform")
    echo "  $platform -> $dir"
    echo "  archive  -> $zip ($(du -h "$zip" | cut -f1))"
done
