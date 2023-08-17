using System;
using System.IO;
using System.Net.Sockets;
/*
 * Acts as a agent between the SSH_AUTH_SOCK created by socat and the UNIX Putty Agent Socket created by PuTTY Auth Agent
 * The name and path of the PuTTY Auth Agent UNIX Socket is hardcoded %Userprofile%\.ssh\agent.sock
 * Usage:
 * Install socat in your WSL2 instance
 * Locate the PageantRelaySocket.exe on your C: drive and start socat 
 * export SSH_AUTH_SOCK="/home/michael/.ssh/agent.sock"
 * setsid nohup socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:/mnt/c/Users/Michael/.ssh/PageantRelaySocket.exe" &
 * 
 * then connect 
 * export SSH_AUTH_SOCK="/home/michael/.ssh/agent.sock"
 * ssh michael@debdev.myDomain.local
 * 
 * Debug with another ssocat as man in the middle:
 * socat  -d UNIX-LISTEN:"/home/michael/.ssh/debug.socket,fork" EXEC:"/mnt/c/Users/Michael/Documents/CSharp/PageantRelaySocket/PageantRelaySocket/bin/Release/netcoreapp2.1/win-x64/PageantRelaySocket.exe"
 * socat -xd UNIX-LISTEN:"/home/michael/.ssh/agent.sock" UNIX-CONNECT:"/home/michael/.ssh/debug.socket"
 * 
 * Send Hex with netcat
 * echo -e '\x80' | nc host port
 * nc -U /home/michael/.ssh/agent.sock
 */

namespace PageantRelaySocket
{
    class Program
    {
        static void Main(string[] args)
        {
            string sPuTTYAgentSocket = System.IO.Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".ssh", "agent.sock");
            if (!System.IO.File.Exists(sPuTTYAgentSocket))
            {
                Console.Error.WriteLine($"\r\n\r\nCannot open PuTTY Authentification Socket {sPuTTYAgentSocket}");
                Console.Error.WriteLine($"Append --unix  {sPuTTYAgentSocket} parameter to pageant.exe and start the Agent");
                Console.Error.WriteLine($"Example: pageant.exe --unix {sPuTTYAgentSocket}");
                Environment.Exit(1);
            }
            Stream inputStream = Console.OpenStandardInput();
            Stream OutputStream = Console.OpenStandardOutput();
            while (true)
            {
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    socket.Connect(new UnixDomainSocketEndPoint(sPuTTYAgentSocket));
                    int SocketBytesReceived = 0;
                    int STDINReceivedBytes = 0;
                    byte[] ByteOut = new Byte[4096];
                    while ((STDINReceivedBytes = inputStream.Read(ByteOut, 0, 4096)) != 0)
                    {
                        socket.Send(ByteOut, STDINReceivedBytes, SocketFlags.None);
                        SocketBytesReceived = socket.Receive(ByteOut);
                        OutputStream.Write(ByteOut, 0, SocketBytesReceived);
                        OutputStream.Flush();
                        // Wait shortly until STDIN contains new data. Otherwise inputStream.Read hangs
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }
    }
}
