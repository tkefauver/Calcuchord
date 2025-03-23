#!/bin/bash

VERSION="1.0.12"
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

# sign
ENTITLEMENTS="$PROJ_DIR/Entitlements2.plist"
SIGNING_ID="Apple Development: thomas kefauver (MY7R67BXWM)"
#SIGNING_ID="Apple Distribution: thomas kefauver (3382GDS46D)"

if [ "$1" = "adhoc" ] || [ "$2" = "adhoc" ] || [ "$3" = "adhoc" ]; then
  SIGNING_ID="Apple Distribution: thomas kefauver (3382GDS46D)"
fi

#codesign --force --deep --sign "$SIGNING_ID" --entitlements "$ENTITLEMENTS" "$BUNDLE_PATH"
find "$BUNDLE_PATH/Contents/MacOS/"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_ID" "$fname"
    fi
done  
find "$BUNDLE_PATH/Contents/MonoBundle/"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_ID" "$fname"
    fi
done  
codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_ID" "$BUNDLE_PATH"
  
OUTPUT_PATH="$BUNDLE_PATH"

# pack
if [ "$1" = "adhoc" ] || [ "$2" = "adhoc" ] || [ "$3" = "adhoc" ]; then
	PACKING_ID="Developer ID Installer: thomas kefauver (3382GDS46D)"
	PACKING_TYPE="adhoc"
	PACKAGE_DIR="$PROJ_DIR/packages/$PACKING_TYPE/$RUNTIME_ID"
	rm -fr "$PACKAGE_DIR"
	mkdir -p "$PACKAGE_DIR"
	PACKAGE_PATH="$PACKAGE_DIR/$DESIRED_APP_HOST_NAME-$VERSION-$RUNTIME_ID.pkg"
	productbuild --sign "$PACKING_ID" --component "$BUNDLE_PATH" /Applications "$PACKAGE_PATH"
	OUTPUT_PATH="$PACKAGE_PATH"	
	rm -fr "$BUNDLE_PATH"
elif [ "$1" = "dmg" ] || [ "$2" = "dmg" ] || [ "$3" = "dmg" ]; then
	PACKING_TYPE="dmg"
	DMG_DIR="$PROJ_DIR/packages/$PACKING_TYPE/$RUNTIME_ID"
	rm -fr "$DMG_DIR"
	mkdir -p "$DMG_DIR"
	mkdir "tmp"
	cp -R "$BUNDLE_PATH" "tmp"
	ln -s /Applications/ tmp
	DMG_PATH="$DMG_DIR/$DESIRED_APP_HOST_NAME.dmg"
	hdiutil create -volname "$DESIRED_APP_HOST_NAME" -srcfolder tmp -ov -format UDZO "$DMG_PATH"
	rm -fr tmp
	
	OUTPUT_PATH="$DMG_PATH"	
	rm -fr "$BUNDLE_PATH"
fi

if [ "$1" = "up" ] || [ "$2" = "up" ] || [ "$3" = "up" ]; then
	TAG_NAME="v$VERSION"
	gh release upload "$TAG_NAME" "$OUTPUT_PATH"
fi

open -R "$OUTPUT_PATH"

# budnle help https://developer.apple.com/forums/thread/701514



