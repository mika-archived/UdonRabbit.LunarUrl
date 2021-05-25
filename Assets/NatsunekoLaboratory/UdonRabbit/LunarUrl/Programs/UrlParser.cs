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
                _scheme = sb;
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

            _scheme = sb;

            if (truncate > 0)
                _hasHostname = true;

            return url.Substring(sb.Length + truncate);
        }

        private string ParseFragment(string url)
        {
            var index = url.IndexOf("#", StringComparison.Ordinal);
            if (index >= 0)
            {
                _fragment = url.Substring(index);
                return url.Substring(0, index);
            }

            _fragment = string.Empty;
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

                _query = parameters;
                _parameters.Initialize((uint) keyValuePair.Length);

                foreach (var parameter in keyValuePair)
                {
                    if (string.IsNullOrWhiteSpace(parameter))
                        continue;

                    var arr = parameter.Replace("+", " ").Split('=');
                    var key = arr[0];

                    arr[0] = "";
                    var value = string.Concat(arr);

                    if (_parameters.IsExists(key))
                    {
                        var item = _parameters.GetItem(key);
                        if (item.GetType() == typeof(string[]))
                        {
                            var items = (string[]) item;
                            var newItems = new string[items.Length + 1];
                            for (var i = 0; i < items.Length; i++)
                                newItems[i] = items[i];
                            newItems[items.Length] = value;

                            _parameters.UpdateItem(key, newItems);
                        }
                        else
                        {
                            var items = new string[2];
                            items[0] = (string) item;
                            items[1] = value;

                            _parameters.UpdateItem(key, items);
                        }
                    }
                    else
                    {
                        _parameters.AddItem(key, value);
                    }
                }

                return url.Substring(0, index);
            }

            _query = "";
            return url;
        }

        private string ParsePath(string url)
        {
            if (_hasHostname)
            {
                var index = url.IndexOf("/", StringComparison.Ordinal);
                if (index >= 0)
                {
                    _path = url.Substring(index);
                    return url.Substring(0, index);
                }

                _path = "";
                return url;
            }

            _path = url;
            return "";
        }

        private string ParseUserInfo(string url)
        {
            var index = url.IndexOf("@", StringComparison.Ordinal);
            if (index >= 0)
            {
                _user = url.Substring(0, index);
                return url.Substring(index + "@".Length);
            }

            _user = "";
            return url;
        }

        private string ParseHostname(string url)
        {
            // IPv6 Addr
            if (url.StartsWith("["))
            {
                var index = url.IndexOf("]", StringComparison.Ordinal);
                _host = url.Substring(0, index + 1).ToLowerInvariant();
                return url.Substring(index + "]".Length);
            }
            else
            {
                var index = url.IndexOf(":", StringComparison.Ordinal);
                if (index >= 0)
                {
                    _host = url.Substring(0, index).ToLowerInvariant();
                    return url.Substring(index + ":".Length);
                }

                _host = url.ToLowerInvariant();
                return "";
            }
        }

        private void ParsePortNumber(string url)
        {
            if (url.StartsWith(":"))
                url = url.Substring(1);

            var port = 0;
            if (int.TryParse(url, out port))
                _port = port;
        }

#if !COMPILER_UDONSHARP

        public void SetDictionary(SimpleDictionary dictionary)
        {
            _parameters = dictionary;
        }

#endif

        #region Private Variables

        private string _fragment;
        private string _host;
        private string _path;
        private int _port;
        private string _query;
        private string _scheme;
        private string _user;
        private bool _hasHostname;

        #endregion

        #region Properties

        public string GetAbsolutePath()
        {
            return _path;
        }

        public string GetFragment()
        {
            return _fragment;
        }

        public string GetHost()
        {
            return _host;
        }

        public string GetPathAndQuery()
        {
            if (string.IsNullOrWhiteSpace(_query))
                return $"{_path}";
            return $"{_path}?{_query}";
        }

        public int GetPort()
        {
            return _port;
        }

        public SimpleDictionary GetQuery()
        {
            return _parameters;
        }

        public string GetScheme()
        {
            return _scheme;
        }

        public string GetUserInfo()
        {
            return _user;
        }

        #endregion
    }
}