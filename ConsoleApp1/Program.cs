using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

Console.WriteLine("Server");
var ep = new IPEndPoint(IPAddress.Parse("192.168.100.62"), 27001);

var listiner = new TcpListener(ep);
var onlines = new List<string>();
var clients = new List<TcpClient>();

try
{
    listiner.Start();

    while (true)
    {
        var client = listiner.AcceptTcpClient();
        clients.Add(client);
        NetworkStream stream = client.GetStream();
        var networkStream = client.GetStream();


        async Task addListUsername()
        {
            Console.WriteLine($"{client.Client.RemoteEndPoint}");

            #region get the name sent from the client
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            var receivedName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            #endregion

            onlines.Add(receivedName);
        }

        async Task checkResponse(string response)
        {
            try
            {
                byte[] data2 = Encoding.UTF8.GetBytes(response);
                await networkStream.WriteAsync(data2, 0, data2.Length);
                await networkStream.FlushAsync();
                Console.WriteLine($"Client'a yanıt gönderildi: {response}"); // Yanıtın gönderildiğini logla
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yanıt gönderme hatası: {ex.Message}");
            }
        }


        async Task selectPlayer()
        {

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            var receivedName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var playerIndex = onlines.FindIndex(name => name == receivedName);

            //await checkResponse("9");

            if (playerIndex != -1)
            {
                var targetClient = clients[playerIndex];
                NetworkStream targetStream = targetClient.GetStream();

                byte[] data2 = Encoding.UTF8.GetBytes("9");
                await targetStream.WriteAsync(data2, 0, data2.Length);
                await targetStream.FlushAsync();

                byte[] buffer1 = new byte[1024];
                int bytesRead1 = stream.Read(buffer1, 0, buffer1.Length);
                var num = Encoding.UTF8.GetString(buffer1, 0, bytesRead1);

                if (num == "7")
                {
                    byte[] data = Encoding.UTF8.GetBytes($"Sizinle {receivedName} adli oyuncu oyun oynamaq isteyir! He Yoxsa Yox?");
                    await targetStream.WriteAsync(data, 0, data.Length); //--------------------------------------------------------
                    await targetStream.FlushAsync();
                    Console.WriteLine("Oyuncu tapildi");

                    await cavab();
                }
            }
            else
            {
                Console.WriteLine("Oyuncu tapilmadi!");
            }
        }

        async Task cavab()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            var cvb = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (cvb == "yes")
            {
                byte[] data = Encoding.UTF8.GetBytes(cvb);
                await networkStream.WriteAsync(data, 0, data.Length);
                await networkStream.FlushAsync();
            }
            else
            {
                byte[] data = Encoding.UTF8.GetBytes(cvb);
                await networkStream.WriteAsync(data, 0, data.Length);
                await networkStream.FlushAsync();
            }
        }

        async Task check()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            var secim = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(secim);
            if (secim == "1")
            {
                await onlineUser();
            }
            else if (secim == "2")
            {
                await exitUser();
            }
            else if (secim == "3")
            {
                await selectPlayer();
            }
        }

        async Task onlineUser()
        {
            await checkResponse("8");

            Thread.Sleep(100);

            #region onlines list-inin olcusun clinete gonderme
            byte[] data1 = Encoding.UTF8.GetBytes(onlines.Count().ToString());
            await networkStream.WriteAsync(data1, 0, data1.Length);
            await networkStream.FlushAsync(); // Buffer-daki verileri temizle
            #endregion

            #region online list-indeki name-lerin client-e gonderilmesi
            foreach (var line in onlines)
            {
                byte[] data = Encoding.UTF8.GetBytes(line);
                await networkStream.WriteAsync(data, 0, data.Length);
                await networkStream.FlushAsync();
            }
            #endregion
            Console.WriteLine("Servere elave olundu...");
        }

        async Task exitUser()
        {
            await checkResponse("8");
            #region get the name sent from the client
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            var receivedName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            #endregion

            onlines.Remove(receivedName);
            clients.Remove(client);
            Console.WriteLine("Serverden silindi..");
        }

        _ = Task.Run(() =>
        {
            addListUsername();
            while (true)
            {
                check();
            }
        });
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}