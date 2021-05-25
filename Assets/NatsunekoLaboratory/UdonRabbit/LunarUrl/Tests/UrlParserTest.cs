using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using VRC.SDKBase;

namespace UdonRabbit.LunarUrl.Tests
{
    [TestFixture]
    public class UrlParserTest
    {
        [SetUp]
        public void BeforeEach()
        {
            _gameObject = new GameObject("UrlParser");
        }

        [TearDown]
        public void AfterEach()
        {
            Object.Destroy(_gameObject);
        }

        private GameObject _gameObject;

        private static TestCaseData[] GetParseUrlTestCases()
        {
            // test data: https://github.com/unshiftio/url-parse/blob/master/test/test.js
            return new[]
            {
                new TestCaseData("", "", "", 0, "", "", "", "", 0).Returns(null),
                new TestCaseData("//foo", "", "", 0, "//foo", "//foo", "", "", 0).Returns(null),
                new TestCaseData("http://example.com", "http:", "", 0, "", "", "", "example.com", 0).Returns(null),
                new TestCaseData(" javascript://foo", "javascript:", "", 0, "", "", "", "foo", 0).Returns(null),
                new TestCaseData("https://google.com/?foo=bar", "https:", "", 1, "/", "/?foo=bar", "", "google.com", 0).Returns(null),
                new TestCaseData("blob:https%3A//gist.github.com/3f272586-6dac-4e29-92d0-f674f2dde618", "blob:", "", 0, "https%3A//gist.github.com/3f272586-6dac-4e29-92d0-f674f2dde618", "https%3A//gist.github.com/3f272586-6dac-4e29-92d0-f674f2dde618", "", "", 0).Returns(null),
                new TestCaseData("HTTP://fOo.eXaMPle.com", "HTTP:", "", 0, "", "", "", "foo.example.com", 0).Returns(null),
                new TestCaseData("http://example.com/", "http:", "", 0, "/", "/", "", "example.com", 0).Returns(null),
                new TestCaseData("http://x.com/path?that 's#all, folks", "http:", "#all, folks", 1, "/path", "/path?that 's", "", "x.com", 0).Returns(null),
                new TestCaseData("http://google.com:80\\@yahoo.com/#what\\is going on", "http:", "#what\\is going on", 0, "/@yahoo.com/", "/@yahoo.com/", "", "google.com", 80).Returns(null),
                new TestCaseData("https:\\/github.com/foo/bar", "https:", "", 0, "/foo/bar", "/foo/bar", "", "github.com", 0).Returns(null),
                new TestCaseData("mailto:test@example.com", "mailto:", "", 0, "test@example.com", "test@example.com", "", "", 0).Returns(null),
                new TestCaseData("data:text/html,%3Ch1%3EHello%2C%20World!%3C%2Fh1%3E", "data:", "", 0, "text/html,%3Ch1%3EHello%2C%20World!%3C%2Fh1%3E", "text/html,%3Ch1%3EHello%2C%20World!%3C%2Fh1%3E", "", "", 0).Returns(null),
                new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]:61616/foo/bar?q=p", "http:", "", 1, "/foo/bar", "/foo/bar?q=p", "", "[1080:0:0:0:8:800:200c:417a]", 61616).Returns(null),
                new TestCaseData("http://user:password@[3ffe:2a00:100:7031::1]:8080/", "http:", "", 0, "/", "/", "user:password", "[3ffe:2a00:100:7031::1]", 8080).Returns(null),
                new TestCaseData("http://222.148.142.13:61616/foo/bar?q=z", "http:", "", 1, "/foo/bar", "/foo/bar?q=z", "", "222.148.142.13", 61616).Returns(null),
                new TestCaseData("http://mt0.google.com/vt/lyrs=m@114&hl=en&src=api&x=2&y=2&z=3&s=", "http:", "", 0, "/vt/lyrs=m@114&hl=en&src=api&x=2&y=2&z=3&s=", "/vt/lyrs=m@114&hl=en&src=api&x=2&y=2&z=3&s=", "", "mt0.google.com", 0).Returns(null),
                new TestCaseData("http://mt0.google.com/vt/lyrs=m@114???&hl=en&src=api&x=2&y=2&z=3&s=", "http:", "", 6, "/vt/lyrs=m@114", "/vt/lyrs=m@114?hl=en&src=api&x=2&y=2&z=3&s=", "", "mt0.google.com", 0).Returns(null)
            };
        }

        [UnityTest]
        [TestCaseSource(nameof(GetParseUrlTestCases))]
        public IEnumerator ParseTest(string url, string scheme, string fragment, int size, string path, string pathAndQuery, string userInfo, string hostname, int port)
        {
            var parser = _gameObject.AddComponent<UrlParser>();
            var dict = _gameObject.AddComponent<SimpleDictionary>();
            parser.SetDictionary(dict);

            yield return null;

            parser.Parse(new VRCUrl(url));

            Assert.AreEqual(parser.GetScheme(), scheme);
            Assert.AreEqual(parser.GetFragment(), fragment);
            Assert.AreEqual(parser.GetQuery().GetCount(), (uint) size);
            Assert.AreEqual(parser.GetAbsolutePath(), path);
            Assert.AreEqual(parser.GetPathAndQuery(), pathAndQuery);
            Assert.AreEqual(parser.GetUserInfo(), userInfo);
            Assert.AreEqual(parser.GetHost(), hostname);
            Assert.AreEqual(parser.GetPort(), port);
        }
    }
}