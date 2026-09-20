#!/usr/bin/env bash

set -euo pipefail

echo "Development tool versions"
echo "-------------------------"

dotnet --version
pre-commit --version
