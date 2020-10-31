using System;
using Halodi.PackageRegistry.Core;
using Halodi.PackageRegistry.NPM;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    internal class TokenMethod : GUIContent
    {
        internal delegate void GetToken(ScopedRegistry registry, string username, string password);
        internal string usernameName;
        internal string passwordName;
        internal GetToken action;

        public TokenMethod(string name, string usernameName, string passwordName, GetToken action) : base(name)
        {
            this.usernameName = usernameName;
            this.passwordName = passwordName;
            this.action = action;
        }
    }

    internal class GetTokenView : EditorWindow
    {
        private static TokenMethod[] methods = {
                new TokenMethod("npm login", "Registry username", "Registry password", GetNPMLoginToken),
                new TokenMethod("bintray", "Bintray username", "Bintray API key", GetBintrayToken),
                // TODO adjust TokenMethod to allow for opening GitHub token URL: https://github.com/settings/tokens/new
            };


        private string username;
        private string password;

        private bool initialized = false;

        private TokenMethod tokenMethod;

        private ScopedRegistry registry;


        void OnEnable()
        {
        }

        void OnDisable()
        {
            initialized = false;
        }

        private void SetRegistry(TokenMethod tokenMethod, ScopedRegistry registry)
        {
            this.tokenMethod = tokenMethod;
            this.registry = registry;
            this.initialized = true;
        }

        void OnGUI()
        {
            if (initialized)
            {
                EditorGUILayout.LabelField(tokenMethod, EditorStyles.whiteLargeLabel);
                username = EditorGUILayout.TextField(tokenMethod.usernameName, username);
                password = EditorGUILayout.PasswordField(tokenMethod.passwordName, password);

                if (GUILayout.Button("Login"))
                {
                    tokenMethod.action(registry, username, password);
                    CloseWindow();
                }

                if (GUILayout.Button("Close"))
                {
                    CloseWindow();
                }
            }
        }

        private static void CreateWindow(TokenMethod method, ScopedRegistry registry)
        {
            GetTokenView getTokenView = EditorWindow.GetWindow<GetTokenView>(true, "Get token", true);
            getTokenView.SetRegistry(method, registry);
        }


        private static void GetNPMLoginToken(ScopedRegistry registry, string username, string password)
        {
            NPMResponse response = NPMLogin.GetLoginToken(registry.url, username, password);

            if (string.IsNullOrEmpty(response.ok))
            {
                EditorUtility.DisplayDialog("Cannot get token", response.error, "Ok");
            }
            else
            {
                registry.token = response.token;
            }
        }

        private static void GetBintrayToken(ScopedRegistry registry, string username, string password)
        {
            registry.token = NPMLogin.GetBintrayToken(username, password);
        }


        private void CloseWindow()
        {

            Close();
            GUIUtility.ExitGUI();
        }


        internal static int CreateGUI(int selectedIndex, ScopedRegistry registry)
        {

            EditorGUILayout.LabelField("Generate token", EditorStyles.largeLabel);
            EditorGUILayout.BeginHorizontal();
            selectedIndex = EditorGUILayout.Popup(new GUIContent("Method"), selectedIndex, methods);

            if(GUILayout.Button("Login & get auth token"))
            {
                CreateWindow(methods[selectedIndex], registry);
            }

            EditorGUILayout.EndHorizontal();

            return selectedIndex;
        }
    }
}