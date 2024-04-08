// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsEmail : APISettings
    {
        public override string caption => "Email";

        [SerializeField] string m_RecipientEmail = "recipient_email@example.com";
        [SerializeField] string m_SenderEmail = "your_email@gmail.com";
        [SerializeField] string m_SenderPassword = "your_email_password";
        [SerializeField] string m_SenderServer = "smtp.gmail.com";
        [SerializeField] int m_SmtpPort = 587;
        
        public async override Task<bool> SendReport(ReportData report)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(m_SenderEmail);
            mail.To.Add(m_RecipientEmail);
            mail.Subject = report.headline;
            mail.Body = report.fullLog;

            SmtpClient smtpServer = new SmtpClient(m_SenderServer);
            smtpServer.Port = m_SmtpPort;
            smtpServer.Credentials = new NetworkCredential(m_SenderEmail, m_SenderPassword) as ICredentialsByHost;
            smtpServer.EnableSsl = true;

            if(report.screenshot != null)
            {
                MemoryStream ms = new MemoryStream(report.GetScreenshotByteArray()); 
                ContentType ct = new ContentType(MediaTypeNames.Image.Jpeg);
                Attachment attach = new Attachment(ms, ct);
                attach.ContentDisposition.FileName = "screenshot.png";

                mail.Attachments.Add(attach);
            }

            smtpServer.SendCompleted += (s, e) => {
                if (e.Error == null)
                {
                    Debug.Log("[Email] Success sending a report via Email.");
                    tcs.SetResult(true);
                } 
                else
                {
                    Debug.Log("[Email] Error sending your email:" + e.Error.ToString());
                    tcs.SetResult(false);
                }
            };

            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            smtpServer.SendAsync(mail, null);

            return await tcs.Task;
        }


#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            string changetest = m_SenderServer + m_SenderEmail + m_SenderPassword + m_SmtpPort;
            m_SenderServer = EditorGUILayout.TextField("STMP Server", m_SenderServer);
            m_SenderEmail = EditorGUILayout.TextField("Sender Email", m_SenderEmail);
            m_SenderPassword = EditorGUILayout.PasswordField("Sender Password", m_SenderPassword);
            m_SmtpPort = EditorGUILayout.IntField("Sending port", m_SmtpPort);
            EditorUtils.Space();
            m_RecipientEmail = EditorGUILayout.TextField("Recipient Email", m_RecipientEmail);
            if(!m_TestedWorking && m_SenderServer.Equals("smtp.gmail.com"))
            {
                EditorUtils.Space();
                EditorUtils.SmallLabel("When using the Gmail SMTP server, consider using App Passwords to login. This might be required for email sending to work. App Passwords are only available if 2-factor authentication is setup.");
                if(GUILayout.Button("Google App Passwords"))
                {
                    Application.OpenURL("https://security.google.com/settings/security/apppasswords");
                }
            }
            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that the credentials entered here will be saved in your game build. For security reasons, we suggest setting up a dedicated Gmail account and/or to remove the credentials for release builds.");

            if (!(m_SenderServer + m_SenderEmail + m_SenderPassword + m_SmtpPort).Equals(changetest))
            {
                m_TestedWorking = false;
            }
        }
#endif
    }
}
