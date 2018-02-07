﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenPop.Mime;
using OpenPop.Pop3;
using System.Text.RegularExpressions;
using OpenPop.Pop3.Exceptions;

namespace Tangh.Infrastructure
{
    /// <summary>
    /// 接收邮件帮助类
    /// </summary>
    public class Pop3Helper
    {
        /// <summary>
        /// 邮件消息事件
        /// </summary>
        public event Action<string> ShowMsg;

        /// <summary>
        /// 接受邮件主函数
        /// </summary>
        /// <param name="name">帐号全名</param>
        /// <param name="pwd">密码</param>
        /// <param name="keyWord">关键字</param>
        /// <returns></returns>
        public string ReceiveMails(string name, string pwd, string keyWord)
        {
            string content = string.Empty;
            string server = GetServer(name);
            Dictionary<int, Message> messages = new Dictionary<int, Message>();
            messages = new Dictionary<int, Message>();
            using (Pop3Client pop3Client = new Pop3Client())
            {
                try
                {
                    if (pop3Client.Connected)
                        pop3Client.Disconnect();
                    pop3Client.Connect(server, 110, false);
                    pop3Client.Authenticate(name, pwd);
                    int count = pop3Client.GetMessageCount();
                    int success = 0;
                    int fail = 0;
                    for (int i = count; i >= 1; i -= 1)
                    {
                        // Check if the form is closed while we are working. If so, abort
                        //if (IsDisposed)
                        //    return url;

                        // Refresh the form while fetching emails
                        // This will fix the "Application is not responding" problem
                        //Application.DoEvents();

                        try
                        {
                            Message message = pop3Client.GetMessage(i);

                            // Add the message to the dictionary from the messageNumber to the Message
                            messages.Add(i, message);

                            success++;

                            break;
                        }
                        catch (Exception ex)
                        {
                            this.WriteLog("读取邮件异常" + ex.ToString());
                        }
                    }

                    this.WriteLog("Mail received!\nSuccesses: " + success + "\nFailed: " + fail);

                    if (fail > 0)
                    {
                        //MessageBox.Show(this,
                        //"Since some of the emails were not parsed correctly (exceptions were thrown)\r\n" +
                        //"please consider sending your log file to the developer for fixing.\r\n" +
                        //"If you are able to include any extra information, please do so.",
                        //"Help improve OpenPop!");
                    }

                    // 读取最后一封邮件
                    if (messages.Count > 0)
                    {
                        // 取时间距离当前最近的一封。
                        var list = (from f in messages
                                    where f.Value.Headers.From.DisplayName == keyWord
                                    orderby f.Value.Headers.DateSent descending
                                    select f).ToList();
                        if (list.Count > 0)
                        {
                            Message message = list[0].Value;

                            // 得到的时间是东0区的时间
                            if (message.Headers.DateSent < DateTime.UtcNow.AddHours(-2))
                            {
                                // 邮件已经失效了
                                // throw new Exception("邮件已经失效了");
                            }

                            if (message.Headers.From.DisplayName == keyWord)
                            {
                                MessagePart plainTextPart = message.FindFirstPlainTextVersion();
                                if (plainTextPart != null)
                                {
                                    // The message had a text/plain version - show that one
                                    content = plainTextPart.GetBodyAsText();
                                }
                            }
                        }
                    }
                }
                catch (InvalidLoginException)
                {
                    // 账号异常了。
                    this.WriteLog("The server did not accept the user credentials!");
                }
                catch (PopServerNotFoundException)
                {
                    this.WriteLog("The server could not be found");
                }
                catch (PopServerLockedException)
                {
                    this.WriteLog("The mailbox is locked. It might be in use or under maintenance. Are you connected elsewhere?");
                }
                catch (LoginDelayException)
                {
                    this.WriteLog("Login not allowed. Server enforces delay between logins. Have you connected recently?");
                }
                catch (Exception e)
                {
                    this.WriteLog("Error occurred retrieving mail. " + e.Message);
                }
            }

            return content;
        }

        /// <summary>
        /// 服务地址
        /// </summary>
        /// <param name="name">邮箱地址</param>
        /// <returns></returns>
        private string GetServer(string name)
        {
            if (name.EndsWith("@sohu.com"))
            {
                return "pop3.sohu.com";
            }
            else if (name.EndsWith("@163.com"))
            {
                return "pop.163.com";
            }

            return string.Empty;
        }

        /// <summary>
        /// 写日志事件
        /// </summary>
        /// <param name="msg"></param>
        private void WriteLog(string msg)
        {
            if (ShowMsg != null)
            {
                this.ShowMsg(msg);
            }
        }
    }
}
