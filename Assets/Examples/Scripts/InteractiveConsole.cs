using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InteractiveConsole : MonoBehaviour
{
    private string status = "Ready";
    private bool isInit = false;

    public string FriendSelectorTitle = "";
    public string FriendSelectorMessage = "Derp";
    public string FriendSelectorFilters = "[\"all\",\"app_users\",\"app_non_users\"]";
    public string FriendSelectorData = "{}";
    public string FriendSelectorExcludeIds = "";
    public string FriendSelectorMax = "";

    public string DirectRequestTitle = "";
    public string DirectRequestMessage = "Herp";
    private string DirectRequestTo = "";

    public string FeedToId = "";
    public string FeedLink = "";
    public string FeedLinkName = "";
    public string FeedLinkCaption = "";
    public string FeedLinkDescription = "";
    public string FeedPicture = "";
    public string FeedMediaSource = "";
    public string FeedActionName = "";
    public string FeedActionLink = "";
    public string FeedReference = "";
    public bool IncludeFeedProperties = false;
    private Dictionary<string, string[]> FeedProperties = new Dictionary<string, string[]>();

    public string PayProduct = "";

    public string ApiQuery = "";

    private string lastResponse = "";
    public GUIStyle textStyle = new GUIStyle();

    private Vector2 scrollPosition = Vector2.zero;
#if UNITY_IOS || UNITY_ANDROID
    int buttonHeight = 60;
    int mainWindowWidth = 610;
    int mainWindowFullWidth = 640;
#else
    int buttonHeight = 24;
    int mainWindowWidth = 500;
    int mainWindowFullWidth = 530;
#endif

    void Awake()
    {
        textStyle.alignment = TextAnchor.UpperLeft;
        textStyle.wordWrap = true;
        textStyle.padding = new RectOffset(10,10,10,10);
        textStyle.stretchHeight = true;
        textStyle.stretchWidth = false;

        FeedProperties.Add("key1", new [] {"valueString1"});
        FeedProperties.Add("key2", new [] {"valueString2", "http://www.facebook.com"});

#if UNITY_WEBPLAYER
        FBScreen.SetResolution(960, 1000, false);
#endif
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Box("Status: " + status, GUILayout.MinWidth(mainWindowWidth));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MinWidth(mainWindowFullWidth));
        GUILayout.BeginVertical();
        GUI.enabled = !isInit;
        if (GUILayout.Button("FB.Init", GUILayout.MinHeight(buttonHeight)))
        {
            FB.Init(OnInitComplete, OnHideUnity);
            status = "FB.Init() called with " + FB.AppId;
        }

        GUI.enabled = isInit && !FB.IsLoggedIn;
        if (GUILayout.Button("Login", GUILayout.MinHeight(buttonHeight)))
        {
            FB.Login();
            status = "Login called";
        }

#if UNITY_IOS || UNITY_ANDROID
        GUI.enabled = isInit;
        if(GUILayout.Button ("Publish Install", GUILayout.MinHeight(buttonHeight)))
        {
            FB.PublishInstall(PublishComplete);
            status = "Install Published";
        }
#endif

        GUI.enabled = FB.IsLoggedIn;
        GUILayout.Space(10);
        LabelAndTextField("Title (optional): ", ref FriendSelectorTitle);
        LabelAndTextField("Message: ", ref FriendSelectorMessage);
        LabelAndTextField("Exclude Ids (optional): ", ref FriendSelectorExcludeIds);
        LabelAndTextField("Filters (optional): ", ref FriendSelectorFilters);
        LabelAndTextField("Max Recipients (optional): ", ref FriendSelectorMax);
        LabelAndTextField("Data (optional): ", ref FriendSelectorData);
        if (GUILayout.Button("Open Friend Selector", GUILayout.MinHeight(buttonHeight)))
        {
            try
            {
                int? maxRecipients = null;
                if(FriendSelectorMax != "") 
                {
                    try
                    {
                        maxRecipients = Int32.Parse(FriendSelectorMax);
                    }
                    catch(Exception e)
                    {
                        status = e.Message;
                    }
                }

                string[] excludeIds = (FriendSelectorExcludeIds == "") ? null : FriendSelectorExcludeIds.Split(',');

                FB.AppRequest(
                    message: FriendSelectorMessage,
                    filters: FriendSelectorFilters,
                    excludeIds: excludeIds,
                    maxRecipients: maxRecipients,
                    data: FriendSelectorData,
                    title: FriendSelectorTitle,
                    callback: Callback
                );
                status = "Friend Selector called";
            }
            catch (Exception e)
            {
                status = e.Message;
            }
        }
        GUILayout.Space(10);
        LabelAndTextField("Title (optional): ", ref DirectRequestTitle);
        LabelAndTextField("Message: ", ref DirectRequestMessage);
        LabelAndTextField("To Comma Ids: ", ref DirectRequestTo);
        if (GUILayout.Button("Open Direct Request", GUILayout.MinHeight(buttonHeight)))
        {
            try
            {
                if (DirectRequestTo == "")
                {
                    throw new ArgumentException("\"To Comma Ids\" must be specificed", "to");
                }
                FB.AppRequest(
                    message: DirectRequestMessage,
                    to: DirectRequestTo.Split(','),
                    title: DirectRequestTitle,
                    callback: Callback
                );
                status = "Direct Request called";
            }
            catch(Exception e)
            {
                status = e.Message;
            }
        }
        GUILayout.Space(10);
        LabelAndTextField("To Id (optional): ", ref FeedToId);
        LabelAndTextField("Link (optional): ", ref FeedLink);
        LabelAndTextField("Link Name (optional): ", ref FeedLinkName);
        LabelAndTextField("Link Desc (optional): ", ref FeedLinkDescription);
        LabelAndTextField("Link Caption (optional): ", ref FeedLinkCaption);
        LabelAndTextField("Picture (optional): ", ref FeedPicture);
        LabelAndTextField("Media Source (optional): ", ref FeedMediaSource);
        LabelAndTextField("Action Name (optional): ", ref FeedActionName);
        LabelAndTextField("Action Link (optional): ", ref FeedActionLink);
        LabelAndTextField("Reference (optional): ", ref FeedReference);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Properties (optional)", GUILayout.Width(150));
        IncludeFeedProperties = GUILayout.Toggle(IncludeFeedProperties, "Include");
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Open Feed Dialog", GUILayout.MinHeight(buttonHeight)))
        {
            Dictionary<string,string[]> feedProperties = null;
            if (IncludeFeedProperties)
            {
                feedProperties = FeedProperties;
            }
            try
            {
                FB.Feed(
                    toId: FeedToId,
                    link: FeedLink,
                    linkName: FeedLinkName,
                    linkCaption: FeedLinkCaption,
                    linkDescription: FeedLinkDescription,
                    picture: FeedPicture,
                    mediaSource: FeedMediaSource,
                    actionName: FeedActionName,
                    actionLink: FeedActionLink,
                    reference: FeedReference,
                    properties: feedProperties
                );
                status = "Feed dialog called";
            }
            catch (Exception e)
            {
                status = e.Message;
            }
        }
        GUILayout.Space(10);

#if UNITY_WEBPLAYER
        LabelAndTextField("Product: ", ref PayProduct);
        if (GUILayout.Button("Call Pay", GUILayout.MinHeight(buttonHeight)))
        {
            FB.Canvas.Pay(PayProduct);
        }
        GUILayout.Space(10);
#endif

        LabelAndTextField("API: ", ref ApiQuery);
        if (GUILayout.Button("Call API", GUILayout.MinHeight(buttonHeight)))
        {
            FB.API(ApiQuery, Facebook.HttpMethod.GET, Callback, null);
        }
        GUI.enabled = isInit && FB.IsLoggedIn;
        GUILayout.Space(10);
        if (GUILayout.Button("Logout", GUILayout.MinHeight(buttonHeight)))
        {
            FB.Logout();
            status = "Logout called";
        }

        GUI.enabled = true;

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        
#if !UNITY_IOS && !UNITY_ANDROID
        GUILayout.EndVertical();
#endif
        GUILayout.TextArea(
            string.Format(
                " AppId: {0} \n Facebook Dll: {1} \n UserId: {2}\n IsLoggedIn: {3}\n AccessToken: {4}\n\n {5}", 
                FB.AppId,
                (isInit) ? "Loaded Successfully" : "Not Loaded",
                FB.UserId, 
                FB.IsLoggedIn, 
                FB.AccessToken,
                lastResponse
#if UNITY_IOS || UNITY_ANDROID
             ), textStyle, GUILayout.Width(640), GUILayout.Height(85));
#else
             ), textStyle, GUILayout.Width(450), GUILayout.Height(Screen.height));
#endif
        
#if UNITY_IOS || UNITY_ANDROID
        GUILayout.EndVertical();
#endif
        GUILayout.EndHorizontal();
    }

    void Callback(string response)
    {
        lastResponse = "Success Response:\n" + response;
    }

    private void LabelAndTextField(string label, ref string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(150));
        text = GUILayout.TextField(text, GUILayout.MinWidth(300));
        GUILayout.EndHorizontal();
    }

    private void OnInitComplete()
    {
        FbDebug.Log("Init completed");
        isInit = true;
    }

    private void OnHideUnity(bool isGameShown)
    {
        FbDebug.Log("Is game showing? " + isGameShown);
    }

    private void PublishComplete(string response)
    {
        FbDebug.Info ("publish response: " + response);
    }

    private void PublishError(string error)
    {
        FbDebug.Info ("publish error: " + error);
    }
}
