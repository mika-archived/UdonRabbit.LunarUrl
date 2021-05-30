/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;

using UdonSharp;

using UnityEngine;

using VRC.SDKBase;

namespace UdonRabbit.LunarUrl
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class UrlParser : UdonSharpBehaviour
    {
        #region Injection

        [SerializeField]
        private SimpleDictionary _parameters;

        #endregion

        public void Parse(VRCUrl url)
        {
            var uri = url.Get().Trim();

            _parameters.Clear();

            var addr1 = ParseScheme(uri);
            var addr2 = ParseFragment(addr1);
            var addr3 = ParseQuery(addr2).Replace("\\", "/");
            var addr4 = ParsePath(addr3);
            var addr5 = ParseUserInfo(addr4);
            var addr6 = ParseHostname(addr5);
            ParsePortNumber(addr6);
        }

        private string ParseScheme(string url)
        {
            var sb = "";
            var i = 0;
            var phase = 0;
            var truncate = 0;
            var chars = url.ToCharArray();

            if (chars.Length == 0)
            {
                Scheme = sb;
                return url;
            }

            while (true)
            {
                var c = chars[i++];

                if (phase == 0)
                {
                    // PHASE0: /a-z/i
                    if (0x41 <= c && c <= 0x5a || 0x61 <= c && c <= 0x7a)
                        sb += c;
                    else
                        break;

                    phase = 1;
                    continue;
                }

                if (phase == 1)
                {
                    // PHASE1: /a-z0-9.+-/i
                    if (0x41 <= c && c <= 0x5a || 0x61 <= c && c <= 0x7a || 0x30 <= c && c <= 0x39 || c == '+' || c == '-' || c == '.')
                    {
                        sb += c;
                        continue;
                    }

                    phase = 2;
                }

                if (phase == 2)
                {
                    if (c == ':')
                        sb += c;

                    phase = 3;
                    continue;
                }

                if (phase == 3)
                {
                    if (c == '/' || c == '\\')
                    {
                        truncate++;
                        continue;
                    }

                    phase = 4;
                }

                if (phase >= 4)
                    break;
            }

            Scheme = sb;

            if (truncate > 0)
                _hasHostname = true;

            return url.Substring(sb.Length + truncate);
        }

        private string ParseFragment(string url)
        {
            var index = url.IndexOf("#", StringComparison.Ordinal);
            if (index >= 0)
            {
                Fragment = url.Substring(index);
                return url.Substring(0, index);
            }

            Fragment = string.Empty;
            return url;
        }

        private string ParseQuery(string url)
        {
            var index = url.IndexOf("?", StringComparison.Ordinal);
            if (index >= 0)
            {
                var parameters = url.Substring(index + 1);
                while (parameters.StartsWith("?") || parameters.StartsWith("#") || parameters.StartsWith("&"))
                    parameters = parameters.Substring(1);

                var keyValuePair = parameters.Split('&');

                Query = parameters;
                QueryDictionary.Initialize((uint) keyValuePair.Length);

                foreach (var parameter in keyValuePair)
                {
                    if (string.IsNullOrWhiteSpace(parameter))
                        continue;

                    var arr = parameter.Replace("+", " ").Split('=');
                    var key = arr[0];

                    arr[0] = "";
                    var value = string.Concat(arr);

                    if (QueryDictionary.IsExists(key))
                    {
                        var item = QueryDictionary.GetItem(key);
                        if (item.GetType() == typeof(string[]))
                        {
                            var items = (string[]) item;
                            var newItems = new string[items.Length + 1];
                            for (var i = 0; i < items.Length; i++)
                                newItems[i] = items[i];
                            newItems[items.Length] = value;

                            QueryDictionary.UpdateItem(key, newItems);
                        }
                        else
                        {
                            var items = new string[2];
                            items[0] = (string) item;
                            items[1] = value;

                            QueryDictionary.UpdateItem(key, items);
                        }
                    }
                    else
                    {
                        QueryDictionary.AddItem(key, value);
                    }
                }

                return url.Substring(0, index);
            }

            Query = "";
            return url;
        }

        private string ParsePath(string url)
        {
            if (_hasHostname)
            {
                var index = url.IndexOf("/", StringComparison.Ordinal);
                if (index >= 0)
                {
                    AbsolutePath = url.Substring(index);
                    return url.Substring(0, index);
                }

                AbsolutePath = "";
                return url;
            }

            AbsolutePath = url;
            return "";
        }

        private string ParseUserInfo(string url)
        {
            var index = url.IndexOf("@", StringComparison.Ordinal);
            if (index >= 0)
            {
                User = url.Substring(0, index);
                return url.Substring(index + "@".Length);
            }

            User = "";
            return url;
        }

        private string ParseHostname(string url)
        {
            // IPv6 Addr
            if (url.StartsWith("["))
            {
                var index = url.IndexOf("]", StringComparison.Ordinal);
                Host = url.Substring(0, index + 1).ToLowerInvariant();
                return url.Substring(index + "]".Length);
            }
            else
            {
                var index = url.IndexOf(":", StringComparison.Ordinal);
                if (index >= 0)
                {
                    Host = url.Substring(0, index).ToLowerInvariant();
                    return url.Substring(index + ":".Length);
                }

                Host = url.ToLowerInvariant();
                return "";
            }
        }

        internal void ParsePortNumber(string url)
        {
            if (url.StartsWith(":"))
                url = url.Substring(1);

            var port = 0;
            if (int.TryParse(url, out port))
                Port = port;
        }

#if !COMPILER_UDONSHARP

        public void SetDictionary(SimpleDictionary dictionary)
        {
            _parameters = dictionary;
        }

#endif

        #region Private Variables

        private bool _hasHostname;

        public string Fragment { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Query { get; private set; }
        public string Scheme { get; private set; }
        public string User { get; private set; }

        public string PathAndQuery
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Query))
                    return $"{AbsolutePath}";
                return $"{AbsolutePath}?{Query}";
            }
        }

        public string AbsolutePath { get; private set; }

        public SimpleDictionary QueryDictionary => _parameters;

        #endregion
    }
}