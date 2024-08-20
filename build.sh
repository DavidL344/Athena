#!/bin/bash

# Usage: ./build.sh <version>
# Example: ./build.sh 0.2.0

current_branch=$(git branch | grep "\*" | awk '{print $2}')
main_branch=$(git remote show origin | sed -n '/HEAD branch/s/.*: //p')
repo_root=$(git rev-parse --show-toplevel)

default_version=0.0.0
version="${1:-$default_version}"

main_dir="$(pwd)"
revert_branch=0

if [ "$main_dir" != "$repo_root" ]; then cd "$repo_root" || exit 1; fi
if [ "$current_branch" != "$main_branch" ]; then git checkout "$main_branch" && revert_branch=1; fi
if [ "$current_branch" != "$main_branch" ]; then exit 1; fi

mkdir -p "./bin/Release/"
mkdir -p "./bin/Publish/"
mkdir -p "./bin/Packaged/"

find . -name '*.csproj' ! -name '*.Tests*' ! -name "*.Wpf*" -exec dotnet publish {} -c Release -r linux-x64 -o "./bin/Release/linux-x64" \;
find . -name '*.csproj' ! -name '*.Tests*' ! -name "*.Gtk*" -exec dotnet publish {} -c Release -r win-x64 -o "./bin/Release/win-x64" \;

mkdir -p "./bin/Publish/v$version/"
cp -r "./bin/Release/linux-x64" "./bin/Publish/v$version/athena-$version-linux-x64/"
cp -r "./bin/Release/win-x64" "./bin/Publish/v$version/athena-$version-win-x64/"

mkdir -p "./bin/Packaged/v$version/"
cd "./bin/Publish/v$version/" || exit 1

zip -r "../../Packaged/v$version/athena-$version-linux-x64.zip" "./athena-$version-linux-x64/"
zip -r "../../Packaged/v$version/athena-$version-win-x64.zip" "./athena-$version-win-x64/"
cd "../../../"

if [ $revert_branch -eq 1 ]; then git checkout -; fi
if [ "$repo_root" != "$main_dir" ]; then cd "$main_dir" || exit 1; fi
