#!/bin/bash

#java -jar svg2png-1.2.1.jar -f ../../../Calcuchord/Assets/Svg/app_icon.svg -o ../../Media.xcassets/AppIcon.appiconset -c ios_icon.json

PNG_PATH="../../../Calcuchord/Assets/Images/logo.png"

mkdir MyIcon.iconset
sips -z 16 16     "$PNG_PATH" --out MyIcon.iconset/icon_16x16.png
sips -z 32 32     "$PNG_PATH" --out MyIcon.iconset/icon_16x16@2x.png
sips -z 32 32     "$PNG_PATH" --out MyIcon.iconset/icon_32x32.png
sips -z 64 64     "$PNG_PATH" --out MyIcon.iconset/icon_32x32@2x.png
sips -z 128 128   "$PNG_PATH" --out MyIcon.iconset/icon_128x128.png
sips -z 256 256   "$PNG_PATH" --out MyIcon.iconset/icon_128x128@2x.png
sips -z 256 256   "$PNG_PATH" --out MyIcon.iconset/icon_256x256.png
sips -z 512 512   "$PNG_PATH" --out MyIcon.iconset/icon_256x256@2x.png
sips -z 512 512   "$PNG_PATH" --out MyIcon.iconset/icon_512x512.png
sips -z 1024 1024   "$PNG_PATH" --out MyIcon.iconset/icon_512x512@2x.png
iconutil -c icns MyIcon.iconset
rm -R MyIcon.iconset
mv MyIcon.icns ../../logo.icns