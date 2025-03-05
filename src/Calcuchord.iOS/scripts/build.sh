#!/bin/bash
CONFIG="Debug"
FRAMEWORK="net9.0-ios"
RUNTIME="ios-arm64"
PLATFORM="AnyCPU"
DEVICE_ID="00008020-001945DA3669402E"
DEVICE_ARG="-p:_DeviceName="
EXE_NAME="Calcuchord.iOS"
PUB_PROP_ARG="-p:IsPublishMode=false"
BUNDLE_ID="com.thomaskefauver.calcuchord"

if [ "$1" = "sim" ] || [ "$2" = "sim" ] || [ "$3" = "sim" ]; then
	RUNTIME="iossimulator-x64"
	# ipad 18.0 4th gen
	DEVICE_ID="3F003BD7-6850-4229-A905-E774A786AEDA"
	# ipad 18.0 pro 11" M4
	#DEVICE_ID="24B0221D-A0D6-41CF-BC1F-262CFC7365C1"
	# ipad 18.0 mini 6th gen
	#DEVICE_ID="B6636BA5-F87B-4E46-90DB-FA9EC529D53C"
	DEVICE_ARG="-p:_DeviceName=:v2:udid="
fi


if [ "$1" = "rel" ] || [ "$2" = "rel" ] || [ "$3" = "rel" ]; then
  CONFIG="Release"
fi

if [ "$1" = "pub" ] || [ "$2" = "pub" ] || [ "$3" = "pub" ]; then
  CONFIG="Release"
  PUB_PROP_ARG="-p:IsPublishMode=true"
fi

clear

cd ..

rm -fr obj
rm -fr bin
rm *.csproj.user

if [ "$1" = "man" ] || [ "$2" = "man" ] || [ "$3" = "man" ]; then
	dotnet publish -c ${CONFIG} -f ${FRAMEWORK} ${PUB_PROP_ARG} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}
	
	cd "bin/${CONFIG}/${FRAMEWORK}/${RUNTIME}/publish"

	# unzip the IPA file to tmp foldercd D
	mkdir ./tmp
	unzip ${EXE_NAME}.ipa -d ./tmp
	
	# from https://github.com/flutter/flutter/issues/133465#issuecomment-2159512125
  xcrun devicectl device uninstall -q app --device ${DEVICE_ID} ${BUNDLE_ID}
  xcrun devicectl device install app --device ${DEVICE_ID} ./tmp/Payload/${EXE_NAME}.app
  xcrun devicectl device process launch --console --device ${DEVICE_ID} ${BUNDLE_ID}

	# run ios-deploy to install the app into iOS device
	#ios-deploy -r -b ./tmp/Payload/*.app -O "/Users/tkefauver/Desktop/output.log" -E "/Users/tkefauver/Desktop/error.log"
	rm -r ./tmp
else
	dotnet build -t:Run -c ${CONFIG} -f ${FRAMEWORK} ${PUB_PROP_ARG} -p:Platform=${PLATFORM} ${DEVICE_ARG}${DEVICE_ID} -p:RuntimeIdentifier=${RUNTIME}
fi
