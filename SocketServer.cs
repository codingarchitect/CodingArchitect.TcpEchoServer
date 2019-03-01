using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingArchitect.TcpEchoServer
{
  public class SocketServer 
  {
    private const int BufferSize = 4096;
    private readonly int port;
    private readonly IPAddress address;
    private readonly Func<string, string> requestProcessor;
    public SocketServer(
      int port,
      IPAddress address = null,
      Func<string, string> requestProcessor = null)
    {
      this.port = port;
      this.address = address == null ? IPAddress.Parse("0.0.0.0") : address;
      this.requestProcessor = requestProcessor == null ? Echo : requestProcessor;
    }

    public async void Run()
    {
      TcpListener listener = new TcpListener(this.address, this.port);
      listener.Start();
      Console.Write("Service is now running");
      Console.WriteLine(" on port " + this.port);
      while (true) {
        try {
          TcpClient tcpClient = await listener.AcceptTcpClientAsync();
          ClientLoop(tcpClient);
        }
        catch (Exception ex) {
          Console.WriteLine(ex.Message);
        }
      }
    }

    private async void ClientLoop(TcpClient tcpClient)
    {
      var buffer = new byte[BufferSize];        
      string clientEndPoint =
        tcpClient.Client.RemoteEndPoint.ToString();
      Console.WriteLine("Received connection request from "
        + clientEndPoint);
      try 
      {
        using(NetworkStream networkStream = tcpClient.GetStream())
        {
          while (true) 
          {
            var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (byteCount > 0)
            {
              var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
              Console.WriteLine("[Server] Client {0} wrote {1}", clientEndPoint, request);
              var response = requestProcessor(request);
              await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
            else
              break; // Client closed connection
          }          
        }
        tcpClient.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        if (tcpClient.Connected)
          tcpClient.Close();
      }
    }
    public static string Echo(string request)
    {
      return request;
    }
  }
}
