using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Facebook
{
    class EditorFacebook : MonoBehaviour, IFacebook
    {
        private IFacebook fb;
        private bool isFakeLoggedIn = false;

        public string UserId { get { return "0"; } }
        public string AccessToken { get { return "abcdefghijklmnopqrstuvwxyz"; } }
        public bool IsLoggedIn { get { return isFakeLoggedIn; } }

        public int DialogMode
        {
            get { return 0; }
            set { ; }
        }

        #region Init
        void Awake()
        {
            // bootstrap the canvas facebook for native dialogs
            StartCoroutine(FB.RemoteFacebookLoader.LoadFacebookClass("CanvasFacebook", OnDllLoaded));
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
            StartCoroutine(OnInit(onInitComplete, appId, cookie, logging, status, xfbml, channelUrl, authResponse, frictionlessRequests, hideUnityDelegate));
        }

        private IEnumerator OnInit(
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
            // wait until the native dialogs are loaded
            while (fb == null)
            {
                yield return null;
            }
            fb.Init(onInitComplete, appId, cookie, logging, status, xfbml, channelUrl, authResponse, frictionlessRequests, hideUnityDelegate);
            if (status || cookie)
            {
                isFakeLoggedIn = true;
            }
            if (onInitComplete != null)
            {
                onInitComplete();
            }
        }

        private void OnDllLoaded(IFacebook fb)
        {
            this.fb = fb;
        }
        #endregion

        public void Login(string scope = "", Facebook.AuthChangeDelegate callback = null)
        {
            if (isFakeLoggedIn)
            {
                FbDebug.Warn("User is already logged in.  You don't need to call this again.");
            }

            isFakeLoggedIn = true;
        }

        public void Logout()
        {
            isFakeLoggedIn = false;
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
            fb.AppRequest(message, to, filters, excludeIds, maxRecipients, data, title, callback);
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
            fb.FeedRequest(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference);
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
            FbDebug.Info("Pay method only works with Facebook Canvas.  Does nothing in the Unity Editor, iOS or Android");
        }

        public void API(
            string query,
            HttpMethod method,
            Facebook.APIDelegate callback = null,
            Dictionary<string, string> formData = null,
            Facebook.ErrorDelegate errorCallback = null)
        {
            if (query.StartsWith("me"))
            {
                FbDebug.Warn("graph.facebook.com/me does not work within the Unity Editor");
            }

            if (!query.Contains("access_token=") && (formData == null || !formData.ContainsKey("access_token")))
            {
                FbDebug.Warn("Without an access_token param explicitly passed in formData, some API graph calls will 404 error in the Unity Editor.");
            }
            fb.API(query, method, callback, formData, errorCallback);
        }

        public void GetAuthResponse(Facebook.AuthChangeDelegate callback = null)
        {
            fb.GetAuthResponse(callback);
        }
        
        public void PublishInstall(string appId, Facebook.APIDelegate callback = null) {}
    }
}
