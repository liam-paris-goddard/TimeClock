#!/bin/bash

# Read the current version from the file
version=$(cat DeployVersion/DeployVersion.txt)

# Extract the major, minor, patch, and rc components of the version
major=$(echo $version | cut -d. -f1)
minor=$(echo $version | cut -d. -f2)
patch=$(echo $version | cut -d. -f3 | cut -d- -f1)
rc=$(echo $version | cut -d. -f4)

echo $patch
echo $rc
# Check the build configuration
if [ "$1" == "ReleaseQA" ]; then
# Check if version already contains -rc
  if [[ $version == *"-rc"* ]]; then
    # If it does, just increase the rc version
    base_version=${version%-rc*}
    rc_version=${version##*-rc.}
    new_rc_version=$((rc_version + 1))
    version="$base_version-rc.$new_rc_version"
  else
    # If it doesn't, append -rc1 to it
    version="$version-rc.1"
  fi
elif [ "$1" == "ReleasePROD" ]; then
  # Ask for major, minor, or patch
  echo "Enter 'major', 'minor', or 'patch' to update the version accordingly:"
  read update

  # Update the version accordingly and strip -rc if there is one
  if [ "$update" == "major" ]; then
    major=$((major + 1))
    minor=0
    patch=0
  elif [ "$update" == "minor" ]; then
    minor=$((minor + 1))
    patch=0
  elif [ "$update" == "patch" ]; then
    patch=$((patch + 1))
  fi

  version="$major.$minor.$patch"
fi

echo $version

# Write the new version back to the file
echo $version > DeployVersion/DeployVersion.txt