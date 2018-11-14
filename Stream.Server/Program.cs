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
                var listener = new TcpListener(IPAddress.Any, 8080);
                listener.Start();
                Console.WriteLine("Server started...");
                //Console.ReadLine();

                TcpClient client;
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    Console.WriteLine("Client Connected...");
                    var str = client.GetStream();
                    while (true)
                    {
                        Console.WriteLine("Writing...");
                        str.Write(bytes, 0, bytes.Length);
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
