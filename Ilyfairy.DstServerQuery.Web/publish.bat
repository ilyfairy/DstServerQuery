@echo off
dotnet publish -c release -r linux-x64 --self-contained true /p:PublishSingleFile=true -p:PublishTrimmed=true -o bin\Release\publish\
echo done!
pause>nul