#!/bin/bash

echo "BLAH $1"
cd "$1"
mkdir tmp
cp -r Avalonia/* tmp/
mv tmp Avalonia/Avalonia

