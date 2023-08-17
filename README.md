# WSL2 SSH Authentication agent forwarding ot PuTTY Agent
An agent with forwards SSH Agent Auth requests from an WSL2 instance to the PuTTY authentification agent

## There are two forwarders in this project
- PageantRelayNamedPipe -> Connects to the PuTTY Agat via a named pipe
- PageantRelaySocket ->  Connects to the PuTTY Agat via a socket
## Requierments
- An Folder C:\Users\<MyProfile>\.ssh (replace <MyProfile> with your Profilename)
- Use the latest Version of [PuTTY CAC](https://github.com/NoMoreFood/putty-cac/releases)
- For PageantRelayNamedPipe, the PuTTY Agent must be started with the --openssh-config C:\Users\<MyProfile>\.ssh\pageant.conf (Path is hardcoded, replace <MyProfile> with your Profilename)
- For PageantRelaySocket, the PuTTY Agent must be started with the --unix C:\Users\<MyProfile>\.ssh\agent.sock (Path is hardcoded, replace <MyProfile> with your Profilename)
- Copy the PageantRelayNamedPipe.exe or PageantRelaySocket.exe to C:\Users\<MyProfile>\.ssh\
- Start the Putty Agent and load keys

- ## Usage Putty Agent NamedPipe
Start your WSL instance (Here also: replace <MyProfile> with your Profilename). Example
    wsl -d Debian
    export SSH_AUTH_SOCK="/home/<MyProfile>/.ssh/agent.sock"
    socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:"/mnt/c/Users/Michael/.ssh/PageantRelayNamedPipe.exe"

## Usage Putty Agent socket
Start your WSL instance (Here also: replace <MyProfile> with your Profilename). Example
    wsl -d Debian
    export SSH_AUTH_SOCK="/home/<MyProfile>/.ssh/agent.sock"
    socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:"/mnt/c/Users/Michael/.ssh/PageantRelaySocket.exe"

## Th    

