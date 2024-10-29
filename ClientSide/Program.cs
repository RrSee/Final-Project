using System.Net.Sockets;
using System.Net;
using System.Text;

Console.WriteLine("Client");
var client = new TcpClient();
var ep = new IPEndPoint(IPAddress.Parse("192.168.100.62"), 27001);
string name;


try
{
    client.Connect(ep);
    var networkStream = client.GetStream();
    NetworkStream stream = client.GetStream();


    async Task addListUsername()
    {
        #region Send the name to the server
        byte[] data = Encoding.UTF8.GetBytes(name);
        await networkStream.WriteAsync(data, 0, data.Length);
        #endregion
    }

    bool checkResponse()
    {
        try
        {
            byte[] buffer4 = new byte[1024];
            int bytesRead4 = stream.Read(buffer4, 0, buffer4.Length);
            var number4 = Encoding.UTF8.GetString(buffer4, 0, bytesRead4);

            Console.WriteLine($"Sunucudan gelen yanito: {number4}"); // Gelen yanıtı kontrol et

            if (number4 == "9")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
            return false;
        }
    }


    async Task onlineUser()
    {
        if (checkResponse() == true)
        {
            //burda 7ni atirdiq en son
            byte[] data = Encoding.UTF8.GetBytes("7");
            await networkStream.WriteAsync(data, 0, data.Length);
            await gameRes();
        }
        else
        {
            #region Server'dan kullanıcı sayısını al
            byte[] sizeBuffer = new byte[4];
            await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
            int userCount = BitConverter.ToInt32(sizeBuffer, 0);
            #endregion

            #region Get the online users sent from the server 
            int count = 0;
            Console.WriteLine("Online Users");
            while (count <= userCount)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                var receivedName = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedName);
                count++;
                break;
            }
            #endregion
        }
    }

    async Task exitUser()
    {
        if (checkResponse() == true)
        {
            await gameRes();
        }
        else
        {
            byte[] data = Encoding.UTF8.GetBytes(name);
            await networkStream.WriteAsync(data, 0, data.Length);
            Console.WriteLine("Oyunu terk edirsiniz...");
        }
    }

    async Task selectPlayerFunc()
    {

        Console.Write("Player Name: ");
        string selectPlayer = Console.ReadLine();

        if (selectPlayer == name)
        {
            Console.WriteLine("Ozuvuze istek ata bilmezsiniz!");
            await selectPlayerFunc();
        }
        else
        {
            byte[] data = Encoding.UTF8.GetBytes("3");
            await networkStream.WriteAsync(data, 0, data.Length);

            byte[] data1 = Encoding.UTF8.GetBytes(selectPlayer);
            await networkStream.WriteAsync(data1, 0, data1.Length);
        }

        //vaxt qoy bir muddet gozlesin gelmese cixsin
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        var cvb = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Sunucudan gelen yanıt: {cvb}");

        if (cvb == "yes")
        {
            await goGame();//goooooooooooooooooooo
        }
        else
        {
            Console.WriteLine("Oyuncu Isteyi Redd Etdi!");
        }

    }

    async Task players()
    {

        byte[] data = Encoding.UTF8.GetBytes("1");
        await networkStream.WriteAsync(data, 0, data.Length);
        await onlineUser();
        Console.WriteLine("1.Select Player");
        Console.WriteLine("2.Exit");
        Console.WriteLine("3.Refresh Players");
        Console.WriteLine("4.Istekler");
        string secim1 = Console.ReadLine();
        if (secim1 == "1")
        {
            await selectPlayerFunc();
        }
        else if (secim1 == "2")
        {
            byte[] data1 = Encoding.UTF8.GetBytes("2");
            await networkStream.WriteAsync(data1, 0, data1.Length);
            await exitUser();
        }
        else if (secim1 == "3")
        {
            await players();
        }
        else
        {
            //Exception
        }

    }

    async Task startScreen()
    {
        Console.WriteLine("1.Game Start");
        Console.WriteLine("2.Exit");
        Console.Write("Secim: ");
        string secim = Console.ReadLine();
        if (secim == "1")
        {
            await players();
        }
        else if (secim == "2")
        {
            byte[] data = Encoding.UTF8.GetBytes("2");
            await networkStream.WriteAsync(data, 0, data.Length);
            await exitUser();
        }
        else
        {
            //Exception
        }

    }

    async Task goGame()
    {
        Console.WriteLine("Oyun Baslayir...");
        Console.ReadLine(); ///oyun baslayandan sonra burdan davam edeceksen yazmaga
    }

    async Task gameRes()
    {
        byte[] buffer1 = new byte[1024];
        int bytesRead1 = stream.Read(buffer1, 0, buffer1.Length);
        var receivedName = Encoding.UTF8.GetString(buffer1, 0, bytesRead1);
        Thread.Sleep(100);
        Console.WriteLine("Istek Varrr!!");
        Console.WriteLine(receivedName);
        Console.Write("Cavab: ");
        string cavab = Console.ReadLine();


        //byte[] data = Encoding.UTF8.GetBytes("4");
        //await networkStream.WriteAsync(data, 0, data.Length);
        Thread.Sleep(100);
        byte[] data1 = Encoding.UTF8.GetBytes(cavab);
        await networkStream.WriteAsync(data1, 0, data1.Length);

        if (cavab == "yes")
        {
            Console.Clear();
            await goGame();// goooooooooooooooooo
        }
        else
        {
            await startScreen();
        }

    };

    if (client.Connected)
    {
        Console.Write("Name: ");
        name = Console.ReadLine(); // Check
        await addListUsername();

        await startScreen();
        //t.Start();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}