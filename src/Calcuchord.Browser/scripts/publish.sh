#!/bin/bash
FRAMEWORK=net9.0-browser
CONFIG=Release

clear 

cd ..
rm -fr obj
rm -fr bin
dotnet publish -f $FRAMEWORK -c $CONFIG

dotnet serve -o --port 7169 -d:bin/$CONFIG/$FRAMEWORK/publish/wwwroot

