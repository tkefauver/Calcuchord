#!/bin/bash

VERSION="1.0.0"
CONFIG="Release"
RUNTIME_ID="osx-x64"
FRAMEWORK_ID="net9.0-macos"
APP_HOST_NAME="Calcuchord.Mac"
DESIRED_APP_HOST_NAME="Calcuchord"
APP_EXE_NAME="Calcuchord.Mac"
PROJ_DIR="/Users/tkefauver/Projects/Calcuchord/src/Calcuchord.Mac"

if [ "$1" = "arm" ] || [ "$2" = "arm" ]; then
	RUNTIME_ID="osx-arm64"
fi

BUILD_DIR="$PROJ_DIR/bin/Release/$FRAMEWORK_ID/$RUNTIME_ID"

clear

# build
if [ "$1" = "clean" ] || [ "$2" = "clean" ] || [ "$3" = "clean" ]; then
  sudo rm -fr ../obj
  sudo rm -fr ../bin
fi
dotnet publish ../Calcuchord.Mac.csproj -r "$RUNTIME_ID" -f "$FRAMEWORK_ID" -c "$CONFIG"

# fix bundle
BUNDLE_PATH="$BUILD_DIR/$APP_HOST_NAME.app"
ICON_FILE="$PROJ_DIR/logo.icns"
INFO_PLIST="$PROJ_DIR/Info.plist"

mkdir -p "$BUNDLE_PATH/Contents/Resources"
cp "$ICON_FILE" "$BUNDLE_PATH/Contents/Resources/logo.icns"
cp "$INFO_PLIST" "$BUNDLE_PATH/Contents/Info.plist"

chmod +x "$BUNDLE_PATH/Contents/MacOS/$APP_EXE_NAME"

mv "$BUNDLE_PATH" "$BUILD_DIR/$DESIRED_APP_HOST_NAME.app"
BUNDLE_PATH="$BUILD_DIR/$DESIRED_APP_HOST_NAME.app"

# pack
ENTITLEMENTS="$PROJ_DIR/Entitlements.plist"
SIGNING_ID="Apple Development: thomas kefauver (MY7R67BXWM)"

if [ "$1" = "store" ] || [ "$2" = "store" ] || [ "$1" = "adhoc" ] || [ "$2" = "adhoc" ]; then
	SIGNING_ID="Developer ID Installer: thomas kefauver (3382GDS46D)"
	PACKING_TYPE="adhoc"
	if [ "$1" = "store" ] || [ "$2" = "store" ]; then
	  SIGNING_ID="3rd Party Mac Developer Installer: thomas kefauver (3382GDS46D)"
	  PACKING_TYPE="store"
	fi
	PACKAGE_DIR="$PROJ_DIR/packages/$PACKING_TYPE/$RUNTIME_ID"
	rm -fr "$PACKAGE_DIR"
	mkdir -p "$PACKAGE_DIR"
	PACKAGE_PATH="$PACKAGE_DIR/$DESIRED_APP_HOST_NAME-$VERSION-$RUNTIME_ID.pkg"
	productbuild --sign "$SIGNING_ID" --component "$BUNDLE_PATH" /Applications "$PACKAGE_PATH"
	open -R "$PACKAGE_PATH"
else
  codesign --force --deep --sign "$SIGNING_ID" --entitlements "$ENTITLEMENTS" "$BUNDLE_PATH"
  open -R "$BUNDLE_PATH"
fi



