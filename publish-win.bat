@echo off
dotnet publish Ilyfairy.DstServerQuery.Web -c release -r win-x64 --self-contained true /p:PublishSingleFile=true -p:PublishTrimmed=true -o publish\
echo done!
pause>nul