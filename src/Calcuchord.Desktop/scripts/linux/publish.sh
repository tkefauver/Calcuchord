#!/bin/bash

PROJ_DIR="../.."
VERSION="1.0.10.0"

# Clean-up
rm -rf ./out/
rm -rf ./staging_folder/

# .NET publish
# self-contained is recommended, so final users won't need to install .NET
dotnet publish "$PROJ_DIR/Calcuchord.Desktop.csproj" \
  --verbosity quiet \
  --nologo \
  --configuration Release \
  --self-contained true \
  --runtime linux-x64 \
  --output "./out/linux-x64"

# Staging directory
mkdir staging_folder

# Debian control file
mkdir ./staging_folder/DEBIAN
cat > staging_folder/DEBIAN/control <<- EOM
Package: Calcuchord
Version: ${VERSION}
Section: devel
Priority: optional
Architecture: amd64
Depends: libx11-6, libice6, libsm6, libfontconfig1, fluidsynth
Maintainer: Thomas Kefauver <tkefauver@gmail.com>
Homepage: https://github.com/tkefauver/Calcuchord
Description: Reverse search for musicians
Copyright: 2022-2024 Thomas Kefauver <tkefauver@gmail.com>
EOM

# Starter script
mkdir ./staging_folder/usr
mkdir ./staging_folder/usr/bin
cp config/calcuchord.sh ./staging_folder/usr/bin/Calcuchord
chmod +x ./staging_folder/usr/bin/Calcuchord # set executable permissions to starter script

# Other files
mkdir ./staging_folder/usr/lib
mkdir ./staging_folder/usr/lib/Calcuchord
cp -f -a ./out/linux-x64/. ./staging_folder/usr/lib/Calcuchord/ # copies all files from publish dir
chmod -R a+rX ./staging_folder/usr/lib/Calcuchord/ # set read permissions to all files
chmod +x ./staging_folder/usr/lib/Calcuchord/Calcuchord.Desktop # set executable permissions to main executable

# Desktop shortcut
mkdir ./staging_folder/usr/share
mkdir ./staging_folder/usr/share/applications
cp config/Calcuchord.desktop ./staging_folder/usr/share/applications/Calcuchord.desktop

# Desktop icon
# A 1024px x 1024px PNG, like VS Code uses for its icon
mkdir ./staging_folder/usr/share/pixmaps
cp config/Calcuchord_icon_1024px.png ./staging_folder/usr/share/pixmaps/Calcuchord.png

# Hicolor icons
mkdir ./staging_folder/usr/share/icons
mkdir ./staging_folder/usr/share/icons/hicolor
mkdir ./staging_folder/usr/share/icons/hicolor/scalable
mkdir ./staging_folder/usr/share/icons/hicolor/scalable/apps
cp ../../../Calcuchord/Assets/Svg/logo.svg ./staging_folder/usr/share/icons/hicolor/scalable/apps/Calcuchord.svg

# Make .deb file
PACKAGE_PATH="Calcuchord-${VERSION}-amd64.deb"
dpkg-deb --root-owner-group --build ./staging_folder/ "./$PACKAGE_PATH"

# upload release
TAG_NAME="v$VERSION"
gh release upload "$TAG_NAME" "$PACKAGE_PATH"
