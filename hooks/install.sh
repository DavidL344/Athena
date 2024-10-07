#!/bin/bash

repo_root=$(git rev-parse --show-toplevel)

ln -s "$repo_root/hooks/post-commit" "$repo_root/.git/hooks/post-commit"
