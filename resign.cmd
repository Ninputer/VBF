sn -R .\bin\VBF.Compilers.Common.dll .\bin\VBF.snk
sn -R .\bin\VBF.Compilers.Scanners.dll .\bin\VBF.snk
sn -R .\bin\VBF.Compilers.Parsers.dll .\bin\VBF.snk

REM verify signing

sn -vf .\bin\VBF.Compilers.Common.dll
sn -vf .\bin\VBF.Compilers.Scanners.dll
sn -vf .\bin\VBF.Compilers.Parsers.dll