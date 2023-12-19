using System.Net.Sockets;
using System.Net;
using static System.Console;
using System.Runtime.Serialization.Json;
using comm_lib;

namespace Whisper_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Accept(); //устанавливаем прослушивание 
            ReadLine(); //Держим сервер в работающем состоянии
        }

        private static async void Accept()
        {
            await Task.Run(() =>
            {
                try
                {
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 49152);
                    Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sListener.Bind(ipEndPoint);
                    sListener.Listen();
                    while (true)
                    {
                        Socket handler = sListener.Accept();
                        Receive(handler);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine("Сервер: " + ex.Message);
                }
            });
        }
        private static async void Receive(Socket handler)
        {
            await Task.Run(() =>
            {
                try
                {
                    Commands command = new Commands();
                    DataContractJsonSerializer jsonFormatter = null;
                    jsonFormatter = new DataContractJsonSerializer(typeof(Commands));
                    byte[] bytes = new byte[1024];
                    int bytesRec = 0;
                    while (true)
                    {
                        bytesRec = handler.Receive(bytes);
                        if (bytesRec == 0)
                        {
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            return;
                        }
                        MemoryStream stream = new MemoryStream(bytes, 0, bytesRec);
                        command = (Commands)jsonFormatter.ReadObject(stream);
                        stream.Close();
                        if (command.command == "Login")
                        {
                            WriteLine("User " + command.login + " sent authorization request on " + DateTime.Now.ToString());
                            //дописать проверку данных с БД и открытие доступа в основной клиент
                            //с отправкой всех данных по пользователю (медиа, переписка, профиль и т.д) - функция Responce();
                            WriteLine("User " + command.login + " is authorized on " + DateTime.Now.ToString());
                        }
                        else if (command.command == "Register")
                        {
                            WriteLine("New user " + command.login + " sent registration request on " + DateTime.Now.ToString());
                            //дописать добавление в БД и открытие доступа в основной клиент
                            WriteLine("New user " + command.login + " is registered on " + DateTime.Now.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLine("Сервер: " + ex.Message);
                }
            });
        }
    }
}