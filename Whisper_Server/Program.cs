using System.Net.Sockets;
using System.Net;
using static System.Console;
using System.Runtime.Serialization.Json;
using Whisper_Server.Model;
using Whisper_Server.DbContexts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Whisper_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Accept();
            ReadLine();
        }
        #region Receiving
        private static async void Accept()
        {
            await Task.Run(() =>
            {
                try
                {
                    byte[] buf = new byte[1000000];
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 49152);
                    Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, buf);
                    sListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, buf);
                    sListener.Bind(ipEndPoint);
                    sListener.Listen();
                    while (true)
                    {
                        Socket handler = sListener.Accept();
                        handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, buf);
                        handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, buf);
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
                        try
                        {
                            MemoryStream stream = new MemoryStream(bytes, 0, bytesRec);
                            user = (User)jsonFormatter.ReadObject(stream);
                            stream.Close();
                        }
                        catch(Exception ex)
                        {
                            WriteLine("Сервер-получение: " + ex.Message);
                        }
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
                                    user.phone = tmp.phone;
                                    user.profile = GetContactList(ip);
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
                                    var User = new Users() { login = user.login, password = user.password, phone = user.phone, ip = ip.ToString(), avatar = user.avatar };
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
                                            select b;
                                var tmp = query.FirstOrDefault();
                                var receiverLogin = tmp.login;
                                var receiverIp = tmp.ip;
                                var query2 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b.login;
                                var senderLogin = query2.FirstOrDefault();
                                if (user.media != null)
                                {
                                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                    string folderPath = Path.Combine(baseDirectory, "Media");
                                    if (!Directory.Exists(folderPath))
                                    {
                                        Directory.CreateDirectory(folderPath);
                                    }
                                    string filePath = Path.Combine(folderPath, user.path);
                                    using(FileStream fs = new FileStream(filePath, FileMode.Create))
                                    {
                                        fs.Write(user.media, 0, user.media.Length);
                                    }
                                    var message = new Messages() { SenderLogin = senderLogin, ReceiverLogin = receiverLogin, Media = filePath, Date = user.data };
                                    db.messages.Add(message);
                                }
                                else
                                {
                                    var message = new Messages() { SenderLogin = senderLogin, ReceiverLogin = receiverLogin, Message = user.mess, Date = user.data };
                                    db.messages.Add(message);
                                }
                                
                                db.SaveChanges();
                                User u = new User();
                                u.chat = new List<Chat>();
                                var query1 = from b in db.messages
                                             where (b.SenderLogin == senderLogin && b.ReceiverLogin == receiverLogin) || (b.SenderLogin == receiverLogin && b.ReceiverLogin == senderLogin)
                                             select b;
                                var tmpObjects = query1.ToList();
                                foreach (var obj in tmpObjects)
                                {
                                    Chat c = new Chat();
                                    if (obj.Message != null)
                                    {
                                        c.message = obj.Message;
                                    }
                                    if (obj.Media != null)
                                    {
                                        c.media = GetImageBytes(obj.Media);
                                    }
                                    c.date = obj.Date;
                                    c.chatContact = obj.SenderLogin;
                                    u.chat.Add(c);
                                }
                                u.contact = receiverIp;
                                u.command = "SendingMessage";
                                SendToReceiver(u);
                            }
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
                                    user.command = "Match";
                                    user.contact = tmp?.login;
                                    user.avatar = tmp.avatar;
                                    user.phone = tmp.phone;
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
                                             select b.login;
                                var query2 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b.login;
                                var contactLogin = query1.FirstOrDefault();
                                var senderLogin = query2.FirstOrDefault();
                                var query = from b in db.messages
                                            where (b.SenderLogin == senderLogin && b.ReceiverLogin == contactLogin) || (b.SenderLogin == contactLogin && b.ReceiverLogin == senderLogin)
                                            select b;
                                var tmpObjects = query.ToList();
                                user.chat = new List<Chat>();
                                foreach (var obj in tmpObjects)
                                {
                                    Chat c = new Chat();
                                    if (obj.Message != null)
                                    {
                                        c.message = obj.Message;
                                    }
                                    else if (obj.Media != null)
                                    {
                                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                        string folderPath = Path.Combine(baseDirectory, "Media");
                                        string filePath = Path.Combine(folderPath, obj.Media);
                                        c.media = GetImageBytes(filePath);
                                    }
                                    c.chatContact = obj.SenderLogin;
                                    c.date = obj.Date;
                                    user.chat.Add(c);
                                }
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
                            User u = new User();
                            string tmp = null;
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.ip == ip.ToString()
                                            select b.Id;
                                var id = query.FirstOrDefault();
                                var toUpdate = db.users.Find(id);
                                if (toUpdate != null)
                                {
                                    tmp = toUpdate.login;
                                    toUpdate.login = user.login;
                                    toUpdate.password = user.password;
                                    toUpdate.phone = user.phone;
                                    if (user.avatar != null)
                                    {
                                        toUpdate.avatar = user.avatar;
                                    }
                                    toUpdate.ip = ip.ToString();
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
                            List<string> ips = SendChangedProfile(ip);
                            using (var db = new UsersContext())
                            {
                                var query1 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b;
                                var tmp1 = query1.FirstOrDefault();
                                u.login = tmp1.login;
                                u.avatar = tmp1.avatar;
                                u.command = "ContactProfileChanged";
                            }
                            u.mess = tmp;
                            foreach(var i in ips)
                            {
                                u.contact = i;
                                SendToReceiver(u);
                            }
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
        #endregion
        #region Responcing
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
                    byte[] buf = new byte[1000000];
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, buf);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, buf);
                    IPAddress ipAddr = IPAddress.Parse(user.contact);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 49153);
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

        private static List<string> SendChangedProfile(IPAddress ip)
        {
            List<string> ipArr = new List<string>();
            using (var db = new UsersContext())
            {
                var query1 = from b in db.users
                             where b.ip == ip.ToString()
                             select b;
                var tmp = query1.FirstOrDefault();
                var query = (from b in db.messages
                             where b.SenderLogin == tmp.login
                             select b.ReceiverLogin)
                            .Distinct();
                var loginArr = query.ToList();
                foreach(var l in  loginArr)
                {
                    var query2 = from b in db.users
                                 where b.login == l
                                 select b.ip;
                    if(query2.Count() > 0)
                        ipArr.Add(query2.FirstOrDefault());
                }
            }
            return ipArr;
        }

        private static List<Profile> GetContactList(IPAddress ip)
        {
            using (var db = new UsersContext())
            {
                var query1 = from b in db.users
                             where b.ip == ip.ToString()
                             select b.login;
                var log = query1.FirstOrDefault();
                var query = (from b in db.messages
                             where b.SenderLogin == log
                             select b.ReceiverLogin)
                                .Distinct();
                var logArr = query.ToArray();
                List<Profile> list = new List<Profile>();
                if (logArr.Length > 0)
                {
                    foreach (var o in logArr)
                    {
                        var query2 = from b in db.users
                                     where b.login == o
                                     select b;
                        var tmp = query2.FirstOrDefault();
                        Profile profile = new Profile();
                        profile.login = tmp.login;
                        profile.avatar = tmp.avatar;
                        profile.phone = tmp.phone;
                        list.Add(profile);
                    }
                    return list;
                }
                else
                    return list;
            }
        }
        #endregion
        private static byte[] GetImageBytes(string p)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap bitmap = new Bitmap(p);
                bitmap.Save(ms, bitmap.RawFormat);
                return ms.ToArray();
            }
        }
    }
}