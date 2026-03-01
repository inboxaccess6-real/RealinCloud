#!/usr/bin/env bash
# Build script for Cloudflare Pages
# Installs .NET SDK if not available, then publishes the Blazor WASM app.

set -euo pipefail

DOTNET_VERSION="10.0"
PROJECT="RealinAdmin/src/RealEstate.Admin/RealEstate.Admin.csproj"

# Install .NET SDK if not found
if ! command -v dotnet &> /dev/null; then
  echo "==> .NET SDK not found, installing..."
  curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel "$DOTNET_VERSION"
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi

echo "==> .NET version: $(dotnet --version)"

# Inject API base URL into WASM appsettings.json
if [ -n "${API_BASE_URL:-}" ]; then
  APPSETTINGS="RealinAdmin/src/RealEstate.Admin/wwwroot/appsettings.json"
  echo "==> Setting ApiBaseUrl to $API_BASE_URL"
  cat > "$APPSETTINGS" << EOF
{
    "ApiBaseUrl": "$API_BASE_URL"
}
EOF
fi

# Publish WASM app
echo "==> Publishing Blazor WASM..."
dotnet publish "$PROJECT" -c Release

echo "==> Build complete."
echo "    Output: RealinAdmin/src/RealEstate.Admin/bin/Release/net${DOTNET_VERSION}/publish/wwwroot"
