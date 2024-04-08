//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OTBT.Framework.Networking
{
    public class PeytonUtility 
    {

        private static string s_PeytonBuildReport = "1091289105633124414";

        public static async Task PeytonTicket(string subject, string bodytext, string usertext, int projectID, Sprite screenshot, string discordChannel = "1091289105633124414")
        {
            WWWForm form = new WWWForm();
            form.AddField("key", "jens123");
            form.AddField("subject", subject);
            form.AddField("bodytext", bodytext);
            form.AddField("usertext", usertext);
            form.AddField("projectnum", projectID);
            //form.AddBinaryData("screenshot", screenshot.texture.EncodeToPNG(), "screenshot.png", "image/png");
            form.AddField("discordChannel", discordChannel);

            UnityWebRequest request_internet = UnityWebRequest.Post($"https://api.jensbahr.com/createOpenprojectTask", form);
            UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();

            while (!op.isDone)
                await Task.Delay(1000 / 30);

            if (request_internet.result != UnityWebRequest.Result.Success)
                UnityEngine.Debug.Log(request_internet.error);
        }

        public static async Task PeytonMail(string post, string subject)
        {
            WWWForm form = new WWWForm();
            form.AddField("passwd", "OTBT4eva");
            form.AddField("receipients", "jens@beatentrack.games");
            form.AddField("subjectLine", subject);
            form.AddField("plainText", post);
            form.AddField("htmltext", "<html><body>" + post + "</body></html>");

            UnityWebRequest request_internet = UnityWebRequest.Post("https://api.jensbahr.com/newsletterSendMail", form);
            UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();

            while (!op.isDone)
                await Task.Delay(1000 / 30);

            if (request_internet.result != UnityWebRequest.Result.Success)
                UnityEngine.Debug.Log(request_internet.error);
        }

        public static async Task PeytonPost(string post, string channel = "")
        {
            if (channel.Equals("")) channel = s_PeytonBuildReport;
            WWWForm form = new WWWForm();
            form.AddField("passwd", "OTBT4eva");
            form.AddField("channel", channel);
            form.AddField("message", post);

            UnityWebRequest request_internet = UnityWebRequest.Post("https://api.jensbahr.com/peytonPostMessage", form);
            UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();

            while (!op.isDone)
                await Task.Delay(1000 / 30);

            if (request_internet.result != UnityWebRequest.Result.Success)
                UnityEngine.Debug.Log(request_internet.error);
        }
    }
}
