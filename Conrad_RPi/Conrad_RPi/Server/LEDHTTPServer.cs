using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Conrad_RPi.Server
{
    class LEDHTTPServer
    {
        private StreamSocketListener listener;
        private const uint BufferSize = 8192;
        public string currentQuery = "";
        public event EventHandler<string> NewQueryAppeared;

        public void Initialise()
        {
            listener = new StreamSocketListener();
            listener.BindServiceNameAsync("80"); // Port
            listener.ConnectionReceived += HandleRequest;
        }

        private static string GetQuery(StringBuilder request)
        {
            //System.Diagnostics.Debug.WriteLine("no the request -----------------------------");
            //System.Diagnostics.Debug.WriteLine(request);
            //System.Diagnostics.Debug.WriteLine("end of request -----------------------------");
            string[] requestLines = request.ToString().Split(' ');
            string url = requestLines.Length > 1 ? requestLines[1] : string.Empty;

            string query = (new Uri("http://localhost" + url)).Query;
            System.Diagnostics.Debug.WriteLine("query: "+ query);
            return query;
        }
        public async void HandleRequest(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StringBuilder request = new StringBuilder();

            try
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await args.Socket.InputStream.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting Data ex:"+ex.Message);
                return;
            }

            currentQuery = GetQuery(request);
            NewQueryAppeared?.Invoke(this, currentQuery);
            byte[] body = Encoding.UTF8.GetBytes(
            $"<html><head><title>Background Message</title></head><body>Hello from the background process!<br/>{currentQuery}</body></html>");

            using (MemoryStream bodyStream = new MemoryStream(body))
            {
                byte[] header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Length: {bodyStream.Length}\r\nConnection: close\r\n\r\n");
                Stream responseStream = args.Socket.OutputStream.AsStreamForWrite();
                await responseStream.WriteAsync(header, 0, header.Length);
                await bodyStream.CopyToAsync(responseStream);
                try
                {
                    await responseStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error sending Answere ex:" + ex.Message);
                    return;
                }

            }
        }
    }
}
