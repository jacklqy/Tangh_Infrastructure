﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Tangh.Infrastructure
{
    public class SmtpHelper
    {
        /// <summary>
        /// 发送邮件帐号
        /// </summary>
        private string sendMailAccount = string.Empty;

        /// <summary>
        /// 发送邮件密码
        /// </summary>
        private string sendMailPwd = string.Empty;

        /// <summary>
        /// 发送SMTP服务器
        /// </summary>
        private string sendSMTP = string.Empty;

        /// <summary>
        /// 收件人邮箱：多个用逗号（英文)隔开
        /// </summary>
        private string notifyEmail = string.Empty;

        public SmtpHelper(string sendMailAccount, string sendMailPwd, string sendSMTP, string notifyEmail)
        {
            this.sendMailAccount = sendMailAccount;
            this.sendMailPwd = sendMailPwd;
            this.sendSMTP = sendSMTP;
            this.notifyEmail = notifyEmail;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailFrom">帐户名显示标题</param>
        /// <param name="mailTitle">主题</param>
        /// <param name="mailContent">内容</param>
        /// <param name="isBodyHtml">是否是html</param>
        public void MailSend(string mailTitle, string mailContent, bool isBodyHtml)
        {
            MailMessage message = new MailMessage();
            if (sendMailAccount.Trim() == "")
            {
                throw new Exception("发送邮件不可以为空");
            }
            message.From = new MailAddress(sendMailAccount);

            List<string> mailTo = notifyEmail.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (mailTo.Count <= 0)
            {
                throw new Exception("接收邮件不可以为空");
            }

            foreach (string s in mailTo)
            {
                message.To.Add(new MailAddress(s));
            }

            message.Subject = mailTitle;
            message.Body = mailContent;
            message.BodyEncoding = Encoding.UTF8;   //邮件编码
            message.IsBodyHtml = isBodyHtml;      //内容格式是否是html
            message.Priority = MailPriority.High;  //设置发送的优先集

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = sendSMTP;
            string name = sendMailAccount.Substring(0, sendMailAccount.IndexOf("@"));
            smtpClient.Credentials = new NetworkCredential(name, sendMailPwd);
            smtpClient.Timeout = 5000;
            smtpClient.EnableSsl = false;        //不使用ssl连接
            smtpClient.Send(message);
        }
    }
}
