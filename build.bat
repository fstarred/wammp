set output_path=D:\SMS_Software\wammp\dist

msbuild  /t:rebuild /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /p:Platform="x64" /p:OutputPath=%output_path%