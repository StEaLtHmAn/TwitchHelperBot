﻿using CSCore.CoreAudioAPI;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using WebView2 = Microsoft.Web.WebView2.WinForms.WebView2;

namespace TwitchHelperBot
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();
        [DllImport("User32.dll")]
        public static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

        private string RedirectURI = null;
        private int currentWindowID = -1;
        private Process currentProcess = null;
        private bool paused = false;

        public MainForm()
        {
            InitializeComponent();

            ((ToolStripDropDownMenu)toolsToolStripMenuItem.DropDown).ShowImageMargin = false;

            if (!checkForUpdates())
            {
                startupApp();
                Globals.registerAudioMixerHotkeys();
                Globals.keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            }
            else
            {
                Globals.DelayAction(0, new Action(() => { Dispose(); }));
            }
        }

        private bool checkForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                string githubLatestReleaseJsonString = client.DownloadString("https://api.github.com/repos/StEaLtHmAn/TwitchHelperBot/releases/latest");
                JObject githubLatestReleaseJson = JObject.Parse(githubLatestReleaseJsonString);

                Version CurrentVersion = Assembly.GetEntryAssembly().GetName().Version;
                string[] githubVersionNumbersSplit = Regex.Replace(githubLatestReleaseJson["tag_name"].ToString().ToLower(), "^[\\D]", string.Empty).Split('.');

                Version GithubVersion;
                if (githubVersionNumbersSplit.Length == 2)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]));
                else if (githubVersionNumbersSplit.Length == 3)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]), int.Parse(githubVersionNumbersSplit[2]));
                else if (githubVersionNumbersSplit.Length == 4)
                    GithubVersion = new Version(int.Parse(githubVersionNumbersSplit[0]), int.Parse(githubVersionNumbersSplit[1]), int.Parse(githubVersionNumbersSplit[2]), int.Parse(githubVersionNumbersSplit[3]));
                else
                    GithubVersion = new Version();

                if (GithubVersion > CurrentVersion)
                {
                    foreach (JObject asset in githubLatestReleaseJson["assets"] as JArray)
                    {
                        if (asset["content_type"].ToString() == "application/x-zip-compressed")
                        {
                            MessageBox.Show(githubLatestReleaseJson["name"].ToString() + "\r\n\r\n" + githubLatestReleaseJson["body"].ToString(),
                            "New Updates - Released " + Globals.getRelativeDateTime(DateTime.Parse(githubLatestReleaseJson["published_at"].ToString()).Add(DateTimeOffset.Now.Offset)) +" ago", MessageBoxButtons.OK);

                            //download latest zip
                            client.DownloadFile(asset["browser_download_url"].ToString(), asset["name"].ToString());
                            //extract latest updater
                            using (ZipArchive archive = ZipFile.OpenRead(asset["name"].ToString()))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.FullName.Contains("Updater.exe"))
                                        entry.ExtractToFile(entry.FullName, true);
                                }
                            }
                            //run the updater
                            Process.Start("Updater.exe", asset["name"].ToString());
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void startupApp()
        {
            //get LoginName, if we dont have LoginName in the config then we ask the user for his LoginName with a popup textbox
            //without a LoginName we cannot continue so not entering it closes the app
            Globals.loginName = Globals.iniHelper.Read("LoginName");
            if (string.IsNullOrEmpty(Globals.loginName))
            {
                using (TextInputForm testDialog = new TextInputForm("Setup LoginName", "Please enter your twitch LoginName to continue."))
                {
                    if (testDialog.ShowDialog(this) == DialogResult.OK && testDialog.textBox.Text.Length > 0)
                    {
                        Globals.loginName = testDialog.textBox.Text;
                        Globals.iniHelper.Write("LoginName", Globals.loginName);
                    }
                    else
                    {
                        Globals.DelayAction(0, new Action(() => { Dispose(); }));
                        return;
                    }
                }
            }

            //get ClientId, if we dont have ClientId in the config then we ask the user for his ClientId with a popup textbox
            //without a ClientId we cannot continue so not entering it closes the app
            Globals.clientId = Globals.iniHelper.Read("ClientId");
            if (string.IsNullOrEmpty(Globals.clientId))
            {
                using (TextInputForm testDialog = new TextInputForm("Setup ClientID", "We need your application ClientID.\r\n\r\n- Browse here: https://dev.twitch.tv/console/apps/create \r\n- Set OAuthRedirectURL to http://localhost (or something else if you know what you doing)\r\n- Set Category to Broadcaster Suite\r\n- Click Create and copy-paste the ClientID into the box below."))
                {
                    if (testDialog.ShowDialog(this) == DialogResult.OK && testDialog.textBox.Text.Length > 0)
                    {
                        Globals.clientId = testDialog.textBox.Text;
                        Globals.iniHelper.Write("ClientId", Globals.clientId);
                    }
                    else
                    {
                        Globals.DelayAction(0, new Action(() => { Dispose(); }));
                        return;
                    }
                }
            }

            RedirectURI = Globals.iniHelper.Read("AuthRedirectURI");
            if (string.IsNullOrEmpty(RedirectURI))
            {
                using (TextInputForm testDialog = new TextInputForm("Setup OAuthRedirectURL", $"We need the OAuthRedirectURL you entered for your application.\r\nIf you closed the page it can be found here https://dev.twitch.tv/console/apps/{Globals.clientId}"))
                {
                    if (testDialog.ShowDialog(this) == DialogResult.OK && testDialog.textBox.Text.Length > 0)
                    {
                        RedirectURI = testDialog.textBox.Text;
                        Globals.iniHelper.Write("AuthRedirectURI", RedirectURI);
                    }
                    else
                    {
                        Globals.DelayAction(0, new Action(() => { Dispose(); }));
                        return;
                    }
                }
            }

            //load configs
            string tmp = Globals.iniHelper.Read("DarkModeEnabled");
            if (string.IsNullOrEmpty(tmp) || !bool.TryParse(tmp, out _))
            {
                Globals.iniHelper.Write("DarkModeEnabled", "false");
            }
            else
            {
                bool DarkModeEnabled = bool.Parse(Globals.iniHelper.Read("DarkModeEnabled"));
                if (DarkModeEnabled)
                {
                    Globals.ToggleDarkMode(this, DarkModeEnabled);
                    NotificationMenuStrip.BackColor = Globals.DarkColour;
                    NotificationMenuStrip.ForeColor = SystemColors.ControlLightLight;
                    NotificationMenuStrip.Renderer = new MyRenderer();
                    foreach (ToolStripItem item in toolsToolStripMenuItem.DropDownItems)
                    {
                        item.BackColor = Globals.DarkColour;
                        item.ForeColor = SystemColors.ControlLightLight;
                    }
                }
            }
            tmp = Globals.iniHelper.Read("ModifyChannelCooldown");
            if (string.IsNullOrEmpty(tmp))
            {
                Globals.iniHelper.Write("ModifyChannelCooldown", "5000");
                updateChannelInfoTimer.Interval = 5000;
            }
            else
            {
                updateChannelInfoTimer.Interval = int.Parse(tmp);
            }
            if (string.IsNullOrEmpty(Globals.iniHelper.Read("NotificationDuration")))
            {
                Globals.iniHelper.Write("NotificationDuration", "5000");
            }
            if (string.IsNullOrEmpty(Globals.iniHelper.Read("VolumeNotificationDuration")))
            {
                Globals.iniHelper.Write("VolumeNotificationDuration", "3000");
            }

            //Login
            Globals.access_token = Globals.iniHelper.Read("access_token");
            if (string.IsNullOrWhiteSpace(Globals.access_token) || !ValidateToken())
            {
                string scopes = "channel:manage:broadcast+moderator:read:chatters+moderator:read:followers+channel:read:subscriptions+chat:edit+chat:read";
                BrowserForm form = new BrowserForm($"https://id.twitch.tv/oauth2/authorize?client_id={Globals.clientId}&redirect_uri={RedirectURI}&response_type=token&scope={scopes}");
                form.webView2.NavigationCompleted += new EventHandler<CoreWebView2NavigationCompletedEventArgs>(webView2_TwitchAuthNavigationCompleted);
                form.ShowDialog();

                //if token is still not valid then we close the app
                if (!ValidateToken())
                {
                    Globals.DelayAction(0, new Action(() => { Dispose(); }));
                    return;
                }
            }

            //get user details
            Globals.userDetailsResponse = JObject.Parse(Globals.GetUserDetails(Globals.loginName));


            ConnectionCredentials credentials = new ConnectionCredentials(Globals.loginName, Globals.access_token);
            WebSocketClient customClient = new WebSocketClient();
            Globals.twitchChatClient = new TwitchClient(customClient);
            Globals.twitchChatClient.Initialize(credentials, Globals.loginName);
            Globals.twitchChatClient.Connect();
            Globals.twitchChatClient.OnUserBanned += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.UserBan.BanReason))
                    Globals.twitchChatClient.SendMessage(e.UserBan.Channel, $"{e.UserBan.Username} BANNED");
                else
                    Globals.twitchChatClient.SendMessage(e.UserBan.Channel, $"{e.UserBan.Username} BANNED Reason: {e.UserBan.BanReason}");
            };
            Globals.twitchChatClient.OnUserTimedout += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.UserTimeout.TimeoutReason))
                    Globals.twitchChatClient.SendMessage(e.UserTimeout.Channel, $"{e.UserTimeout.Username} BANNED Duration: {Globals.getRelativeTimeSpan(TimeSpan.FromSeconds(e.UserTimeout.TimeoutDuration))}");
                else
                    Globals.twitchChatClient.SendMessage(e.UserTimeout.Channel, $"{e.UserTimeout.Username} BANNED Reason: {e.UserTimeout.TimeoutReason} Duration: {Globals.getRelativeTimeSpan(TimeSpan.FromSeconds(e.UserTimeout.TimeoutDuration))}");
            };
            Globals.twitchChatClient.OnMessageReceived += (sender, e) =>
            {
                if (e.ChatMessage.Bits > 0)
                {
                    Globals.twitchChatClient.SendMessage(e.ChatMessage.Channel, $"MrDestructoid Thanks @{e.ChatMessage.DisplayName}, for the {e.ChatMessage.Bits} bits (${e.ChatMessage.BitsInDollars:0:##})");
                }
            };
            Globals.twitchChatClient.OnNewSubscriber += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.Subscriber.DisplayName}, for the {e.Subscriber.SubscriptionPlan} subscription.");
            };
            Globals.twitchChatClient.OnReSubscriber += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.ReSubscriber.DisplayName}, for the {e.ReSubscriber.Months} Months {e.ReSubscriber.SubscriptionPlan} subscription.");
            };
            Globals.twitchChatClient.OnPrimePaidSubscriber += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.PrimePaidSubscriber.DisplayName}, for the {e.PrimePaidSubscriber.SubscriptionPlan} subscription.");
            };
            Globals.twitchChatClient.OnGiftedSubscription += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.GiftedSubscription.DisplayName}, for the gifted {e.GiftedSubscription.MsgParamSubPlan} subscription.");
            };
            Globals.twitchChatClient.OnContinuedGiftedSubscription += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.ContinuedGiftedSubscription.MsgParamSenderName}, for the gifting {e.ContinuedGiftedSubscription.DisplayName} a subscription.");
            };
            Globals.twitchChatClient.OnCommunitySubscription += (sender, e) =>
            {
                Globals.twitchChatClient.SendMessage(e.Channel, $"MrDestructoid Thanks @{e.GiftedSubscription.DisplayName}, for the gifting {e.GiftedSubscription.MsgParamMassGiftCount} {e.GiftedSubscription.MsgParamSubPlan} subscriptions.");
            };
            Globals.twitchChatClient.OnChatCommandReceived += (sender, e) =>
            {
                switch (e.Command.CommandText.ToLower())
                {
                    case "eskont":
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                string htmlSchedule = webClient.DownloadString("https://www.ourpower.co.za/areas/nelson-mandela-bay/lorraine?block=13");
                                int i1 = htmlSchedule.IndexOf("time dateTime=");
                                int i2 = htmlSchedule.IndexOf("social");
                                htmlSchedule = "<xml><section><h3><" + htmlSchedule.Substring(i1, i2 - i1);
                                int i3 = htmlSchedule.LastIndexOf("</li></span></ul></section>");
                                htmlSchedule = htmlSchedule.Substring(0, i3+ "</li></span></ul></section>".Length) + "</xml>";

                                XmlDocument document = new XmlDocument();
                                document.LoadXml(htmlSchedule);

                                string message = $"MrDestructoid {Globals.userDetailsResponse["data"][0]["display_name"]}'s Loadshedding schedule for today: ";

                                XmlNode section = document.SelectSingleNode("/xml/section");
                                XmlNodeList times = section.SelectNodes("ul/span/li");

                                for (int i = 0; i < times.Count; i++)
                                {
                                    message += times[i].InnerText;
                                    if(i != times.Count-1)
                                        message += " & ";
                                }

                                Globals.twitchChatClient.SendReply(e.Command.ChatMessage.Channel, e.Command.ChatMessage.Id, message);
                            }
                            break;
                        }
                    case "time":
                        {
                            Globals.twitchChatClient.SendReply(e.Command.ChatMessage.Channel, e.Command.ChatMessage.Id, $"MrDestructoid {Globals.userDetailsResponse["data"][0]["display_name"]}'s local time is: {DateTime.Now.ToShortTimeString()} {TimeZone.CurrentTimeZone.StandardName}");
                            break;
                        }
                    case "topviewers":// todo: optimize data collection
                        {
                            List<ViewerListForm.SessionData> Sessions = new List<ViewerListForm.SessionData>();
                            //read x amount of archive data
                            int SessionsArchiveReadCount = int.Parse(Globals.iniHelper.Read("SessionsArchiveReadCount"));
                            for (int i = DateTime.UtcNow.Year; i <= DateTime.UtcNow.Year - SessionsArchiveReadCount; i--)
                            {
                                if (File.Exists($"SessionsArchive{i}.json"))
                                    Sessions.AddRange(JsonConvert.DeserializeObject<List<ViewerListForm.SessionData>>(File.ReadAllText($"SessionsArchive{i}.json")));
                            }
                            //get session data from file
                            if (File.Exists("WatchTimeSessions.json"))
                                Sessions.AddRange(JsonConvert.DeserializeObject<List<ViewerListForm.SessionData>>(File.ReadAllText("WatchTimeSessions.json")));

                            Dictionary<string, TimeSpan> tmpWatchTimeList = new Dictionary<string, TimeSpan>();
                            foreach (var sessionData in Sessions)
                            {
                                foreach (var viewerData in sessionData.WatchTimeData)
                                {
                                    if (!tmpWatchTimeList.ContainsKey(viewerData.Key))
                                    {
                                        tmpWatchTimeList.Add(viewerData.Key, viewerData.Value);
                                    }
                                    else
                                    {
                                        tmpWatchTimeList[viewerData.Key] += viewerData.Value;
                                    }
                                }
                            }

                            int count = 1;
                            IOrderedEnumerable<KeyValuePair<string, TimeSpan>> sortedList = tmpWatchTimeList.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                            string messageToSend = "MrDestructoid ";
                            foreach (KeyValuePair<string, TimeSpan> kvp in sortedList)
                            {
                                string newPart = $"{count}. {kvp.Key} - {Globals.getRelativeTimeSpan(kvp.Value)} | ";
                                if (messageToSend.Length + newPart.Length <= 500)
                                    messageToSend += newPart;
                                else
                                    break;
                                count++;
                            }
                            Globals.twitchChatClient.SendReply(e.Command.ChatMessage.Channel, e.Command.ChatMessage.Id, messageToSend);

                            break;
                        }
                }
            };
            Globals.twitchChatClient.OnError += (sender, e) =>
            {
            };
            Globals.twitchChatClient.OnLog += (sender, e) =>
            {
            };

            //show welcome message
            OverlayNotificationMessage form123 = new OverlayNotificationMessage($"Logged in as {Globals.userDetailsResponse["data"][0]["display_name"]}", Globals.userDetailsResponse["data"][0]["profile_image_url"].ToString(), Globals.userDetailsResponse["data"][0]["id"].ToString());
            form123.Show();

            Globals.windowLocations = File.Exists("WindowLocations.json") ? JObject.Parse(File.ReadAllText("WindowLocations.json")) : new JObject();
            if (Globals.windowLocations["SpotifyPreviewForm"]?["IsOpen"]?.ToString() == "true")
            {
                SpotifyPreviewForm sForm = new SpotifyPreviewForm();
                sForm.Show();
            }
            if (Globals.windowLocations["ViewerListForm"]?["IsOpen"]?.ToString() == "true")
            {
                ViewerListForm sForm = new ViewerListForm();
                sForm.Show();
            }
        }

        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            string[] HotkeysUpList = Globals.iniHelper.ReadKeys("HotkeysUp");
            string processPath = string.Empty;
            bool isUp = false;
            foreach (string key in HotkeysUpList)
            {
                Keys keys = (Keys)int.Parse(Globals.iniHelper.Read(key, "HotkeysUp") ?? "0");
                ModifierKeys modifiers = KeyPressedEventArgs.GetModifiers(keys, out keys);
                if (keys == e.Key && modifiers == e.Modifier)
                {
                    processPath = key;
                    isUp = true;
                    break;
                }
            }
            string[] HotkeysDownList = Globals.iniHelper.ReadKeys("HotkeysDown");
            foreach (string key in HotkeysUpList)
            {
                Keys keys = (Keys)int.Parse(Globals.iniHelper.Read(key, "HotkeysDown") ?? "0");
                ModifierKeys modifiers = KeyPressedEventArgs.GetModifiers(keys, out keys);
                if (keys == e.Key && modifiers == e.Modifier)
                {
                    processPath = key;
                    break;
                }
            }

            Task.Run(() => {
                try
                {
                    using (var sessionEnumerator = AudioManager.GetAudioSessions())
                    {
                        foreach (var session in sessionEnumerator)
                        {
                            using (var sessionControl = session.QueryInterface<AudioSessionControl2>())
                            {
                                bool shouldSkip = true;
                                try
                                {
                                    if ((processPath == "0" && sessionControl.ProcessID == 0) || processPath == sessionControl.Process.MainModule.FileName)
                                        shouldSkip = false;
                                }
                                catch { }

                                if (!shouldSkip)
                                {
                                    using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                                    {
                                        if (isUp)
                                            AudioManager.SetVolumeForProcess(sessionControl.Process.Id, Math.Min(simpleVolume.MasterVolume + 0.01f, 1f));
                                        else
                                            AudioManager.SetVolumeForProcess(sessionControl.Process.Id, Math.Max(simpleVolume.MasterVolume - 0.01f, 0));

                                        if (int.Parse(Globals.iniHelper.Read("VolumeNotificationDuration") ?? "3000") > 0)
                                        {
                                            string name = string.Empty;
                                            if (sessionControl.Process.Id == 0)
                                                name = "System Sounds";
                                            else
                                                try
                                                {
                                                    name = sessionControl.Process?.MainModule?.ModuleName ?? "Unknown";
                                                }
                                                catch { }
                                            Bitmap icon = null;
                                            try
                                            {
                                                if (sessionControl.Process != null && sessionControl.Process.MainModule != null && sessionControl.Process.MainModule.FileName != null)
                                                {
                                                    icon = IconExtractor.GetIconFromPath(sessionControl.Process.MainModule.FileName);
                                                }
                                            }
                                            catch { }
                                            if (icon == null)
                                            {
                                                icon = Bitmap.FromHicon(SystemIcons.WinLogo.Handle);
                                            }
                                            Invoke(new Action(() =>
                                            {
                                                OverlayNotificationVolume form = new OverlayNotificationVolume($"{name} - {(int)(simpleVolume.MasterVolume * 100f)}%", (int)(simpleVolume.MasterVolume * 100f), icon);
                                                form.Show();
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Globals.LogMessage("KeyboardHook_KeyPressed exception: " + ex);
                }
            });
        }

        //this is used to set the on-hover highlight colour of the menustrip
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public MyRenderer() : base(new MyColors()) { }
        }
        private class MyColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected
            {
                get { return Globals.DarkColour2; }
            }
            public override Color MenuItemBorder
            {
                get { return Color.White; }
            }
        }

        //this hides this window from the alt-tab menu
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= WS_EX_TOOLWINDOW;
                return Params;
            }
        }

        private void webView2_TwitchAuthNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            (sender as WebView2).Parent.Text = (sender as WebView2).CoreWebView2.DocumentTitle;
            (sender as WebView2).Parent.BringToFront();

            //if we land on the RedirectURI then grab token and dispose browser
            if ((sender as WebView2).CoreWebView2.Source.StartsWith(RedirectURI))
            {
                Globals.access_token = (sender as WebView2).CoreWebView2.Source.Split('#', '&')[1].Replace("access_token=", string.Empty);
                if (!string.IsNullOrEmpty(Globals.access_token))
                    Globals.iniHelper.Write("access_token", Globals.access_token);
                (sender as WebView2).Parent.Dispose();
            }
            //if we land on twitch login page then we can auto-fill username
            else if ((sender as WebView2).CoreWebView2.Source.StartsWith("https://www.twitch.tv/login"))
            {
                (sender as WebView2).CoreWebView2.ExecuteScriptAsync(
                    $"document.getElementById(\"login-username\").value = \"{Globals.loginName}\";document.getElementById(\"password-input\").focus();");
            }
        }

        private bool ValidateToken()
        {
            RestClient client = new RestClient();
            client.AddDefaultHeader("Authorization", "OAuth " + Globals.access_token);
            RestRequest request = new RestRequest("https://id.twitch.tv/oauth2/validate", Method.Get);
            RestResponse response = client.Execute(request);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool UpdateChannelInfo(string game_id, string title)
        {
            RestClient client = new RestClient();
            client.AddDefaultHeader("Client-ID", Globals.clientId);
            client.AddDefaultHeader("Authorization", "Bearer " + Globals.access_token);
            client.AddDefaultHeader("Content-Type", "application/json");
            RestRequest request = new RestRequest("https://api.twitch.tv/helix/channels", Method.Patch);
            request.AddQueryParameter("broadcaster_id", Globals.userDetailsResponse["data"][0]["id"].ToString());
            JObject parameters = new JObject();
            if (!string.IsNullOrEmpty(game_id))
                parameters.Add("game_id", game_id);
            if (!string.IsNullOrEmpty(title))
                parameters.Add("title", title);
            request.AddJsonBody(parameters.ToString(Newtonsoft.Json.Formatting.None));
            RestResponse response = client.Execute(request);
            if(!response.IsSuccessful)
                Globals.LogMessage("UpdateChannelInfo exception: " + response.Content);
            return response.IsSuccessful;
        }

        private void updateChannelInfoTimer_Tick(object sender, EventArgs e)
        {
            if (paused)
                return;//return if paused

            updateChannelInfoTimer.Enabled = false;

            try
            {
                //if foreground window changes
                int windowID = GetForegroundWindow();
                if (windowID != 0 && currentWindowID != windowID)
                {
                    currentWindowID = windowID;
                    GetWindowThreadProcessId(currentWindowID, out int processID);
                    currentProcess = Process.GetProcessById(processID);
                    //if to prevent exceptions
                    if (currentProcess != null && !currentProcess.HasExited && !string.IsNullOrEmpty(currentProcess?.MainWindowTitle))
                    {
                        string forgroundAppName = currentProcess?.MainModule?.FileName ?? string.Empty;

                        if (Globals.iniHelper.SectionNames().Contains(forgroundAppName))
                        {
                            string PresetTitle = Globals.iniHelper.Read("PresetTitle", forgroundAppName);
                            JObject category = JObject.Parse(Globals.iniHelper.Read("PresetCategory", forgroundAppName));

                            if (UpdateChannelInfo(category["id"].ToString(), PresetTitle))
                            {
                                OverlayNotificationMessage form = new OverlayNotificationMessage($"Channel Info Updated\r\n{category["name"]}\r\n{PresetTitle}", category["box_art_url"].ToString(), category["id"].ToString());
                                form.Show();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.LogMessage("updateChannelInfoTimer_Tick exception: " + ex);
            }

            //old code was used as a backup to set category/title based on whats running and not whats in forground

            //catch (Win32Exception w32ex)
            //{
            //    //backup method if first method is failing
            //    if (w32ex.GetType() == typeof(Win32Exception))
            //    {
            //        foreach (Process p in Process.GetProcesses())
            //        {
            //            try
            //            {
            //                if (string.IsNullOrEmpty(p?.MainWindowTitle) || p.HasExited)
            //                    continue;
            //                string forgroundAppName = p?.MainModule?.FileName ?? string.Empty;
            //                if (Globals.iniHelper.SectionNames().Contains(forgroundAppName))
            //                {
            //                    string PresetTitle = Globals.iniHelper.Read("PresetTitle", forgroundAppName);
            //                    string PresetCategory = Globals.iniHelper.Read("PresetCategory", forgroundAppName);
            //                    string PresetCategoryID = PresetCategory.Substring(PresetCategory.LastIndexOf(" - ") + 3);

            //                    UpdateChannelInfo(PresetCategoryID, PresetTitle);
            //                    break;
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                Globals.LogMessage("timer1_Tick exception: " + ex);
            //            }
            //        }
            //    }
            //}
            updateChannelInfoTimer.Enabled = true;
        }

        private void setupPresetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<GameMatcherForm>().Count() == 0)
            {
                GameMatcherForm form = new GameMatcherForm();
                form.ShowDialog();
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            paused = !paused;

            if (paused)
                pauseToolStripMenuItem.Text = "Resume Channel Edits";
            else
                pauseToolStripMenuItem.Text = "Pause Channel Edits"; 
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<SettingsForm>().Count() == 0)
            {
                SettingsForm form = new SettingsForm();
                form.ShowDialog();
            }
        }

        private void AudioMixerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<AudioMixerForm>().Count() == 0)
            {
                AudioMixerForm form = new AudioMixerForm();
                form.ShowDialog();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Globals.keyboardHook.Dispose();
        }

        private void showViewerListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<ViewerListForm>().Count() == 0)
            {
                ViewerListForm sForm = new ViewerListForm();
                sForm.Show();

                if (Globals.windowLocations[sForm.Name] == null)
                {
                    Globals.windowLocations.Add(sForm.Name, new JObject()
                    {
                        { "Location", $"{sForm.Location.X}x{sForm.Location.Y}"},
                        { "IsOpen", "true"}
                    });
                    File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));
                }
                else if (Globals.windowLocations[sForm.Name]?["IsOpen"].ToString() != "true")
                {
                    Globals.windowLocations[sForm.Name]["IsOpen"] = "true";
                    File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
            else
            {
                var form = Application.OpenForms.OfType<ViewerListForm>().First();

                Globals.windowLocations[form.Name]["IsOpen"] = "false";
                File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));

                form.Dispose();
            }
        }

        private void NotificationMenuStrip_Opened(object sender, EventArgs e)
        {
            toolsToolStripMenuItem.ShowDropDown();
        }

        private void spotifyPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<SpotifyPreviewForm>().Count() == 0)
            {
                SpotifyPreviewForm sForm = new SpotifyPreviewForm();
                sForm.Show();

                if (Globals.windowLocations[sForm.Name] == null)
                {
                    Globals.windowLocations.Add(sForm.Name, new JObject()
                    {
                        { "Location", $"{sForm.Location.X}x{sForm.Location.Y}"},
                        { "IsOpen", "true"}
                    });
                    File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));
                }
                else if (Globals.windowLocations[sForm.Name]?["IsOpen"].ToString() != "true")
                {
                    Globals.windowLocations[sForm.Name]["IsOpen"] = "true";
                    File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));
                }
            }
            else
            {
                var form = Application.OpenForms.OfType<SpotifyPreviewForm>().First();

                Globals.windowLocations[form.Name]["IsOpen"] = "false";
                File.WriteAllText("WindowLocations.json", Globals.windowLocations.ToString(Newtonsoft.Json.Formatting.None));

                form.Dispose();
            }
        }

        public new void Dispose()
        {
            if (Application.OpenForms.OfType<SpotifyPreviewForm>().Count() > 0)
                Application.OpenForms.OfType<SpotifyPreviewForm>().First().Dispose();
            if (Application.OpenForms.OfType<ViewerListForm>().Count() > 0)
                Application.OpenForms.OfType<ViewerListForm>().First().Dispose();
            if(Globals.twitchChatClient != null && Globals.twitchChatClient.IsConnected)
                Globals.twitchChatClient.Disconnect();
            base.Dispose();
        }
    }
}