#!/bin/bash

cd "$1"
rm -fr tmp
mkdir tmp
cp -r Avalonia/* tmp/
rm -fr Avalonia/Avalonia
mv tmp Avalonia/Avalonia

