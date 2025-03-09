#!/bin/bash

find "$1/"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$2" --sign "$3" "$fname"
    fi
done