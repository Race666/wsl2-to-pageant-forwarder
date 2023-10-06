using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;
/* 
 * Acts as a agent between the SSH_AUTH_SOCK created by socat and the Putty Agent namedpipe created by PuTTY Auth Agent
 * The name and path of the PuTTY Auth Agent OpenSSH config file where the NamePipe Path is written to is hardcoded %Userprofile%\.ssh\pageant.conf
 * Usage:
 * Install socat in your WSL2 instance
 * Locate the PageantRelaySocket.exe on your C: drive and start socat 
 * export SSH_AUTH_SOCK="/home/michael/.ssh/agent.sock"
 * setsid nohup socat UNIX-LISTEN:"$SSH_AUTH_SOCK,fork" EXEC:/mnt/c/Users/Michael/.ssh/PageantRelayNamedPipe.exe" &
 * 
 * then connect 
 * export SSH_AUTH_SOCK="/home/michael/.ssh/agent.sock"
 * ssh michael@debdev.myDomain.local
 * 
 * Debug with another ssocat as man in the middle:
 * socat  -d UNIX-LISTEN:"/home/michael/.ssh/debug.socket,fork" EXEC:"/mnt/c/Users/Michael/.ssh/PageantRelayNamedPipe.exe"
 * socat -xd UNIX-LISTEN:"/home/michael/.ssh/agent.sock" UNIX-CONNECT:"/home/michael/.ssh/debug.socket"
 * 
 * Send Hex with netcat
 * echo -e '\x80' | nc host port
 * nc -U /home/michael/.ssh/agent.sock
 *
*/

namespace PageantRelayNamedPipe
{
    class Program
    {
        static void Main(string[] args)
        {
            const string PUTTY_AGENT_NAMED_OPENSSH_CONFFILE = "pageant.conf";
            string sPuttyAgentOpenSSHConfigFileFullpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".ssh", PUTTY_AGENT_NAMED_OPENSSH_CONFFILE);
            if (!System.IO.File.Exists(sPuttyAgentOpenSSHConfigFileFullpath))
            {
                Console.Error.WriteLine($"\r\n\r\nCannot open PuTTY Authentification agents OpenSSH config file {sPuttyAgentOpenSSHConfigFileFullpath}");
                Console.Error.WriteLine($"Append --openssh-config {sPuttyAgentOpenSSHConfigFileFullpath} parameter to pageant.exe and start the Agent");
                Console.Error.WriteLine($"Example: pageant.exe --openssh-config {sPuttyAgentOpenSSHConfigFileFullpath}");
                Environment.Exit(1);
            }
            string sAgentNamedPipe = @"";
            // Read OpenSSH conf file and determine NamedPipe
            using (StreamReader OpenSSHConfFile = new StreamReader(sPuttyAgentOpenSSHConfigFileFullpath))
            {
                string Line = "";
                Regex RegPipe = new Regex(@"(IdentityAgent \\\\\.\\pipe\\(.+))|(IdentityAgent ""//\./pipe/(.+))""");
                while ((Line = OpenSSHConfFile.ReadLine()) != null)
                {
                    Match RegMatch = RegPipe.Match(Line);
                    if (RegMatch.Success)
                    {
                        // putty agent <= 0.78u1
                        if (RegMatch.Groups[2].Success)
                        {
                            sAgentNamedPipe = RegMatch.Groups[2].Value;
                            break;
                        }
                        // putty agent >= 0.79
                        else if (RegMatch.Groups[4].Success)
                        {
                            sAgentNamedPipe = RegMatch.Groups[4].Value;
                            break;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(sAgentNamedPipe))
            {
                Console.Error.WriteLine($"\r\n\r\nCannot find named pipe name in file {sPuttyAgentOpenSSHConfigFileFullpath}");
                Environment.Exit(2);
            }
            string[] aNamedPipes=System.IO.Directory.GetFiles(@"\\.\pipe\");
            if(!aNamedPipes.Contains($@"\\.\pipe\{sAgentNamedPipe}"))
            {
                Console.Error.WriteLine($"Named pipe {sAgentNamedPipe} not found! Possible reasons\r\n  PuTTY Agent is not started\r\n  PuTTY Agent is not configured to provide a named pipe. Parameter --openssh-config");
                Environment.Exit(2);
            }
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", sAgentNamedPipe, PipeDirection.InOut, System.IO.Pipes.PipeOptions.WriteThrough))
            {
                Stream inputStream = Console.OpenStandardInput();
                Stream OutputStream = Console.OpenStandardOutput();
                pipeClient.Connect();
                while (true)
                {
                    int NamedPipeBytesReceived = 0;
                    int STDINReceivedBytes = 0;
                    byte[] ByteOut = new Byte[4096];
                    while ((STDINReceivedBytes = inputStream.Read(ByteOut, 0, 4096)) != 0)
                    {
                        pipeClient.Write(ByteOut, 0, STDINReceivedBytes);
                        NamedPipeBytesReceived = pipeClient.Read(ByteOut, 0, 4096);

                        OutputStream.Write(ByteOut, 0, NamedPipeBytesReceived);
                        OutputStream.Flush();
                        // Wait shortly until STDIN contains new data. Otherwise inputStream.Read hangs
                        System.Threading.Thread.Sleep(100);

                    }
                }
            }
        }
    }
}
