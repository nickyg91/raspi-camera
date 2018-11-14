using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace Stream.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            StartSocketAndSendVideo();
        }

        public static void StartSocketAndSendVideo()
        {
            var videoSettings = new CameraVideoSettings
            {
                CaptureTimeoutMilliseconds = 0,
                CaptureDisplayPreview = false,
                ImageFlipVertically = true,
                CaptureExposure = CameraExposureMode.Night,
                CaptureWidth = 1280,
                CaptureHeight = 720
            };
           
            try
            {
                if (Pi.Camera.IsBusy)
                {
                    Pi.Camera.CloseVideoStream();
                }

                bool isExited = false;
                byte[] bytes = null;
                Pi.Camera.OpenVideoStream(
                    videoSettings,
                    data => { bytes = data; }, 
                    () => { isExited = true; });

                //var serverSocket = 
                //    new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
                //serverSocket.Listen(4);
                //while (true)
                //{
                //    var handler = serverSocket.Accept();
                //    Console.WriteLine("Received connection");
                //    var receivedBytes = new byte[1024];
                //    handler.Receive(receivedBytes);
                //    if (bytes != null)
                //    {
                //        handler.Send(bytes);
                //    }
                //}
                var listener = new TcpListener(IPAddress.Any, 8080);
                listener.Start();
                Console.WriteLine("Server started...");
                ////Console.ReadLine();

                TcpClient client;
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    Console.WriteLine("Client Connected...");
                    var stream = client.GetStream();
                    while (true)
                    {
                        if (client.Connected)
                        {
                            Console.WriteLine("Writing...");
                            stream.WriteAsync(bytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }

                Console.WriteLine(ex.StackTrace);
                Pi.Camera.CloseVideoStream();
            }
        }
    }
}
