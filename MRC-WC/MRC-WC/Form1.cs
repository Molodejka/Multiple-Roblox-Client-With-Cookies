﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace MRC_WC
{
    public partial class Form1 : Form
    {
        private static string[] cookies = null;
        private static string gameid = string.Empty;
        private static int maxclients;
        private string currentapp_path = Directory.GetCurrentDirectory();

        public Form1()
        {
            InitializeComponent();
        }

        private string output(string arg)
        {
            Invoke((Action)delegate
            {
                output_tb.AppendText(arg + "\r\n");
            });
            return null;
        }

        private int getrobloxcount()
        {
            int launched = 0;
            Process[] robloxs = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (Process process in robloxs)
            {
                launched += 1;   
            }
            return launched;
        }


        private void LaunchRoblox(string authcode)
        {
            Random rnd = new Random();
            long browserTrackerId = 55393295400 + rnd.Next(1, 100);
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int launchTime = (int)t.TotalSeconds * 1000;
            string url = $@"roblox-player:1+launchmode:play+gameinfo:{authcode}+launchtime:{launchTime}+placelauncherurl:https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestGame&browserTrackerId="+ browserTrackerId + "&placeId={"+ gameid +"}&isPlayTogetherGame=false+browsertrackerid:{"+ browserTrackerId +"}+robloxLocale:en_us+gameLocale:en_us";
            System.Diagnostics.Process.Start(url);
        }



        private async void Visit(string cookie)
        {
                try
                {
                    var baseAddress = new Uri("https://auth.roblox.com/v1/authentication-ticket/");
                    var cookieContainer = new CookieContainer();
                    var request = new HttpRequestMessage(HttpMethod.Post, baseAddress);
                    request.Headers.Add("User-Agent", "Roblox/WinInet");
                    request.Headers.Add("Referer", "https://www.roblox.com/develop");
                    request.Headers.Add("RBX-For-Gameauth", "true");

                    var request2 = new HttpRequestMessage(HttpMethod.Post, baseAddress);
                    request2.Headers.Add("User-Agent", "Roblox/WinInet");
                    request2.Headers.Add("Referer", "https://www.roblox.com/develop");
                    request2.Headers.Add("RBX-For-Gameauth", "true");

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer, AllowAutoRedirect = false })
                    using (var client = new HttpClient(handler))
                    {
                        cookieContainer.Add(baseAddress, new Cookie(".ROBLOSECURITY", cookie));
                        var result = await client.SendAsync(request);
                        var xcsrf = (String[])result.Headers.GetValues("X-CSRF-TOKEN");
                        output("Got X-CSRF Token:" + xcsrf[0]);

                        request2.Headers.Add("X-CSRF-TOKEN", xcsrf[0]);
                        result = await client.SendAsync(request2);
                        var authcode = (String[])result.Headers.GetValues("rbx-authentication-ticket");
                        output("Got Auth Token:" + authcode[0]);
                        LaunchRoblox(authcode[0]);
                }


                }
                catch (Exception exc)
                {
                    output("Failed to connect, Trying again! ");
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text documents (*.txt*)|*.txt*";
            var dialog = openFileDialog.ShowDialog();
                if (dialog == DialogResult.OK)
                {
                    cookiespath_tb.AppendText(openFileDialog.FileName);
                    cookies = File.ReadAllLines(openFileDialog.FileName);

                    output("Added cookies!");
                }
                else if (dialog != DialogResult.OK)
                    output("Cookies were not added!");
        }

        


        private void start_bt(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("multi");


            maxclients = int.Parse(clients_tb.Text);
            gameid = gameid_tb.Text;

            if (cookies == null)
                output("Please add cookies!");
            else
            {
                if (pname.Length == 0)
                    System.Diagnostics.Process.Start(currentapp_path + @"\multi.exe");
                else
                    output("Multi roblox already running, starting process!");


                Thread.Sleep(2000);

                try
                {
                    for (int clients = getrobloxcount(); clients < maxclients; clients++)
                    {
                        Visit(cookies[clients]);
                        output("Current clients: " + clients);
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }

        }


        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("multi");
            foreach (Process process in pname)
            {
                process.Kill();
                process.WaitForExit();
            }
        }
    }
}