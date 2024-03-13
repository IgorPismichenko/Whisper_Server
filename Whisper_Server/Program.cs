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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Whisper_Server
{
    class Program
    {
        //static Socket socket;
        static void Main(string[] args)
        {
            //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                    sListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 10000000);
                    sListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 10000000);
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
                            User u = new User();
                            //string tmp1 = null;
                            WriteLine("User " + user.login + " sent authorization request on " + DateTime.Now.ToString());
                            using (var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.login == user.login && b.password == user.password
                                            select b;
                                var query1 = from b in db.users
                                            where b.ip == ip.ToString()
                                            select b.Id;

                                var tmp = query.FirstOrDefault();

                              

                                if (query.Count() > 0)
                                {
                                    user.command = "AcceptLog";
                                    user.avatar = tmp.avatar;
                                    user.phone = tmp.phone;
                                    user.online = "green";


                                    user.profile = GetContactList(ip);
                                    WriteLine("User " + user.login + " is authorized on " + DateTime.Now.ToString());

                                    

                                    var id = query1.FirstOrDefault();
                                    var toUpdate = db.users.Find(id);

                                    if (toUpdate != null)
                                    {
                                        
                                        toUpdate.isOnline = "green";
                                        db.SaveChanges();
                                    }
                                    using (var db2 = new UsersContext())
                                    {
                                        var query2 = (from b in db2.messages
                                                      where b.SenderIp == ip.ToString()
                                                      select b.ReceiverIp).Distinct();

                                        var receiverIps = query2.ToList();



                                        //var query3 = from b in db2.users
                                        //             where b.login == user.contact
                                        //             select b.ip;
                                        //var tmp3 = query3.FirstOrDefault();
                                        //var tmp4 = db2.blackList.Where(x => x.BlokedIp == ip.ToString() && x.BlockerIp == tmp3.ToString()).FirstOrDefault();

                                        //var tmp4 = query4.FirstOrDefault();

                                        //var onlineUsers = db.users.Where(u => u.ip == ip.ToString() && u.isOnline == u.isOnline).ToList();

                                        var onlineUsers = from b in db2.users
                                                          where b.ip == ip.ToString()
                                                          select b;
                                        var onlineReceiverIps = onlineUsers.FirstOrDefault();

                                        var check = (from b in db2.blackList
                                                     where b.BlockerIp == ip.ToString()
                                                     select b.BloсkedIp).Distinct();
                                        var blackList = check.ToList();


                                        u.login = onlineReceiverIps.login;
                                        u.command = "ContactIsOnline";


                                        u.online = "green";
                                        foreach (var onlineUser in receiverIps)
                                        {

                                           foreach(var blockerUser in blackList)
                                           {
                                                u.online = "black";
                                                
                                           }

                                               
                                           u.contact = onlineUser;
                                           SendToReceiver(u);
                                          
                                              
                                        }


                                    }


                                }
                                else if (user.login == "login" || user.password == "password" || query.Count() == 0)
                                {
                                    user.command = "Denied";
                                    WriteLine("User " + user.login + " entered incorrect data - denied " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);

                            //u = SendOnlineStatus(ip);
                            //u.mess = tmp1;
                            //SendToReceiver(u);
                        }
                        else if (user.command == "Register")
                        {
                            //User u = new User();
                            //string tmp1 = null;
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
                                    var User = new Users() { login = user.login, password = user.password, phone = user.phone, ip = ip.ToString(), avatar = user.avatar , isOnline = "green"};
                                    db.users.Add(User);
                                    db.SaveChanges();
                                    user.command = "Accept";
                                    user.online = "green";

                                    WriteLine("New user " + user.login + " is registered on " + DateTime.Now.ToString());
                                }
                            }
                            Responce(handler, user);

                            //u = SendOnlineStatus(ip);
                            //u.mess = tmp1;
                            //SendToReceiver(u);
                        }
                        else if (user.command == "Send")
                        {
                           
                            WriteLine("User " + user.login + " sent message on " + DateTime.Now.ToString() + " to " + user.contact);
                            using (var db = new UsersContext())
                            {


                                var query = from b in db.users
                                            where b.login == user.contact
                                            where b.login == user.contact
                                            select b.ip;
                                var tmp = query.FirstOrDefault();

                                var query2 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b.login;
                                var tmp2 = query2.FirstOrDefault();

                              

                                var query3 = from b in db.blackList
                                             where b.BlockerIp == ip.ToString() && b.BloсkedIp == tmp.ToString() && b.Value == true
                                             select b;
                                var tmp3 = query3.FirstOrDefault();



                                var query4 = from b in db.blackList
                                             where b.BlockerIp == tmp.ToString() && b.BloсkedIp == ip.ToString() && b.Value == true
                                             select b;
                                var tmp4 = query4.FirstOrDefault();

                                var query5 = from b in db.users
                                             where b.ip == tmp.ToString()
                                             select b.login;
                                var tmp5 = query5.FirstOrDefault();

                                if (tmp3 != null ) 
                                {
                                    //user.online = "black";
                                    //user.contact = tmp4.ToString();
                                    user.login = tmp2.ToString();
                                   
                                    user.command = "UserInBlackList";
                                    Responce(handler, user);
                                }
                                if ( tmp4 != null)
                                {
                                    //user.online = "black";
                                    //user.contact = tmp4.ToString();
                                   
                                    user.contact = tmp5.ToString();
                                    user.command = "UserInBlackList";
                                    Responce(handler, user);
                                }
                                else if (tmp3 == null || tmp4 == null) 
                                {
                                    
                                    var message = new Messages() { SenderIp = ip.ToString(), ReceiverIp = tmp?.ToString(), Message = user.mess };
                                    db.messages.Add(message);
                                    db.SaveChanges();
                                    var query1 = from b in db.messages
                                                 where (b.SenderIp == ip.ToString() && b.ReceiverIp == tmp) || (b.SenderIp == tmp && b.ReceiverIp == ip.ToString())
                                                 select b.Message;
                                    user.chat = query1.ToList();
                                    user.contact = tmp;
                                    user.login = tmp2;
                                    user.command = "SendingMessage";
                                    SendToReceiver(user);
                                }

                               
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
                            User u = new User();
                            string tmp1 = null;
                            using (var db = new UsersContext())
                            {
                                var query1 = from b in db.users
                                             where b.login == user.contact
                                             select b;
                                var temp = query1.FirstOrDefault();

                                var query2 = from b in db.users
                                            where b.login == user.contact
                                            where b.login == user.contact
                                            select b.ip;
                                var tmp2 = query2.FirstOrDefault();

                                var query = from b in db.messages
                                            where (b.SenderIp == ip.ToString() && b.ReceiverIp == temp.ip) || (b.SenderIp == temp.ip && b.ReceiverIp == ip.ToString())
                                            select b.Message;

                                var onlineUsers = from b in db.users
                                                  where b.ip == ip.ToString()
                                                  select b;
                                var onlineReceiverIps = onlineUsers.FirstOrDefault();

                                var query3 = from b in db.blackList
                                             where b.BloсkedIp == tmp2.ToString() && b.BlockerIp == ip.ToString()
                                             select b;
                                var tmp3 = query3.FirstOrDefault();

                                var query4 = from b in db.blackList
                                             where b.BloсkedIp == ip.ToString() && b.BlockerIp == tmp2.ToString()
                                             select b;
                                var tmp4 = query4.FirstOrDefault();

                                if (tmp3 != null)
                                {
                                    //user.contact = temp.ToString();
                                   
                                    user.block = tmp3.Value;

                                    user.avatar = temp.avatar;
                                    user.chat = query.ToList();
                                    user.login = temp.login;
                                    user.online = temp.isOnline;
                                    user.command = "Chat";
                                }
                                if (tmp4 != null)
                                {
                                    user.contact = temp.ToString();
                                    user.online = "black";
                                    user.avatar = temp.avatar;
                                    user.chat = query.ToList();
                                    user.login = temp.login;
                                    user.command = "Chat";

                                }
                                if (tmp3 == null && tmp4 == null) 
                                {
                                    user.avatar = temp.avatar;
                                    user.chat = query.ToList();
                                    user.login = temp.login;
                                    user.online = temp.isOnline;
                                    user.command = "Chat";

                                }

                                //user.avatar = temp.avatar;
                                //user.chat = query.ToList();
                                //user.login = temp.login;
                                //user.online = temp.isOnline;
                                //user.command = "Chat";
                                

                                //using (var db2 = new UsersContext())
                                //{
                                //    var query2 = (from b in db2.messages
                                //                  where b.SenderIp == ip.ToString()
                                //                  select b.ReceiverIp).Distinct();

                                //    var receiverIps = query2.ToList();


                                //    //var onlineUsers = db.users.Where(u => u.ip == ip.ToString() && u.isOnline == u.isOnline).ToList();
                                //    var onlineUsers = from b in db2.users
                                //                      where b.ip == ip.ToString()
                                //                      select b;
                                //    var onlineReceiverIps = onlineUsers.FirstOrDefault();
                                //    u.login = onlineReceiverIps.login;
                                //    u.command = "ContactIsOnline";
                                //    u.online = onlineReceiverIps.isOnline;

                                //    foreach (var onlineUser in receiverIps)
                                //    {

                                //        u.contact = onlineUser;
                                //        SendToReceiver(u);

                                //    }


                                //}

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
                            u = SendChangedProfile(ip);
                            u.mess = tmp;
                            SendToReceiver(u);
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
                        else if (user.command == "DeleteUser")
                        {

                            using (var db = new UsersContext())
                            {
                                var query1 = from b in db.users
                                             where b.login == user.contact
                                             select b.ip;
                                var temp = query1.FirstOrDefault();
                                var chatToDelete = from b in db.messages
                                                       where (b.SenderIp == ip.ToString() && b.ReceiverIp == temp) || (b.SenderIp == temp && b.ReceiverIp == ip.ToString())
                                                       select b;

                                foreach (var message in chatToDelete)
                                {
                                    db.messages.Remove(message);
                                }

                                db.SaveChanges();

                                user.command = "successfulDeleted";
                            }
                            WriteLine("User " + user.login + " deleted chat with " + user.contact + " in" + DateTime.Now.ToString());


                            Responce(handler, user);
                        }
                        else if(user.command == "DeleteSms")
                        {
                            using (var db = new UsersContext())
                            {

                                var query = from b in db.messages
                                                      where (b.Message == user.mess)
                                                      select b;

                               
                                var messageToDelete = query.FirstOrDefault();
                                if (messageToDelete != null)
                                {
                                    db.messages.Remove(messageToDelete);
                                    db.SaveChanges();
                                    WriteLine("User " + user.login + " deleted sms in chat with " + user.contact + " at " + DateTime.Now.ToString());
                                    user.command = "successfulDeletedSms";
                                }
                                else
                                {
                                    WriteLine("Message not found or already deleted.");
                                    user.command = "failedDeletedSms";
                                }
                              

                                
                            }
                            WriteLine("User " + user.login + " deleted sms in chat with " + user.contact + " at" + DateTime.Now.ToString());


                            Responce(handler, user);
                        }
                        else if(user.command == "CloseCommand")
                        {
                            User u = new User();

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
                                    toUpdate.isOnline = user.online;
                                    db.SaveChanges();
                                }
                                //user.command = "statusSaved";

                            }
                            Responce(handler, user);

                            //u = SendOnlineStatus(ip);

                            using (var db = new UsersContext())
                            {
                                var query = (from b in db.messages
                                             where b.SenderIp == ip.ToString()
                                             select b.ReceiverIp).Distinct();

                                var receiverIps = query.ToList();

                               
                                    //var onlineUsers = db.users.Where(u => u.ip == ip.ToString() && u.isOnline == u.isOnline).ToList();
                                    var onlineUsers = from b in db.users
                                                 where b.ip == ip.ToString()
                                                 select b;
                                    var onlineReceiverIps = onlineUsers.FirstOrDefault();

                                    var check = (from b in db.blackList
                                                 where b.BlockerIp == ip.ToString()
                                                 select b.BloсkedIp).Distinct();
                                    var blackList = check.ToList();

                                    u.login = onlineReceiverIps.login;
                                    u.command = "ContactIsOnline";
                                    u.online = "red";

                                    foreach (var onlineUser in receiverIps)
                                    {
                                        foreach (var blockerUser in blackList)
                                        {
                                            u.online = "black";

                                        }
                                        u.contact = onlineUser;
                                        SendToReceiver(u);

                                    }
                                
                            }

                            
                        }
                        else if(user.command == "BlockContact")
                        {
                            using(var db = new UsersContext())
                            {
                                var query = from b in db.users
                                            where b.login == user.contact
                                            where b.login == user.contact
                                            select b.ip;
                                var tmp = query.FirstOrDefault();

                                var query2 = from b in db.users
                                             where b.ip == ip.ToString()
                                             select b.login;

                                var tmp2 = query2.FirstOrDefault();

                                //var query3 = from b in db.blackList
                                //            where b.BlockerIp == ip.ToString() &&  b.BlokedIp == tmp.ToString()
                                //            select b.Value;
                                //var tmp3 = query.FirstOrDefault();
                                //if(tmp3 == null)
                                //{
                                //    var content = new BlackList() { BlockerIp = ip.ToString(), BlokedIp = tmp?.ToString(), Value = true };
                                //    db.blackList.Add(content);
                                //    db.SaveChanges();
                                //    user.block = true;
                                //    user.command = "BlockIsSuccessful";

                                //}
                                //else
                                //{
                                //    user.command = "CantBeBlock";
                                //}

                                var isBlocked = db.blackList.Any(b => b.BlockerIp == ip.ToString() && b.BloсkedIp == tmp.ToString());

                                if (!isBlocked)
                                {
                                    var content = new BlackList() { BlockerIp = ip.ToString(), BloсkedIp = tmp?.ToString(), Value = true };
                                    db.blackList.Add(content);
                                    db.SaveChanges();
                                    user.block = true;
                                    user.command = "BlockIsSuccessful";
                                }
                                else
                                {
                                    user.command = "CantBeBlock";
                                }
                            }
                            Responce(handler, user);
                        }
                        else if(user.command == "UnblockContact")
                        {
                            using(var db = new UsersContext())
                            {
                                var query = from b in db.blackList
                                            where b.BlockerIp == ip.ToString()
                                            
                                            select b;
                                var tmp = query.FirstOrDefault();
                                db.blackList.Remove(tmp);
                                db.SaveChanges();

                                user.command = "UnblockIsSuccessful";
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
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 10000000);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 10000000);
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
                        //GetProfileRequest();
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

        private static User SendChangedProfile(IPAddress ip)
        {
            User u = new User();
            using (var db = new UsersContext())
            {
                var query = (from b in db.messages
                             where b.SenderIp == ip.ToString()
                             select b.ReceiverIp)
                            .Distinct();
                var ipArr = query.FirstOrDefault();
                var query1 = from b in db.users
                             where b.ip == ip.ToString()
                             select b;
                var tmp = query1.FirstOrDefault();
                u.login = tmp.login;
                u.avatar = tmp.avatar;
                u.command = "ContactProfileChanged";
                u.contact = ipArr;
            }
            return u;
        }
        
       

        private static List<Profile> GetContactList(IPAddress ip)
        {
            using (var db = new UsersContext())
            {
                var query = (from b in db.messages
                             where b.SenderIp == ip.ToString()
                             select b.ReceiverIp)
                                .Distinct();
                var ipArr = query.ToArray();
                List<Profile> list = new List<Profile>();
                if (ipArr.Length > 0)
                {
                    foreach (var o in ipArr)
                    {
                        var query1 = from b in db.users
                                     where b.ip == o
                                     select b;
                        var tmp = query1.FirstOrDefault();
                        Profile profile = new Profile();
                        profile.login = tmp.login;
                        profile.avatar = tmp.avatar;
                        profile.phone = tmp.phone;
                        profile.isOnline = tmp.isOnline;
                        list.Add(profile);
                    }
                    return list;
                }
                else
                    return list;
            }
        }

        //private static async void GetProfileRequest()
        //{
        //    await Task.Run(() =>
        //    {
        //        try
        //        {
        //            byte[] bytes = new byte[1024];
        //            int bytesRec = 0;
        //            while (true)
        //            {
        //                bytesRec = socket.Receive(bytes);
        //                if (bytesRec == 0)
        //                {
        //                    socket.Shutdown(SocketShutdown.Both);
        //                    socket.Close();
        //                    return;
        //                }
        //                MemoryStream stream = new MemoryStream(bytes, 0, bytesRec);
        //                DataContractJsonSerializer jsonFormatter = null;
        //                jsonFormatter = new DataContractJsonSerializer(typeof(string));
        //                var str = (string)jsonFormatter.ReadObject(stream);
        //                stream.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLine("Получение рассылки" + ex.Message);
        //        }
        //    });
        //}
    }
}