using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Facebook
{
    sealed class AndroidFacebook : MonoBehaviour, IFacebook
    {
        public const int BrowserDialogMode = 0;

        private const string AccessTokenKey = "access_token";
        private const string AndroidJavaFacebookClass = "com.facebook.unity.FB";
        private const string CallbackIdKey = "callback_id";

        private string userId;
        private string accessToken;
        private bool isLoggedIn;

        private int nextApiDelegateId = 0;
        private Dictionary<string, APIDelegate> apiDelegates = new Dictionary<string, APIDelegate>();
        private List<Facebook.AuthChangeDelegate> authChangeDelegates = new List<Facebook.AuthChangeDelegate>();

        #region IFacebook
        public string UserId { get { return userId; } }
        public string AccessToken { get { return accessToken; } }
        public bool IsLoggedIn { get { return isLoggedIn; } }
        public int DialogMode { get { return BrowserDialogMode; } set { } }
        #endregion

        #region FBJava

#if UNITY_ANDROID
        private AndroidJavaClass fbJava;
        private AndroidJavaClass FB
        {
            get
            {
                if (fbJava == null)
                {
                    fbJava = new AndroidJavaClass(AndroidJavaFacebookClass);

                    if (fbJava == null)
                    {
                        throw new MissingReferenceException(string.Format("AndroidFacebook failed to load {0} class", AndroidJavaFacebookClass));
                    }
                }
                return fbJava;
            }
        }
#endif
        private void CallFB(string method, string args)
        {
#if UNITY_ANDROID
            FB.CallStatic(method, args);
#else
            FbDebug.Error("Using Android when not on an Android build!  Doesn't Work!");
#endif
        }

        #endregion

        #region FBAndroid

        void Awake()
        {
            DontDestroyOnLoad(this);

            accessToken = "";
            isLoggedIn = false;
            userId = "";
            nextApiDelegateId = 0;
        }

        private bool IsErrorResponse(string response)
        {
            //var res = MiniJSON.Json.Deserialize(response);
            return false;
        }

        private InitDelegate onInitComplete = null;
        public void Init(
            InitDelegate onInitComplete,
            string appId,
            bool cookie = false,
            bool logging = true,
            bool status = true,
            bool xfbml = false,
            string channelUrl = "",
            string authResponse = null,
            bool frictionlessRequests = false,
            HideUnityDelegate hideUnityDelegate = null)
        {
            if (appId == null || appId == "")
            {
                throw new ArgumentException("appId cannot be null or empty!");
            }

            FbDebug.Log("start android init");

            var parameters = new Dictionary<string, object>();

            if (appId != "")
            {
                parameters.Add("appId", appId);
            }
            if (cookie != false)
            {
                parameters.Add("cookie", true);
            }
            if (logging != true)
            {
                parameters.Add("logging", false);
            }
            if (status != true)
            {
                parameters.Add("status", false);
            }
            if (xfbml != false)
            {
                parameters.Add("xfbml", true);
            }
            if (channelUrl != "")
            {
                parameters.Add("channelUrl", channelUrl);
            }
            if (authResponse != null)
            {
                parameters.Add("authResponse", authResponse);
            }
            if (frictionlessRequests != false)
            {
                parameters.Add("frictionlessRequests", true);
            }

            var paramJson = MiniJSON.Json.Serialize(parameters);
            this.onInitComplete = onInitComplete;

            this.CallFB("Init", paramJson.ToString());
        }

        public void OnInitComplete(string message)
        {
            if (this.onInitComplete != null)
            {
                this.onInitComplete();
            }
            OnLoginComplete(message);
        }

        public void Login(string scope = "", Facebook.AuthChangeDelegate callback = null)
        {
            FbDebug.Log("Login(" + scope + ")");
            var parameters = new Dictionary<string, object>();
            parameters.Add("scope", scope);
            var paramJson = MiniJSON.Json.Serialize(parameters);
            authChangeDelegates.Add(callback);
            this.CallFB("Login", paramJson);
        }

        public void OnLoginComplete(string message)
        {
            var parameters = (Dictionary<string, object>)MiniJSON.Json.Deserialize(message);

            if (parameters.ContainsKey("user_id"))
            {
                isLoggedIn = true;
                userId = (string)parameters["user_id"];
                accessToken = (string)parameters["access_token"];
            }

            foreach (AuthChangeDelegate callback in authChangeDelegates)
            {
                callback();
            }
            authChangeDelegates.Clear();
        }

        public void Logout()
        {
            this.CallFB("Logout", "");
        }

        public void OnLogoutComplete(string message)
        {
            FbDebug.Log("OnLogoutComplete");
            isLoggedIn = false;
            userId = "";
            accessToken = "";
        }

        public void AppRequest(
            string message,
            string[] to = null,
            string filters = "",
            string[] excludeIds = null,
            int? maxRecipients = null,
            string data = "",
            string title = "",
            Facebook.APIDelegate callback = null)
        {
            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            // Marshal all the above into the thing

            paramsDict["message"] = message;

            if (callback != null)
            {
                paramsDict["callback_id"] = AddApiDelegate(callback);
            }

            if (to != null)
            {
                paramsDict["to"] = string.Join(",", to);
            }

            if (filters != "")
            {
                paramsDict["filters"] = filters;
            }

            if (maxRecipients != null)
            {
                paramsDict["max_recipients"] = maxRecipients.Value;
            }

            if (data != "")
            {
                paramsDict["data"] = data;
            }

            if (title != "")
            {
                paramsDict["title"] = title;
            }

            CallFB("AppRequest", MiniJSON.Json.Serialize(paramsDict));
        }

        public void OnAppRequestsComplete(string message)
        {
            var rawResult = (Dictionary<string, object>)MiniJSON.Json.Deserialize(message);
            if (rawResult.ContainsKey(CallbackIdKey))
            {
                var result = new Dictionary<string, object>();
                var callbackId = (string)rawResult[CallbackIdKey];
                rawResult.Remove(CallbackIdKey);
                if (rawResult.Count > 0)
                {
                    List<string> to = new List<string> (rawResult.Count - 1);
                    foreach (string key in rawResult.Keys)
                    {
                        if (!key.StartsWith ("to"))
                        {
                            result [key] = rawResult [key];
                            continue;
                        }
                        to.Add ((string)rawResult [key]);
                    }
                    result.Add ("to", to);
                    rawResult.Clear ();
                    CallApiDelegate (callbackId, MiniJSON.Json.Serialize (result));
                } 
                else
                {
                    //if we make it here java returned a callback message with only an id
                    //this isnt supposed to happen
                    result.Add ("error", "Malformed request response.  Please file a bug with facebook here: https://developers.facebook.com/bugs");
                    CallApiDelegate (callbackId, MiniJSON.Json.Serialize (result));
                }
            }
        }

        public void FeedRequest(
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null)
        {
            Dictionary<string, object> paramsDict = new Dictionary<string, object>();
            // Marshal all the above into the thing

            if (toId != "")
            {
                paramsDict.Add("to", toId);
            }

            if (link != "")
            {
                paramsDict.Add("link", link);
            }

            if (linkName != "")
            {
                paramsDict.Add("name", linkName);
            }

            if (linkCaption != "")
            {
                paramsDict.Add("caption", linkCaption);
            }

            if (linkDescription != "")
            {
                paramsDict.Add("description", linkDescription);
            }

            if (picture != "")
            {
                paramsDict.Add("picture", picture);
            }

            if (mediaSource != "")
            {
                paramsDict.Add("source", mediaSource);
            }

            if (actionName != "" && actionLink != "")
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("name", actionName);
                dict.Add("link", actionLink);

                paramsDict.Add("actions", new[] { dict });
            }

            if (reference != "")
            {
                paramsDict.Add("ref", reference);
            }

            if (properties != null)
            {
                Dictionary<string, object> newObj = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string[]> pair in properties)
                {
                    if (pair.Value.Length < 1)
                        continue;

                    if (pair.Value.Length == 1)
                    {
                        // String-string
                        newObj.Add(pair.Key, pair.Value[0]);
                    }
                    else
                    {
                        // String-Object with two parameters
                        Dictionary<string, object> innerObj = new Dictionary<string, object>();

                        innerObj.Add("text", pair.Value[0]);
                        innerObj.Add("href", pair.Value[1]);

                        newObj.Add(pair.Key, innerObj);
                    }
                }
                paramsDict.Add("properties", newObj);
            }

            CallFB("FeedRequest", MiniJSON.Json.Serialize(paramsDict));
        }


        public void OnFeedRequestComplete(string message)
        {
            FbDebug.Log("OnFeedRequestComplete: " + message);
        }

        public void Pay(
            string product,
            string action = "purchaseitem",
            int quantity = 1,
            int? quantityMin = null,
            int? quantityMax = null,
            string requestId = null,
            string pricepointId = null,
            string testCurrency = null,
            Facebook.APIDelegate callback = null)
        {
            throw new PlatformNotSupportedException("There is no Facebook Pay Dialog on Android");
        }

        public void API(
            string query,
            HttpMethod method,
            Facebook.APIDelegate callback = null,
            Dictionary<string, string> formData = null,
            Facebook.ErrorDelegate errorCallback = null)
        {

            FbDebug.Log("Calling API");

            if (!query.StartsWith("/"))
            {
                query = "/" + query;
            }
            string url = IntegratedPluginCanvasLocation.GraphUrl + query;

            // Copy the formData by value so it's not vulnerable to scene changes and object deletions
            Dictionary<string, string> inputFormData = (formData != null) ? CopyByValue(formData) : new Dictionary<string, string>(1);
            if (!inputFormData.ContainsKey(AccessTokenKey))
            {
                inputFormData[AccessTokenKey] = AccessToken;
            }

            StartCoroutine(graphAPI(url, method, callback, inputFormData, errorCallback));
        }

        public void GetAuthResponse(Facebook.AuthChangeDelegate callback = null)
        {
            authChangeDelegates.Add(callback);
        }

        public void PublishInstall(string appId, Facebook.APIDelegate callback = null)
        {
          var parameters = new Dictionary<string, string>(2);
          parameters["app_id"] = appId;
          if(callback != null) {
            parameters["callback_id"] = AddApiDelegate(callback);
          }
        	CallFB("PublishInstall", MiniJSON.Json.Serialize(parameters));
        }
        
        public void OnPublishInstallComplete(string message) {
          var response = (Dictionary<string, object>) MiniJSON.Json.Deserialize(message);
          if(response.ContainsKey("callback_id")) {
            CallApiDelegate((string) response["callback_id"], "");
          }
        }

        #endregion

        #region Helper Functions

        private string AddApiDelegate(Facebook.APIDelegate callback)
        {
            nextApiDelegateId++;

            apiDelegates.Add(nextApiDelegateId.ToString(), callback);

            return nextApiDelegateId.ToString();
        }

        private void CallApiDelegate(string callbackId, string result)
        {
            Facebook.APIDelegate callback = null;

            if (apiDelegates.TryGetValue(callbackId, out callback))
            {
                FbDebug.Log("Calling callback with value: " + result);
                callback(result);
                apiDelegates.Remove(callbackId);
            }
            else
            {
                FbDebug.Warn("Could not find requested callback. " + result);
            }
        }

        private IEnumerator graphAPI(
            string url,
            HttpMethod method,
            Facebook.APIDelegate callback = null,
            Dictionary<string, string> formData = null,
            Facebook.ErrorDelegate errorCallback = null)
        {
            WWW www;
            if (method == HttpMethod.GET)
            {
                string query = (url.Contains("?")) ? "&" : "?";
                if (formData != null)
                {
                    foreach (KeyValuePair<string, string> pair in formData)
                    {
                        query += string.Format("{0}={1}&", Uri.EscapeDataString(pair.Key), Uri.EscapeDataString(pair.Value));
                    }
                }
                www = new WWW(url + query);
            }
            else //POST
            {
                WWWForm query = new WWWForm();
                foreach (KeyValuePair<string, string> pair in formData)
                {
                    query.AddField(pair.Key, pair.Value);
                }

                www = new WWW(url, query);
            }

            FbDebug.Log("Fetching from " + www.url);
            yield return www;

            if (www.error != null)
            {
                if (errorCallback != null)
                {
                    errorCallback(www.error);
                }
                else
                {
                    FbDebug.Error("Web Error: " + www.error);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(www.text);
                }
                else
                {
                    FbDebug.Log(www.text);
                }
            }
            www.Dispose();
            formData.Clear();
        }

        private Dictionary<string, string> CopyByValue(Dictionary<string, string> data)
        {
            var newData = new Dictionary<string, string>(data.Count);
            foreach (KeyValuePair<string, string> kvp in data)
            {
                newData[kvp.Key] = String.Copy(kvp.Value);
            }
            return newData;
        }

        #endregion

        #region MonoBehavior

        void Start()
        {
#if DEBUG
            AndroidJNIHelper.debug = true;
#endif
        }
        #endregion
    }
}
   
