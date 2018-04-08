using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SKStudios.Portals.Editor
{
    public class FeedbackPanel : GUIPanel
    {
        [MenuItem(SettingsWindow.baseMenuPath + "Feedback", priority = 310)]
        static void Show()
        {
            SettingsWindow.Show(true, 6);
        }

        internal static class Content
        {
            public static readonly GUIContent ratingPrompt = new GUIContent("How would you rate PortalKit Pro?");
            public static readonly GUIContent supportLabel = new GUIContent("I need help!");
            public static readonly GUIContent s1Text = new GUIContent("Send us an email");
            public static readonly string s1WebsiteLink = "mailto:support@skstudios.zendesk.com";

            public static readonly GUIContent s2Text = new GUIContent("Open the Documentation");
            public static string s2WebsiteLink {
                get {
                    string assetName = "PortalKit Pro";
                    string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    if (path == null)
                        return "";
                    path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
                    string root = path.Substring(0, path.LastIndexOf(assetName) + (assetName.Length + 1));
                    string PDFPath = root + "README.pdf";
                    return PDFPath;
                }
            }

            public static readonly GUIContent ReviewText = new GUIContent(
                "I'm glad to hear that you're enjoying the asset! Do you want to leave a review or a rating?");

            public static readonly GUIContent SupportText = new GUIContent(
                "I'm sorry that you're having a bad experience with " +
                "the asset. If you need it, please contact my support line here:");

            public static readonly GUIContent SupportLinkText = new GUIContent("Support Contact");

            public static readonly GUIContent RatingLinkText = new GUIContent("Leave a review/rating in the store");
            public static readonly string RatingLink = "https://www.assetstore.unity3d.com/en/#!/content/81638";

            public static readonly GUIContent GalleryText = new GUIContent(
                "I made something cool! ");
            public static readonly GUIContent GalleryResponseText = new GUIContent(
                "Awesome! If you want, you can submit it to our (wip) gallery of totally awesome projects");
            public static readonly GUIContent GalleryLinkText = new GUIContent("Submit it to our gallery!");
            public static readonly string GalleryLink = "mailto:superkawaiiltd@gmail.com";

            public static readonly GUIContent EmailField = new GUIContent("Email: ");
            public static readonly GUIContent SubmitFeedback = new GUIContent("Submit");
            public static readonly GUIContent SubmittedFeedback = new GUIContent("Sent!");
        }

        private const int maxRating = 5;
        private const int supportRating = 3;

        private static readonly float starSize = EditorGUIUtility.singleLineHeight * 1.5f;
        private static readonly float starSizeFull = starSize * maxRating;

        private static string email = "If you want us to contact you back, leave your email";
        internal static class Styles
        {
            static bool _initialized = false;

            public static GUIStyle TitleStyle;
            public static GUIStyle bgStyle;
            public static GUIStyle subHeadingStyle;
            public static GUIStyle starSliderStyle;
            public static GUIStyle feedbackLabelStyle;
            public static GUIStyle feedbackWindowStyle;

            public static Texture2D starEmptyTex;
            public static Texture2D starFullTex;

            public static GUIStyle EmailStyle;
            public static GUIStyle SubmitStyle;

            public static void Init()
            {

                if (_initialized) return;
                _initialized = true;

                TitleStyle = new GUIStyle(GlobalStyles.settingsHeaderText);
                TitleStyle.margin = new RectOffset(0, 0, 0, 0);

                var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
                var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");

                bgStyle = new GUIStyle();
                bgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                bgStyle.border = new RectOffset(2, 2, 2, 2);

                subHeadingStyle = new GUIStyle(EditorStyles.boldLabel);
                subHeadingStyle.alignment = TextAnchor.UpperLeft;
                subHeadingStyle.margin = new RectOffset(0, 0, 0, 0);

                starEmptyTex = GlobalStyles.LoadImageResource("star_empty");
                starFullTex = GlobalStyles.LoadImageResource("star_filled");

                starSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);

                feedbackLabelStyle = new GUIStyle(EditorStyles.label);
                feedbackLabelStyle.richText = true;
                feedbackLabelStyle.wordWrap = true;
                feedbackLabelStyle.margin = new RectOffset(0, 0, 0, 0);
                feedbackLabelStyle.alignment = TextAnchor.UpperLeft;
                feedbackWindowStyle = new GUIStyle(EditorStyles.textArea);
                feedbackWindowStyle.richText = true;

                EmailStyle = new GUIStyle(EditorStyles.textField);
                EmailStyle.margin = new RectOffset(0, 0, 0, 0);
                EmailStyle.alignment = TextAnchor.UpperLeft;

                SubmitStyle = new GUIStyle(GUI.skin.button);
                SubmitStyle.margin = new RectOffset(0, 0, 0, 0);
            }
        }


        public override string title {
            get {
                return "Feedback";
            }
        }

        private int rating = 0;
        private bool hasRated = false;
        private bool hasSubmitted = false;
        private string feedbackText = "Any comments or concerns? Let them be heard." +
        "\n(This will also report your portalkit settings)";
        private string feedbackText2 = "We'd hate to leave anyone with anything less than a five-star \n" +
                                       "experience. If you leave feedback in the box below, we will use \n" +
                                       "it to improve the asset in future updates.\n" +
                                       "(This will also report your portalkit settings)";
        private AnimBool fadeRatingPopup;
        private AnimBool hasRatedFadePopup;
        private AnimBool hasNotRatedFadePopup;
        private AnimBool fadeSupportPopup;
        public FeedbackPanel(SettingsWindow window)
        {
            fadeRatingPopup = new AnimBool(rating == maxRating && hasRated, window.Repaint);
            hasRatedFadePopup = new AnimBool(hasRated, window.Repaint);
            hasNotRatedFadePopup = new AnimBool(!hasRated, window.Repaint);
            fadeSupportPopup = new AnimBool(rating < supportRating && hasRated, window.Repaint);
        }


        public override void OnGUI(Rect position)
        {
            position = ApplySettingsPadding(position);
            Styles.Init();
            GUILayout.BeginArea(position);
            {
                GUILayout.Label(title, Styles.TitleStyle);

                EditorGUIUtility.labelWidth = 250;

                position.x = 0;
                position.y = 0;

                EditorGUILayout.Space();
            }
            GUILayout.EndArea();



            position = ApplySettingsPadding(position);

            GUILayout.BeginArea(position);
            {
                hasNotRatedFadePopup.target = !hasRated;

                GUILayout.Space(30);
                //Star Bar
                if (EditorGUILayout.BeginFadeGroup(hasNotRatedFadePopup.faded))
                {
                    //GUILayout.FlexibleSpace();
                }EditorGUILayout.EndFadeGroup();

                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(Content.ratingPrompt, Styles.subHeadingStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {

                    //Rating selector is invisible
                    GlobalStyles.StartColorArea(Color.clear);
                    //Push ratings bar to middle
                    //if (EditorGUILayout.BeginFadeGroup(hasNotRatedFadePopup.faded))
                    {
                        GUILayout.FlexibleSpace();
                        //Ensure maximum middle-ness
                        EditorGUILayout.IntField(0, GUILayout.MaxWidth(EditorGUIUtility.fieldWidth));
                    }
                    //EditorGUILayout.EndFadeGroup();
                    //Rating selector
                    EditorGUI.BeginChangeCheck();
                    rating = EditorGUILayout.IntSlider(rating, 1, maxRating, GUILayout.MaxWidth(starSizeFull + EditorGUIUtility.fieldWidth));
                    if (EditorGUI.EndChangeCheck())
                        hasRated = true;

                    GlobalStyles.EndColorArea();



                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    Rect filledStarRect = new Rect(lastRect.x, lastRect.y - (starSize / 8f),
                        starSizeFull / (maxRating / (float)rating), starSize);
                    Rect emptyStarRect = new Rect(filledStarRect.x + filledStarRect.width, filledStarRect.y,
                        starSizeFull - filledStarRect.width, starSize);
                    //Draw full stars
                    if (!hasRated)
                    {
                        GUI.DrawTextureWithTexCoords(
                            new Rect(filledStarRect.x, filledStarRect.y, starSizeFull, starSize),
                            Styles.starEmptyTex, new Rect(0, 0, maxRating, 1));
                    }
                    else if (rating < maxRating)
                    {
                        GlobalStyles.StartColorArea(new Color(0.2755f, 0.2755f, 0.2755f, 1));
                        GUI.DrawTextureWithTexCoords(filledStarRect, Styles.starFullTex, new Rect(0, 0, rating, 1));
                        GlobalStyles.EndColorArea();
                    }
                    else
                    {
                        GlobalStyles.StartColorArea(new Color(1, 0.61f, 0, 1));
                        GUI.DrawTextureWithTexCoords(filledStarRect, Styles.starFullTex, new Rect(0, 0, rating, 1));
                        GlobalStyles.EndColorArea();
                    }

                    if (hasRated)
                    {
                        //Draw empty stars
                        GlobalStyles.StartColorArea(EditorGUIUtility.isProSkin
                            ? Color.white
                            : new Color(0.2755f, 0.2755f, 0.2755f, 1));
                        GUI.DrawTextureWithTexCoords(emptyStarRect, Styles.starEmptyTex,
                            new Rect(0, 0, maxRating - rating, 1));
                        GlobalStyles.EndColorArea();
                    }
                }
                //if (EditorGUILayout.BeginFadeGroup(hasNotRatedFadePopup.faded)) {
                    GUILayout.FlexibleSpace();
                //}
                //EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndHorizontal();

                hasRatedFadePopup.target = hasRated;
                if (EditorGUILayout.BeginFadeGroup(hasRatedFadePopup.faded))
                {

                    GUILayout.Space(5);

                    //Show the rating dialog if rating is full
                    fadeRatingPopup.target = rating == maxRating;
                    //Show the support dialog if rating is less than the support level
                    fadeSupportPopup.target = rating < supportRating && hasRated;

                    //Rating Dialog
                    if (EditorGUILayout.BeginFadeGroup(fadeRatingPopup.faded))
                    {
                        EditorGUILayout.LabelField(Content.ReviewText, Styles.feedbackLabelStyle);
                        GlobalStyles.LayoutExternalLink(Content.RatingLinkText, Content.RatingLink);
                    }
                    EditorGUILayout.EndFadeGroup();



                    //Support dialog
                    if (EditorGUILayout.BeginFadeGroup(fadeSupportPopup.faded))
                    {
                        EditorGUILayout.LabelField(Content.SupportText, Styles.feedbackLabelStyle);
                        GlobalStyles.LayoutExternalLink(Content.SupportLinkText, Content.s1WebsiteLink);
                    }
                    EditorGUILayout.EndFadeGroup();

                    //Feedback Email
                    //Feedback input box


                    GUI.enabled = !hasSubmitted;
                    {
                        float minHeight = EditorGUIUtility.singleLineHeight * 3;
                        //Expand the textbox if required
                        if ((rating == maxRating) && !String.IsNullOrEmpty(feedbackText))
                        {
                            int count = feedbackText.Split('\n').Length;
                            if (count > 3)
                                minHeight = count * EditorGUIUtility.singleLineHeight;
                        }else if ((rating < maxRating) && !String.IsNullOrEmpty(feedbackText2)) {
                            int count = feedbackText2.Split('\n').Length;
                            if (count > 3)
                                minHeight = count * EditorGUIUtility.singleLineHeight;
                        }



                        if (!hasSubmitted)
                        {
                            if (rating < maxRating && hasRated) {
                                feedbackText2 = EditorGUILayout.TextArea(feedbackText2, Styles.feedbackWindowStyle,
                                    GUILayout.MinHeight(minHeight));
                            }
                            else {
                                feedbackText = EditorGUILayout.TextArea(feedbackText, Styles.feedbackWindowStyle,
                                    GUILayout.MinHeight(minHeight));
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(feedbackText, Styles.feedbackWindowStyle,
                                GUILayout.MinHeight(minHeight));
                        }



                        if (!hasSubmitted && GUILayout.Button(Content.SubmitFeedback))
                        {
                            if (rating < maxRating && hasRated) {
                                feedbackText = feedbackText2;
                            }
                            
                            hasSubmitted = true;
                            StringBuilder ratingAppendBuilder = new StringBuilder();
                            for (int i = 0; i < maxRating; i++)
                            {
                                if (i < rating)
                                    ratingAppendBuilder.Append('★');
                                else
                                    ratingAppendBuilder.Append('☆');

                            }
                           
                            ratingAppendBuilder.Append('|').Append(String.Format(
                                "{0}|{1}.{2}.{3} ", Application.unityVersion,
                                GlobalPortalSettings.MAJOR_VERSION, GlobalPortalSettings.MINOR_VERSION,
                                GlobalPortalSettings.PATCH_VERSION));
                            ratingAppendBuilder.Append('|').Append(SKSGlobalRenderSettings.Instance.ToString());
                            ratingAppendBuilder.Append('|').Append(GlobalPortalSettings.Instance.ToString());
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.deviceUniqueIdentifier));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.graphicsDeviceName));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.operatingSystem));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.processorType));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.systemMemorySize));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SystemInfo.graphicsMemorySize));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", Application.platform));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", EditorUserBuildSettings.activeBuildTarget));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", PlayerSettings.stereoRenderingPath));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", SetupUtility.projectMode.ToString()));
                            ratingAppendBuilder.Append('|')
                                .Append(String.Format("{0}", Application.systemLanguage));
                            

                            String feedbackNext = feedbackText;
                            feedbackNext = feedbackNext.Replace(Environment.NewLine, " ");
                            feedbackNext = feedbackNext.Replace("\n", " ");
                            feedbackNext = feedbackNext.Replace("|", " ");
                            feedbackNext = feedbackNext + '|' + ratingAppendBuilder;
                            //Queue and send IRC message (wow such technology (I swear there is a reason we're doing it this way))
                            TcpClient socket = new TcpClient();
                            Int32 port = 7000;
                            string server = "irc.rizon.net";
                            String chan = "#SKSPortalKitFeedback";
                            socket.Connect(server, port);
                            StreamReader input = new System.IO.StreamReader(socket.GetStream());
                            StreamWriter output = new System.IO.StreamWriter(socket.GetStream());
                            String nick = SystemInfo.deviceName;
                            output.Write(
                                "USER " + nick + " 0 * :" + "ChunkBuster" + "\r\n" +
                                "NICK " + nick + "\r\n"
                            );
                            output.Flush();
                            Thread sendMsgThread = new Thread(() => {
                                bool joined = false;
                                while (true)
                                {
                                    try
                                    {
                                        String buf = input.ReadLine();
                                        //Debug.Log(buf);
                                        if (buf == null)
                                        {
                                            //Debug.Log("Null");
                                            return;
                                        }

                                        //Send pong reply to any ping messages
                                        if (buf.StartsWith("PING "))
                                        {
                                            output.Write(buf.Replace("PING", "PONG") + "\r\n");
                                            output.Flush();
                                        }
                                        if (buf[0] != ':') continue;

                                        /* IRC commands come in one of these formats:
                                         * :NICK!USER@HOST COMMAND ARGS ... :DATA\r\n
                                         * :SERVER COMAND ARGS ... :DATA\r\n
                                         */

                                        //After server sends 001 command, we can set mode to bot and join a channel
                    if (buf.Split(' ')[1] == "001")
                                        {
                                            output.Write(
                                                "MODE " + nick + " +B\r\n" +
                                                "JOIN " + chan + "\r\n"
                                            );
                                            output.Flush();
                                            joined = true;
                                            continue;
                                        }
                                        if (buf.Contains("End of /NAMES list"))
                                        {
                                            String[] outputText = feedbackNext.Split('\n');
                                            foreach (string s in outputText)
                                            {
                                                output.Write("PRIVMSG " + chan + " :" + s + "\r\n");
                                                output.Flush();
                                                Thread.Sleep(200);
                                            }

                                            socket.Close();
                                            return;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //If this doesn't function perfectly then just dispose of it, not worth possibly causing issues with clients over network issues
                                        return;
                                    }
                                }
                            });
                            sendMsgThread.Start();
                        }
                        if (hasSubmitted)
                            GUILayout.Button(Content.SubmittedFeedback);
                    }
                    GUI.enabled = true;

                    GUILayout.Space(20);
                    //Content submission
                    GUILayout.Label(Content.GalleryText, Styles.subHeadingStyle);
                    GUILayout.Label(Content.GalleryResponseText, Styles.feedbackLabelStyle);
                    GlobalStyles.LayoutExternalLink(Content.GalleryLinkText, Content.GalleryLink);
                    GUILayout.FlexibleSpace();

                    
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndFadeGroup();
                GUILayout.Label(Content.supportLabel, Styles.subHeadingStyle);
                GlobalStyles.LayoutExternalLink(Content.s1Text, Content.s1WebsiteLink);
                GlobalStyles.LayoutExternalLink(Content.s2Text, Content.s2WebsiteLink);
            }
            GUILayout.EndArea();
        }
    }
}
