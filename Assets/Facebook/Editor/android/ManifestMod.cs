using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;

namespace UnityEditor.FacebookEditor
{
    public class ManifestMod
    {
        public const string ActivityName = "com.facebook.unity.FBUnityPlayerActivity";

        public static void GenerateManifest()
        {
            var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
            var outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

            // TODO: be able to merge AndroidManifest.xml that may be included
            File.Copy(inputFile, outputFile, true);

            UpdateManifest(outputFile);
        }

        private static XmlNode FindChildNode(XmlNode parent, string name)
        {
            XmlNode curr = parent.FirstChild;
            while(curr != null)
            {
                if(curr.Name.Equals(name))
                {
                    return curr;
                }
                curr = curr.NextSibling;
            }
            return null;
        }

        private static XmlElement FindMainActivityNode(XmlNode parent)
        {
            XmlNode curr = parent.FirstChild;
            while(curr != null)
            {
                if(curr.Name.Equals("activity") && curr.FirstChild != null && curr.FirstChild.Name.Equals("intent-filter"))
                {
                    return curr is XmlElement ? (XmlElement)curr : null;
                }
                curr = curr.NextSibling;
            }
            return null;
        }


        public static void UpdateManifest(string fullPath)
        {
            string appId = FBSettings.AppId;

            if(!FBSettings.IsValidAppId)
            {
                Debug.LogError("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);

            if(doc == null)
            {
                Debug.LogError("Couldn't load " + fullPath);
                return;
            }

            XmlNode manNode = FindChildNode (doc, "manifest");
            XmlNode dict = FindChildNode (manNode, "application");

            if(dict == null)
            {
                Debug.LogError("Error parsing " + fullPath);
                return;
            }

            string ns = dict.GetNamespaceOfPrefix("android");

            //change 
            //<activity android:name="com.unity3d.player.UnityPlayerProxyActivity" android:launchMode="singleTask" android:label="@string/app_name" android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen" android:screenOrientation="portrait">
            //to
            //<activity android:name="com.facebook.unity.FBUnityPlayerActivity" android:launchMode="singleTask" android:label="@string/app_name" android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen" android:screenOrientation="portrait">
            XmlElement mainActivity = FindMainActivityNode (dict);
            mainActivity.SetAttribute("name", ns, ActivityName);

            //add the login activity
            //<activity android:name="com.facebook.LoginActivity" android:screenOrientation="portrait" android:configChanges="keyboardHidden|orientation">
            //</activity>
            XmlElement loginElement = doc.CreateElement("activity");
            loginElement.SetAttribute ("name", ns, "com.facebook.LoginActivity");
            loginElement.SetAttribute ("screenOrientation", ns, "portrait");
            loginElement.SetAttribute ("configChanges", ns, "keyboardHidden|orientation");
            loginElement.InnerText = "\n    ";  //be extremely anal to make diff tools happy
            dict.AppendChild (loginElement);

            //add the app id
            //<meta-data android:name="com.facebook.sdk.ApplicationId" android:value="\ 409682555812308" />
            XmlElement appIdElement = doc.CreateElement("meta-data");
            appIdElement.SetAttribute ("name", ns, "com.facebook.sdk.ApplicationId");
            appIdElement.SetAttribute ("value", ns, "\\ " + appId); //stupid hack so that the id comes out as a string
            dict.AppendChild(appIdElement);

            doc.Save(fullPath);
        }
    }
}