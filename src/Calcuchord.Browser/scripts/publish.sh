#!/bin/bash
FRAMEWORK=net9.0-browser
CONFIG=Release

clear 

cd ..
dotnet publish -f $FRAMEWORK -c $CONFIG

dotnet serve -d:bin/$CONFIG/$FRAMEWORK/publish/wwwroot

