@echo off
dotnet publish DstServerQuery.Web -c release -r win-x64 --self-contained true /p:PublishSingleFile=true -o publish\
echo done!
pause>nul