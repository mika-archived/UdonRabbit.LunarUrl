/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

using UnityEngine;
using UnityEngine.UI;

using VRC.SDK3.Components;

namespace UdonRabbit.LunarUrl
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class LunarUrlSample : UdonSharpBehaviour
    {
        [SerializeField]
        private Text[] _arguments;

        [SerializeField]
        private VRCUrlInputField _field;

        [SerializeField]
        private Text _fragment;

        [SerializeField]
        private Text _hostname;

        [SerializeField]
        private UrlParser _parser;

        [SerializeField]
        private Text _path;

        [SerializeField]
        private Text _pathAndQuery;

        [SerializeField]
        private Text _port;

        [SerializeField]
        private Text _scheme;

        [SerializeField]
        private Text _userInfo;

        public void OnHandleUrlInput()
        {
            _parser.Parse(_field.GetUrl());

            _scheme.text = $"Scheme : {_parser.Scheme}";
            _hostname.text = $"Hostname : {_parser.Host}";
            _path.text = $"Path : {_parser.AbsolutePath}";
            _pathAndQuery.text = $"PathAndQuery : {_parser.PathAndQuery}";
            _userInfo.text = $"UserInfo : {_parser.User}";
            _port.text = $"Port : {_parser.Port}";
            _fragment.text = $"Fragment : {_parser.Fragment}";

            var keys = _parser.QueryDictionary.GetKeys();

            foreach (var t in _arguments)
                t.text = "Key=Value";

            for (var i = 0; i < keys.Length; i++)
            {
                if (i >= _arguments.Length)
                    break;

                var key = keys[i];
                var value = _parser.QueryDictionary.GetItem(key);

                if (value == null)
                    _arguments[i].text = $"{key}={null}";
                else if (value.GetType() == typeof(string[]))
                    _arguments[i].text = $"{key}={string.Join(", ", value)}";
                else
                    _arguments[i].text = $"{key}={value}";
            }
        }
    }
}