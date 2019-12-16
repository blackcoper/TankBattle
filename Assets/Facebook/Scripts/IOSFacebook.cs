using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NativeDialogModes;

namespace Facebook
{
    class IOSFacebook : MonoBehaviour, IFacebook
    {
#if UNITY_IOS
        [DllImport ("__Internal")] private static extern void iosInit(bool cookie, bool logging, bool status, bool frictionlessRequests);
        [DllImport ("__Internal")] private static extern void iosLogin(string scope);
        [DllImport ("__Internal")] private static extern void iosLogout();

        [DllImport ("__Internal")] private static extern void iosSetShareDialogMode(int mode);

        [DllImport ("__Internal")] 
        private static extern void iosFeedRequest(
            int requestId,
            string toId, 
            string link, 
            string linkName, 
            string linkCaption, 
            string linkDescription, 
            string picture, 
            string mediaSource, 
            string actionName, 
            string actionLink, 
            string reference);
        
        [DllImport ("__Internal")] 
        private static extern void iosAppRequest(
            int requestId,
            string message, 
            string[] to = null,
            int toLength = 0,
            string filters = "", 
            string[] excludeIds = null,
            int excludeIdsLength = 0,
            bool hasMaxRecipients = false,
            int maxRecipients = 0, 
            string data = "", 
            string title = "");
        
        [DllImport ("__Internal")] 
        private static extern void iosCallFbApi(
            int requestId,
            string query,
            string method,
            string[] formDataKeys = null,
            string[] formDataVals = null,
            int formDataLen = 0);
        

        //neko, insights stuff
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsFlush();

        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsLogConversionPixel(
            string pixelID, 
            double value);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsLogPurchase(
            double purchaseAmount,
            string currency,
            int numParams,
            string[] paramKeys,
            string[] paramVals);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsSetAppVersion(string version);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsSetFlushBehavior(int behavior);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBSettingsPublishInstall(int requestId, string appId);
#else
        void iosInit(bool cookie, bool logging, bool status, bool frictionlessRequests) {}
        void iosLogin(string scope) {}
        void iosLogout() {}

        void iosSetShareDialogMode(int mode) {}
        
        void iosFeedRequest(
            int requestId,
            string toId, 
            string link, 
            string linkName, 
            string linkCaption, 
            string linkDescription, 
            string picture, 
            string mediaSource, 
            string actionName, 
            string actionLink, 
            string reference) {}
        
        void iosAppRequest(
            int requestId,
            string message, 
            string[] to = null,
            int toLength = 0,
            string filters = "", 
            string[] excludeIds = null,
            int excludeIdsLength = 0,
            bool hasMaxRecipients = false,
            int maxRecipients = 0, 
            string data = "", 
            string title = "") {}
        
        void iosCallFbApi(
            int requestId,
            string query,
            string method,
            string[] formDataKeys = null,
            string[] formDataVals = null,
            int formDataLen = 0) {}
        
        void iosFBInsightsFlush() {}

        void iosFBInsightsLogConversionPixel(
            string pixelID, 
            double value) {}
        
        void iosFBInsightsLogPurchase(
            double purchaseAmount,
            string currency,
            int numParams,
            string[] paramKeys,
            string[] paramVals) {}
        
        void iosFBInsightsSetAppVersion(string version) {}
        
        void iosFBInsightsSetFlushBehavior(int behavior) {}
        
        void iosFBSettingsPublishInstall(int requestId, string appId) {}
#endif
        
        private class NativeDict
        {
            public NativeDict()
            {
                numEntries = 0;
                keys = null;
                vals = null;
            }
            
            public int numEntries;
            public string[] keys;
            public string[] vals;
        };
        
        public enum FBInsightsFlushBehavior 
        {
            FBInsightsFlushBehaviorAuto,
            FBInsightsFlushBehaviorExplicitOnly,
        };
            
        private bool isLoggedIn;
        private string userId;
        private string accessToken;
        private int dialogMode = (int)NativeDialogModes.eModes.FAST_APP_SWITCH_SHARE_DIALOG;
        private string appVersion;
        private FBInsightsFlushBehavior flushBehavior;

        private InitDelegate externalInitDelegate;

        private List<Facebook.AuthChangeDelegate> authResponseCallbacks;

        private int currRequestId = 0;
        private Dictionary<int, Facebook.APIDelegate> OutstandingRequestCallbacks;

        public bool IsLoggedIn { get { return isLoggedIn; } }
        
        public string UserId { get { return userId; } }
        public string AccessToken { get { return accessToken; } }
        
        public string AppVersion{ get {return appVersion;} }
        
        public int DialogMode
        {
            get { return dialogMode; }
            set { dialogMode = value; iosSetShareDialogMode (dialogMode); }
        }
        
        public FBInsightsFlushBehavior FlushBehavior{ get {return flushBehavior;} }
        
        #region Init
        void Awake()
        {
            DontDestroyOnLoad (this);
            accessToken = "";
            isLoggedIn = false;
            userId = "";
            currRequestId=0;
            OutstandingRequestCallbacks = new Dictionary<int, Facebook.APIDelegate>();
            authResponseCallbacks = new List<Facebook.AuthChangeDelegate>();
        }

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
            Facebook.HideUnityDelegate hideUnityDelegate = null)
        {
            iosInit(cookie, logging, status, frictionlessRequests);
            externalInitDelegate = onInitComplete;
        }
        #endregion

        #region FB public interface
        public void Login(string scope = "", Facebook.AuthChangeDelegate callback = null)
        {
            if (isLoggedIn)
            {
                FbDebug.Warn("User " + userId + " is already logged in.  You do not need to call Login() again.");
            }
            AddAuthCallback(callback);
            iosLogin(scope);
        }
        
