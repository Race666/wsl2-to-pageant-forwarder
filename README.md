# WSL2 SSH Authentication agent forwarding ot PuTTY Agent
An agent with forwards SSH Agent Auth requests from an WSL2 instance to the PuTTY authentification agent

## There are two forwarders in this project
- PageantRelayNamedPipe -> Connects to the PuTTY Agat via a named pipe
- PageantRelaySocket ->  Connects to the PuTTY Agat via a socket
## Requierments
- An Folder **C:\Users\<MyProfile>\.ssh** (replace <MyProfile> with your Profilename)
- Use the latest Version of [PuTTY CAC](https://github.com/NoMoreFood/putty-cac/releases)
- For **PageantRelayNamedPipe** (.NET Framework 4 Application), the PuTTY Agent must be started with the **--openssh-config C:\\Users\\%UserName%\\.ssh\\pageant.conf** (Path is hardcoded, replace %UserName% with your User/Profilename)
- For **PageantRelaySocket** ([.NET Core Application](https://dotnet.microsoft.com/en-us/download)), the PuTTY Agent must be started with the **--unix C:\\Users\\%UserName%\\.ssh\\agent.sock** (Path is hardcoded, replace %UserName% with your User/Profilename)
- Copy the [**PageantRelayNamedPipe.exe**](https://github.com/Race666/wsl2-to-pageant-forwarder/releases/tag/PageantRelayNamedPipe) or extract [**PageantRelaySocket.7z**](https://github.com/Race666/wsl2-to-pageant-forwarder/releases/tag/PageantRelaySocket) to **C:\\Users\\%UserName%\\.ssh\\**
- Start the Putty Agent and load keys

- ## Usage Putty Agent NamedPipe
Start your WSL instance (Here also: replace <User> with your User/Profilename). In this example socat is starting in foreground. Test it and if the test succeeds append a & at the of the command line and it move to background.

    wsl -d Debian
    export SSH_AUTH_SOCK="/home/$USER/.ssh/agent.sock"
    socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:"/mnt/c/Users/$USER/.ssh/PageantRelayNamedPipe.exe"

## Usage Putty Agent socket
Start your WSL instance (Here also: replace <User> with your Profilename). Example

    wsl -d Debian
    export SSH_AUTH_SOCK="/home/$USER/.ssh/agent.sock"
    socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:"/mnt/c/Users/$USER/.ssh/PageantRelaySocket.exe"

## Then connect to your host

    export SSH_AUTH_SOCK="/home/$USER/.ssh/agent.sock"
    ssh michael@debdev.myDomain.local

