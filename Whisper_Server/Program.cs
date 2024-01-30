using System.Net.Sockets;
using System.Net;
using static System.Console;
using System.Runtime.Serialization.Json;
using comm_lib;
using UsersDB;
using UsersDBContext;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

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
                    WriteLine("Сервер-соединение: " + ex.Message);
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
                    byte[] bytes = new byte[10000000];
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
                                var tmp = query.FirstOrDefault();
                                if (query.Count() > 0)
                                {
                                    user.command = "AcceptLog";
                                    user.avatar = tmp.avatar;
                                    var query1 = from b in db.users
                                                 where b.isInContactList == true
                                                 select b;
                                    if (query1.Count() > 0)
                                    {
                                        foreach (var el in query1)
                                        {
                                            User u = new User();
                                            u.login = el.login;
                                            u.avatar = el.avatar;
                                            user.contactList.Add(u);
                                        }
                                    }
                                    WriteLine("User " + user.login + " is authorized on " + DateTime.Now.ToString());
                                }
                                else if (user.login == "login" || user.password == "password" || query.Count() == 0)
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
                                            where b.login == user.login || b.phone == user.phone || b.ip == ip.ToString()
                                            select b;
                                if (query.Count() > 0 || user.login == "login" || user.password == "password" || user.phone == "phone")
                                {
                                    user.command = "Exist";
                                    WriteLine("New user " + user.login + " was not registered - user info already exists" + DateTime.Now.ToString());
                                }
                                else
                                {
                                    var User = new Users() { login = user.login, password = user.password, phone = user.phone, ip = ip.ToString(),avatar = user.avatar, isInContactList = false};
                                    db.users.Add(User);
                                    db.SaveChanges();
                                    user.command = "Accept";
                                    WriteLine("New user " + user.login + " is registered on " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "Send")
                        {
                            WriteLine("User " + user.login + " sent message on " + DateTime.Now.ToString() + " to " + user.contact);
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.login == user.contact
                                            select b.ip;
                                var tmp = query.FirstOrDefault();
                                var query2 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b.login;
                                var tmp2 = query2.FirstOrDefault();
                                var message = new Messages() { SenderIp = ip.ToString(), ReceiverIp = tmp?.ToString(), Message = user.mess };
                                db.messages.Add(message);
                                db.SaveChanges();
                                var query1 = from b in db.messages
                                             where (b.SenderIp == ip.ToString() && b.ReceiverIp == tmp) || (b.SenderIp == tmp && b.ReceiverIp == ip.ToString())
                                             select b.Message;
                                user.chat = query1.ToList();
                                user.contact = tmp;
                                user.login = tmp2;
                            }
                            SendToReceiver(user);
                        }
                        else if (user.command == "Search")
                        {
                            WriteLine("User " + user.login + " on " + DateTime.Now.ToString() + " requested to search for a contact in DB by phone number " + user.phone);
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.phone == user.phone
                                            select b;
                                if (query.Count() > 0)
                                {
                                    var tmp = query.FirstOrDefault();
                                    var updateBool = db.users.Find(tmp?.Id);
                                    updateBool.isInContactList = true;
                                    db.SaveChanges();
                                    user.command = "Match";
                                    user.contact = tmp?.login;
                                    user.avatar = tmp.avatar;
                                    WriteLine("Match found: " + tmp?.login);
                                }
                                else
                                {
                                    user.command = "No match";
                                    WriteLine("No match found");
                                }
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "Update")
                        {
                            using (var db = new UsersContext())
                            {
                                var query1 = from b in db.users
                                             where b.login == user.contact
                                             select b.ip;
                                var temp = query1.FirstOrDefault();
                                var query = from b in db.messages
                                            where (b.SenderIp == ip.ToString() && b.ReceiverIp == temp) || (b.SenderIp == temp && b.ReceiverIp == ip.ToString())
                                            select b.Message;
                                user.chat = query.ToList();
                                user.command = "Chat";
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "ChangeProfile")
                        {
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.ip == ip.ToString()
                                            select b;
                                var data = query.FirstOrDefault();
                                user.login = data?.login;
                                user.password = data?.password;
                                user.phone = data?.phone;
                                user.command = "CurrentProfile";
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "Profile")
                        {
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.ip == ip.ToString()
                                            select b.Id;
                                var id = query.FirstOrDefault();
                                var toUpdate = db.users.Find(id);
                                if (toUpdate != null)
                                {
                                    toUpdate.login = user.login;
                                    toUpdate.password = user.password;
                                    toUpdate.phone = user.phone;
                                    toUpdate.avatar = user.avatar;
                                    db.SaveChanges();
                                    user.command = "ProfileSaved";
                                    WriteLine("User " + user.login + " changed his profile on " + DateTime.Now.ToString());
                                }
                                else
                                {
                                    user.command = "ProfileNotSaved";
                                    WriteLine("User " + user.login + " changing profile failed on " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);
                        }
                        else if (user.command == "DeleteProfile")
                        {
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.ip == ip.ToString()
                                            select b.Id;
                                var id = query.FirstOrDefault();
                                var toUpdate = db.users.Find(id);
                                if (toUpdate != null)
                                {
                                    db.users.Remove(toUpdate);
                                    db.SaveChanges();
                                    user.command = "ProfileDeleted";
                                    WriteLine("User " + user.login + " deleted his profile on " + DateTime.Now.ToString());
                                }
                                else
                                {
                                    user.command = "ProfileNotDeleted";
                                    WriteLine("User " + user.login + " deleting profile failed on " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);
                        }
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

        private static async void SendToReceiver(User user)
        {
            await Task.Run(() =>
            {
                try
                {
                    IPAddress ipAddr = IPAddress.Parse(user.contact);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 49153);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    if (IsEndPointAvailable(ipEndPoint, socket))
                    {
                        DataContractJsonSerializer jsonFormatter = null;
                        jsonFormatter = new DataContractJsonSerializer(typeof(User));
                        MemoryStream stream = new MemoryStream();
                        byte[] msg = null;
                        jsonFormatter.WriteObject(stream, user);
                        msg = stream.ToArray();
                        socket.Send(msg);
                        stream.Close();
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    WriteLine("Сервер-ответ смс: " + ex.Message);
                }
            });
        }
        private static bool IsEndPointAvailable(IPEndPoint iPEnd, Socket socket)
        {
            try
            {
                socket.Connect(iPEnd);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}