using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Assignment1;
using Newtonsoft.Json;

namespace Assignment4
{
    class Server
        {

        static List<Book> books = new List<Book>()
        {
            new Book("Harry Potter", "J.K. Rowling",600,"HP00000000000"),
            new Book("The Great Gatsby", "F. Scott Fitzgerald",600,"TGG0000000000"),
            new Book("Moby Dick", "J.K. Rowling",600,"MD00000000000"),
        };

        public void Start()
            {
                TcpListener server = null;
                try
                {
                    // Set the TcpListener port.
                    Int32 port = 4646;
                    IPAddress localAddr = IPAddress.Loopback;

                    int clientNumber = 0;

                    // TcpListener server = new TcpListener(port);
                    server = new TcpListener(localAddr, port);

                    // Start listening for client requests.
                    server.Start();

                    // Enter the listening loop.
                    while (true)
                    {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");

                        Task.Run(() => HandleStream(client, ref clientNumber));
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                finally
                {
                    // Stop listening for new clients.
                    server.Stop();
                }
                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
            }

            public void HandleStream(TcpClient client, ref int clientNumber)
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                clientNumber++;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i).Trim();
                    Console.WriteLine("Received: {0} from client {1}", data, clientNumber);

                // Process the data sent by the client.
                string message = "The command is not valid";

                    string[] words = data.Split(' ');
                    //data = data + " " + words.Length;

                if (words[0] == "GetAll")
                {
                    message = JsonConvert.SerializeObject(books);
                }
                if (words[0] == "Get")
                {
                    message = JsonConvert.SerializeObject(books.Find(e => e.Isbn13 == words[1]));
                }
                if (words[0] == "Save")
                {
                    string myjson = data.Split("{")[1].Split("}")[0];
                    myjson = "{" + myjson + "}";
                    books.Add(JsonConvert.DeserializeObject<Book>(myjson));
                    message = "";
                }
                    //Encode message
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);

                    Thread.Sleep(1000);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent: {0}", message);
                }

                // Shutdown and end connection
                client.Close();
                clientNumber--;
            }
        }
}
