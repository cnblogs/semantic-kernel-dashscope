#!/bin/bash
set -e

[ -z "$1" ] && echo "Missing tag and version" && exit 1

commit_tag=$1
IFS=/ read -r tagname version <<< "$commit_tag"

version=${version:1}
project=src/${tagname}
dotnet clean -c Release
dotnet build -p:Version=${version-*} -c Release $project
dotnet pack $project -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg --include-source -p:PackageVersion=$version -p:Version=${version-*} -o ./artifacts
