#!/bin/bash

CONFIG="Release"
RUNTIME_ID="osx-x64"
FRAMEWORK_ID="net9.0-macos"
APP_HOST_NAME="Calcuchord.Mac"
PROJ_DIR="/Users/tkefauver/Projects/Calcuchord/src/Calcuchord.Mac"
BUILD_DIR="$PROJ_DIR/bin/Release/$FRAMEWORK_ID/$RUNTIME_ID"

clear

# build
rm -fr ../obj
rm -fr ../bin
#dotnet publish ../Calcuchord.Mac.csproj -r "$RUNTIME_ID" -f "$FRAMEWORK_ID" -c "$CONFIG"

# fix bundle
BUNDLE_PATH="$BUILD_DIR/$APP_HOST_NAME.app"
ICON_FILE="$PROJ_DIR/logo.icns"

#mkdir "$BUNDLE_PATH/Contents/Resources"
#cp "$ICON_FILE" "$BUNDLE_PATH/Contents/Resources/logo.icns"
#chmod +x "$BUNDLE_PATH/Contents/MacOS/Calcuchord.Mac"

# sign
ENTITLEMENTS="$PROJ_DIR/Entitlements.plist"
#SIGNING_ID="Apple Distribution: thomas kefauver (3382GDS46D)"
SIGNING_ID="Apple Development: thomas kefauver (MY7R67BXWM)"
PACKAGE_SIGNING_ID="3rd Party Mac Developer Installer: thomas kefauver (3382GDS46D)"
#PROV_PROFILE="cc mac dist"
PROV_PROFILE="tk mac dev"

dotnet publish ../Calcuchord.Mac.csproj  -c $CONFIG -p:CreatePackage=true -p:EnableCodeSigning=true -p:EnablePackageSigning=true -p:CodesignKey="$SIGNING_ID" -p:CodesignProvision="$PROV_PROFILE" -p:CodesignEntitlements="$ENTITLEMENTS" -p:PackageSigningKey="$PACKAGE_SIGNING_ID"