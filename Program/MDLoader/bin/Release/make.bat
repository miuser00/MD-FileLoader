set rar="C:\Program Files (x86)\WinRAR\Winrar.exe"
for %%a in ("%~f0\..") do  (set filename=%%~nxa)

%rar% a -r -sfx -IICON"md.ico" -z"auto.src" %filename% *.*  