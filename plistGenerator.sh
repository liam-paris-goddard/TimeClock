#!/bin/bash

# Get the current build configuration
CONFIGURATION=$1
# Define the CFBundleIdentifier for each build configuration
if [ "$CONFIGURATION" == "Debug" ]; then
    BUNDLE_ID="com.GoddardSystems.TimeClock"
elif [ "$CONFIGURATION" == "BuildQA" ]; then
    BUNDLE_ID="com.GoddardSystems.TimeClock"
elif [ "$CONFIGURATION" == "ReleaseQA" ]; then
    BUNDLE_ID="com.GoddardSystems.TimeClock"
elif [ "$CONFIGURATION" == "BuildPROD" ]; then
    BUNDLE_ID="com.GoddardSystems.TimeClock"
elif [ "$CONFIGURATION" == "ReleasePROD" ]; then
    BUNDLE_ID="com.GoddardSystems.TimeClock"
fi
# Replace the CFBundleIdentifier in the Info.plist file
sed -i '' "s/<key>CFBundleIdentifier<\/key><string>.*<\/string>/<key>CFBundleIdentifier<\/key><string>$BUNDLE_ID<\/string>/g" Platforms/iOS/Info.plist