        public void Logout()
        {
            iosLogout();
            isLoggedIn = false;
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
            iosAppRequest(GetCallbackHandle(callback), message, to, to != null?to.Length:0, filters, excludeIds, excludeIds != null?excludeIds.Length:0, maxRecipients.HasValue, maxRecipients.HasValue?maxRecipients.Value:0, data, title);
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
            iosFeedRequest(GetCallbackHandle(null), toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference);
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
            throw new PlatformNotSupportedException("There is no Facebook Pay Dialog on iOS");
        }

        public void API(
            string query,
            HttpMethod method,
            Facebook.APIDelegate callback = null,
            Dictionary<string, string> formData = null,
            Facebook.ErrorDelegate errorCallback = null)
        {
            string[] dictKeys = null;
            string[] dictVals = null;

            if(formData != null && formData.Count > 0)
            {
                dictKeys = new string[formData.Count];
                dictVals = new string[formData.Count];
                int idx = 0;
                foreach( KeyValuePair<string, string> kvp in formData )
                {
                    dictKeys[idx] = kvp.Key;
                    dictVals[idx] = System.String.Copy(kvp.Value);
                    idx++;
                }
            }
            iosCallFbApi(GetCallbackHandle(callback), query, method!=null?method.ToString():null, dictKeys, dictVals, formData!=null?formData.Count:0);
        }
        
        public void GetAuthResponse(Facebook.AuthChangeDelegate callback = null)
        {
            AddAuthCallback(callback);
        }
        #endregion
        
        #region Insights public interface
        public void InsightsFlush() 
        {
            iosFBInsightsFlush();
        }

        public void InsightsLogConversionPixel(
            string pixelID, 
            double val) 
        {
            iosFBInsightsLogConversionPixel(pixelID, val);
        }
        
        public void InsightsLogPurchase(
            double purchaseAmount,
            string currency,
            Dictionary<string, string> properties = null) 
        {
            NativeDict dict = MarshallDict(properties);
            iosFBInsightsLogPurchase(purchaseAmount, currency, dict.numEntries, dict.keys, dict.vals);
        }
        
        public void InsightsSetAppVersion(string version) 
        {
            appVersion = version;
            iosFBInsightsSetAppVersion(version);
        }
        
        public void InsightsSetFlushBehavior(FBInsightsFlushBehavior behavior)
        {
            flushBehavior = behavior;
            iosFBInsightsSetFlushBehavior((int)flushBehavior);
        }
        
        public void PublishInstall(string appId, Facebook.APIDelegate callback = null)
        {
            iosFBSettingsPublishInstall(GetCallbackHandle(callback), appId);
        }
        #endregion
        
        
        
        #region Interal stuff
        private NativeDict MarshallDict(Dictionary<string, string> dict)
        {
            NativeDict res = new NativeDict();
            
            if(dict != null && dict.Count > 0)
            {
                res.keys = new string[dict.Count];
                res.vals = new string[dict.Count];
                res.numEntries=0;
                foreach( KeyValuePair<string, string> kvp in dict )
                {
                    res.keys[res.numEntries] = kvp.Key;
                    res.vals[res.numEntries] = kvp.Value;
                    res.numEntries++;
                }
            }
            return res;
        }
        
        private int GetCallbackHandle(Facebook.APIDelegate callback)
        {
            currRequestId++;
            OutstandingRequestCallbacks.Add(currRequestId, callback);
            return currRequestId;
        }
        
        private void AddAuthCallback(Facebook.AuthChangeDelegate callback)
        {
            if (callback != null && !authResponseCallbacks.Contains(callback))
            {
                authResponseCallbacks.Add(callback);
            }
        }

        private void OnInitComplete(string msg)
        {
            externalInitDelegate();
            if(msg != null && msg.Length > 0)
            {
                OnLogin (msg);
            }
        }
        
        private void OnAuthResponse()
        {
            isLoggedIn = true;
            foreach (Facebook.AuthChangeDelegate callback in authResponseCallbacks)
            {
                callback();
            }
            authResponseCallbacks.Clear();
        }
        
        public void OnLogin(string msg)
        {
            isLoggedIn = true;
            
            int delimIdx = msg.IndexOf(":");
            if(delimIdx <= 0)
            {
                FbDebug.Error("Malformed login response from ios.");
                FbDebug.Error("Here's the response that errored: " + msg);
                return;
            }
            
            userId = msg.Substring(0, delimIdx);
            accessToken = msg.Substring(delimIdx+1);
            OnAuthResponse();
        }
        
        public void OnLogout(string msg)
        {
            isLoggedIn = false;
        }
        
        public void OnRequestComplete(string msg)
        {
            int delimIdx = msg.IndexOf(":");
            if(delimIdx <= 0)
            {
                FbDebug.Error("Malformed callback from ios.  I expected the form id:message but couldn't find either the ':' character or the id.");
                FbDebug.Error("Here's the message that errored: " + msg);
                return;
            }
            
            string idStr = msg.Substring(0, delimIdx);
            string payload = msg.Substring(delimIdx+1);
            
            FbDebug.Info("id:" + idStr + " msg:" + payload);
            
            try
            {
                int key = System.Convert.ToInt32(idStr);
                if(!OutstandingRequestCallbacks.ContainsKey(key))
                {
                    return;
                }

                if(OutstandingRequestCallbacks[key] != null)
                    OutstandingRequestCallbacks[key](payload);


                OutstandingRequestCallbacks.Remove(key);
            } 
            catch (System.Exception e)
            {
                FbDebug.Error("Error converting callback id from string to int: " + e);
                FbDebug.Error("Here's the message that errored: " + msg);
            }
        }
        #endregion
    }
}
