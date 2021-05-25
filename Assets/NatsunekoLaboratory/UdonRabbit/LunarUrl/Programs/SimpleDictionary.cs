using System;

using UdonSharp;

using UnityEngine;

namespace UdonRabbit.LunarUrl
{
    public class SimpleDictionary : UdonSharpBehaviour
    {
        private string[] _aliases;
        private bool[] _buckets;
        private uint _capacity;
        private object[] _contents;
        private uint _count;

        public void Initialize(uint capacity)
        {
            _capacity = capacity;
            _aliases = new string[capacity];
            _contents = new object[capacity];
            _buckets = new bool[capacity];
            _count = 0;
        }

        public void AddItem(string key, object value)
        {
            var index = FindLastSpace();
            if (index < 0)
            {
                Debug.LogError("The item could not be added because the dictionary size has reached its maximum.");
                return;
            }

            _aliases[index] = key;
            _contents[index] = value;
            _buckets[index] = true;
            _count++;
        }

        public void RemoveItem(string key)
        {
            var index = FindItem(key);
            if (index < 0)
                return;

            _aliases[index] = null;
            _contents[index] = null;
            _buckets[index] = false;
            _count--;
        }

        public object GetItem(string key)
        {
            var index = FindItem(key);
            if (index < 0)
                return null;

            return _contents[index];
        }

        public void UpdateItem(string key, object value)
        {
            var index = FindItem(key);
            if (index < 0)
                return;

            _contents[index] = value;
        }

        public bool IsExists(string key)
        {
            return FindItem(key) >= 0;
        }

        public uint GetCount()
        {
            return _count;
        }

        public void Clear()
        {
            if (_aliases == null || _contents == null)
                return;

            Array.Clear(_aliases, 0, (int) _count);
            Array.Clear(_contents, 0, (int) _count);
        }

        private int FindItem(string key)
        {
            return Array.IndexOf(_aliases, key);
        }

        private int FindLastSpace()
        {
            return Array.IndexOf(_buckets, false);
        }
    }
}