using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public sealed class FB : ScriptableObject
{
    public static Facebook.InitDelegate OnInitComplete;
    public static Facebook.HideUnityDelegate OnHideUnity;

    private static IFacebook facebook;
    private static string authResponse;
    private static bool isInitCalled = false;

    static IFacebook FacebookImpl
    {
        get
        {
            if (facebook == null)
            {
                throw new NullReferenceException("Facebook object is not yet loaded.  Did you call FB.Init()?");
            }
            return facebook;
        }
    }

    public static string AppId { get { return FBSettings.AppId; } }
    public static string UserId 
    { 
        get 
        {
            return (facebook != null) ? facebook.UserId : ""; 
        } 
    }
    public static string AccessToken 
    {
        get 
        { 
            return (facebook != null) ? facebook.AccessToken : ""; 
        } 
    }

    public static bool IsLoggedIn
    {
        get
        {
            return (facebook != null) ? facebook.IsLoggedIn : false;
        }
    }

    #region Init
    /**
     * onInitComplete - Delegate is called when FB.Init() finished initializing everything.
     *                  By passing in a delegate you can find out when you can safely call the other methods.
     */
    public static void Init(Facebook.InitDelegate onInitComplete, Facebook.HideUnityDelegate onHideUnity = null, string authResponse = null)
    {
        if (!isInitCalled)
        {
            FB.authResponse = authResponse;
            FB.OnInitComplete = onInitComplete;
            FB.OnHideUnity = onHideUnity;

            FbDebug.Log(String.Format("Using SDK {0}, Build {1}", FBBuildVersionAttribute.SDKVersion, GetBuildVersionOfType(typeof(IFacebook))));

#if UNITY_EDITOR
            FbDebug.Log("Creating Editor version of Facebook object...");
            FBComponentFactory.GetComponent<EditorFacebookLoader>();
#elif UNITY_WEBPLAYER
            FbDebug.Log("Creating Webplayer version of Facebook object...");
            FBComponentFactory.GetComponent<CanvasFacebookLoader>();
#elif UNITY_IOS
            FbDebug.Log("Creating iOS version of Facebook object...");
            FBComponentFactory.GetComponent<IOSFacebookLoader>();
#elif UNITY_ANDROID
            FbDebug.Log("Creating Android version of Facebook object...");
            FBComponentFactory.GetComponent<AndroidFacebookLoader>();
#else
            throw new NotImplementedException("Facebook API does not yet support this platform");
#endif
            FB.isInitCalled = true;
            return;
        }

        FbDebug.Warn("FB.Init() has already been called.  You only need to call this once and only once.");
		
		// Init again if possible just in case something bad actually happened.
		if (FacebookImpl != null) {
			OnDllLoaded();
		}
    }

    private static void OnDllLoaded()
    {
        FbDebug.Log("Finished loading Facebook dll. Build " + GetBuildVersionOfType(FacebookImpl.GetType()));
        FacebookImpl.Init(
            OnInitComplete,
            FBSettings.AppId,
            FBSettings.Cookie,
            FBSettings.Logging,
            FBSettings.Status,
            FBSettings.Xfbml,
            FBSettings.ChannelUrl,
            authResponse,
            FBSettings.FrictionlessRequests,
            OnHideUnity
        );
    }
    #endregion

    public static void Login(string scope = "", Facebook.AuthChangeDelegate callback = null)
    {
        FacebookImpl.Login(scope, callback);
    }

    public static void Logout()
    {
        FacebookImpl.Logout();
    }

    public static void AppRequest(
            string message,
            string[] to = null,
            string filters = "",
            string[] excludeIds = null,
            int? maxRecipients = null,
            string data = "",
            string title = "",
            Facebook.APIDelegate callback = null)
    {
        FacebookImpl.AppRequest(message, to, filters, excludeIds, maxRecipients, data, title, callback);
    }

    public static void Feed(
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
        FacebookImpl.FeedRequest(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference, properties);
    }

    public static void API(string query, HttpMethod method, Facebook.APIDelegate callback = null, Dictionary<string, string> formData = null)
    {
        FacebookImpl.API(query, method, callback, formData);
    }

    public static void GetAuthResponse(Facebook.AuthChangeDelegate callback = null)
    {
        FacebookImpl.GetAuthResponse(callback);
    }

    public static void PublishInstall(Facebook.APIDelegate callback = null)
    {
        FacebookImpl.PublishInstall(AppId, callback);
    }

    #region Canvas-Only Implemented Methods
    public sealed class Canvas
    {
        public static void Pay(
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
            FacebookImpl.Pay(product, action, quantity, quantityMin, quantityMax, requestId, pricepointId, testCurrency, callback);
        }
    }
    #endregion

    #region Build Version Info
    private static string GetBuildVersionOfType(Type type)
    {
        Assembly assembly = Assembly.GetAssembly(type);
        var buildVersionAttributes = (FBBuildVersionAttribute[])(assembly.GetCustomAttributes(typeof(FBBuildVersionAttribute), false));
        string buildVersion = "N/A";
        foreach (FBBuildVersionAttribute attribute in buildVersionAttributes)
        {
            buildVersion = attribute.ToString();
        }
        return buildVersion;
    }
    #endregion

    #region Facebook Loader Class
    public abstract class RemoteFacebookLoader : MonoBehaviour
    {
        public delegate void LoadedDllCallback(IFacebook fb);

        public static IEnumerator LoadFacebookClass(string className, LoadedDllCallback callback)
        {
            var url = string.Format(IntegratedPluginCanvasLocation.DllUrl, className);
            FbDebug.Log("Loading " + className + " from: " + url);
            var www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                ThrowNullReferenceException(www.error);
            }

            var assembly = Security.LoadAndVerifyAssembly(www.bytes);

            if (assembly == null)
            {
                ThrowNullReferenceException("Could not load assembly from " + url);
            }

            var facebookClass = assembly.GetType("Facebook." + className);

            if (facebookClass == null)
            {
                ThrowNullReferenceException(className + " not found in assembly!");
            }

            // load the Facebook component into the gameobject
            var fb = (IFacebook)typeof(FBComponentFactory)
                                            .GetMethod("GetComponent")
                                            .MakeGenericMethod(facebookClass)
                                            .Invoke(null, new object[1] { IfNotExist.AddNew });

            if (fb == null)
            {
                ThrowNullReferenceException(className + " couldn't be created.");
            }

            callback(fb);
            www.Dispose();
        }

        private static void ThrowNullReferenceException(string error)
        {
            FbDebug.Error(error);
            throw new NullReferenceException(error);
        }

        protected abstract string className { get; }

        IEnumerator Start()
        {
            var loader = LoadFacebookClass(className, OnDllLoaded);
            while (loader != null && loader.MoveNext())
            {
                yield return loader.Current;
            }
            Destroy(this);
        }

        private void OnDllLoaded(IFacebook fb)
        {
            FB.facebook = fb;
            FB.OnDllLoaded();
        }
    }

    public abstract class CompiledFacebookLoader : MonoBehaviour
    {
        protected abstract IFacebook fb { get; }

        void Start()
        {
            FB.facebook = fb;
            FB.OnDllLoaded();
            Destroy(this);
        }
    }
    #endregion
}
