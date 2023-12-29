using System.Net.Sockets;
using System.Net;
using static System.Console;
using System.Runtime.Serialization.Json;
using comm_lib;
using UsersDB;
using UsersDBContext;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;
using Azure;
using System.Collections.ObjectModel;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore.Metadata;

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
                    User user = new User();
                    DataContractJsonSerializer jsonFormatter = null;
                    jsonFormatter = new DataContractJsonSerializer(typeof(User));
                    byte[] bytes = new byte[1024];
                    int bytesRec = 0;
                    while (true)
                    {
                        bytesRec = handler.Receive(bytes);
                        IPAddress ip = ((IPEndPoint)handler.RemoteEndPoint).Address;
                        if (bytesRec == 0)
                        {
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            return;
                        }
                        MemoryStream stream = new MemoryStream(bytes, 0, bytesRec);
                        user = (User)jsonFormatter.ReadObject(stream);
                        stream.Close();
                        if (user.command == "Login")
                        {
                            WriteLine("User " + user.login + " sent authorization request on " + DateTime.Now.ToString());
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.login == user.login && b.password == user.password
                                            select b;
                                if (query.Count() > 0)
                                {
                                    user.command = "Accept";
                                    query = from b in db.users
                                            where b.login != user.login
                                            select b;
                                    //user.contacts = (ObservableCollection<string>)query;
                                    WriteLine("User " + user.login + " is authorized on " + DateTime.Now.ToString());
                                }
                                else if(user.login == "login" || user.password == "password" || query.Count() == 0)
                                {
                                    user.command = "Denied";
                                    WriteLine("User " + user.login + " entered incorrect data - denied " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "Register")
                        {
                            WriteLine("New user " + user.login + " sent registration request on " + DateTime.Now.ToString());
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.login == user.login || b.phone == user.phone
                                            select b;
                                if (query.Count() > 0 || user.login == "login" || user.password == "password" || user.phone == "phone")
                                {
                                    user.command = "Exist";
                                    WriteLine("New user " + user.login + " was not registered - user info already exists" + DateTime.Now.ToString());
                                }
                                else
                                {
                                    var User = new Users() { login = user.login, password = user.password, phone = user.phone, ip = ip.ToString()};
                                    db.users.Add(User);
                                    db.SaveChanges();
                                    user.command = "Accept";
                                    query = from b in db.users
                                            where b.login != user.login
                                            select b;
                                    //user.contacts = (ObservableCollection<string>)query;
                                    WriteLine("New user " + user.login + " is registered on " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);
                        }
                        //else if (user.command == "Send")
                        //{
                        //    WriteLine("New user " + user.login + " sent message on " + DateTime.Now.ToString() + "to " + user.contact);
                        //    using (var db = new UsersContext())
                        //    {
                        //        var query = from b in db.users
                        //                    where b.login == user.contact
                        //                    select b.ip;
                        //        var message = new Messages() { SenderIp = ip.ToString(), ReceiverIp = query.ToString(), Message = user.mess };
                        //        db.messages.Add(message);
                        //        db.SaveChanges();
                        //    }
                        //    //SendToReceiver(Socket socket); доделать!!!
                        //}
                    }
                }
                catch (Exception ex)
                {
                    WriteLine("Сервер-запрос: " + ex.Message);
                }
            });
        }
        private static async void Responce(Socket socket, User user)
        {
            await Task.Run(() =>
            {
                try
                {
                    DataContractJsonSerializer jsonFormatter = null;
                    jsonFormatter = new DataContractJsonSerializer(typeof(User));
                    MemoryStream stream = new MemoryStream();
                    byte[] msg = null;
                    jsonFormatter.WriteObject(stream, user);
                    msg = stream.ToArray();
                    socket.Send(msg);
                    stream.Close();
                }
                catch (Exception ex)
                {
                    WriteLine("Сервер-ответ: " + ex.Message);
                }
            });
        }
    }
}