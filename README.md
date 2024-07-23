Update to TimeClock to use .net MAUI

**CHANGES MADE**



**Commands**
Releasing new QA version: update deployversion in ConstantsStatics.cs to the next prod version with "-rc.x" at the end, and then run: dotnet clean && dotnet restore && dotnet publish -c ReleaseQA -f net8.0-ios -r ios-arm64 --self-contained true

Releasing new PROD version: remove the -rc.x from deploy version and then run 
dotnet clean && dotnet restore && dotnet publish -c ReleasePROD -f net8.0-ios -r ios-arm64 --self-contained true

**Things to do**
- clean up errors
- automate versioning
- android support?