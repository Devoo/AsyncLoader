using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace System.Web
{
    public static class AsyncLoader
    {

        private static readonly ConcurrentStringDictionaryWithTimeout<Func<string, string>> mAsyncData =
            new ConcurrentStringDictionaryWithTimeout<Func<string, string>>();

        public static string AsyncData(string id, string data = null)
        {
            Func<string, string> task;
            if (!mAsyncData.TryGetValue(id, out task))
                return "";

            var res = task(data);

            return res;
        }
        
        public static HtmlString LoadAsync(
            this HtmlHelper helper,
            Func<string> asyncData,
            TimeSpan refreshEvery = default(TimeSpan),
            Func<string, string> afterLoadScript = null)
        {
            return LoadAsync(helper, @"<i class=""fa fa-2x fa-spin fa-spinner"" style=""color:#999;margin: 10px;""></i><br/>",
                asyncData, refreshEvery, afterLoadScript);
        }

        public static HtmlString LoadAsync(
            this HtmlHelper helper,
            string loadingHtml,
            Func<string> asyncData,
            TimeSpan refreshEvery = default(TimeSpan),
            Func<string, string> afterLoadScript = null)
        {
            return new HtmlString(LoadAsync(loadingHtml, asyncData, refreshEvery, afterLoadScript));
        }

        public static string LoadAsync(
            Func<string> asyncData,
            TimeSpan refreshEvery,
            Func<string, string> afterLoadScript = null)
        {
            return LoadAsync(@"<i class=""fa fa-2x fa-spin fa-spinner"" style=""color:#999;margin: 10px;""></i><br/>",
                asyncData, refreshEvery, afterLoadScript);
        }

        public static string LoadAsync(
            string loadingHtml,
            Func<string> asyncData,
            TimeSpan refreshEvery,
            Func<string, string> afterLoadScript = null)
        {
            return LoadAsync(loadingHtml, args => asyncData(), refreshEvery, afterLoadScript);
        }

        public static string LoadAsync(
            string loadingHtml,
            Func<string, string> asyncData,
            TimeSpan refreshEvery,
            Func<string, string> afterLoadScript = null)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");

            var res = Environment.NewLine + "<div id='async-" + guid + "' style='clear:both;' >" + loadingHtml + "</div> ";

            mAsyncData.Add(guid, refreshEvery != TimeSpan.Zero
                ? TimeSpan.FromDays(1)
                : TimeSpan.FromHours(1), (args) => asyncData(args));

            res += @"<script>$(function() {";

            if (refreshEvery != TimeSpan.Zero)
                res += "var interval_" + guid + " = null;" + Environment.NewLine + " function asyncLoad" + guid + "(){";

            res += @"$('#async-" + guid + @"').load('/async/data/" + guid + @"',
                        function( response, status, xhr )  {
                            if ( status == 'error' ) { 
                                $('#async-" + guid + @"').html('Error de carga...');
                                return;
                               } 
                    ";

            if (refreshEvery != TimeSpan.Zero)
                res += "if (interval_" + guid + @" == null) interval_" + guid + " = setInterval(asyncLoad" + guid +
                       ", " + ((int)refreshEvery.TotalMilliseconds) + ");" + Environment.NewLine + Environment.NewLine;

            if (afterLoadScript != null)
                res += afterLoadScript("async-" + guid);

            res += "})";


            if (refreshEvery != TimeSpan.Zero)
                res += "}; asyncLoad" + guid + "();";

            res += " });</script>" + Environment.NewLine;


            return res;
        }


        public static string SaveForAsync(string asyncResponse)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");

            mAsyncData.Add(guid, TimeSpan.FromHours(1), _ => asyncResponse);

            return guid;
        }

        public static string SaveForAsync(Func<string> asyncResponseFunc)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");

            mAsyncData.Add(guid, TimeSpan.FromHours(1), _ => asyncResponseFunc());

            return guid;
        }

        public static string SaveForAsync(Func<string, string> asyncResponseFunc)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");

            mAsyncData.Add(guid, TimeSpan.FromHours(1), asyncResponseFunc);

            return guid;
        }

        #region "  Helper Cache Class  "

        [Serializable]
        [ComVisible(false)]
        private class ConcurrentStringDictionaryWithTimeout<TValue>
            where TValue : class
        {
            public ConcurrentStringDictionaryWithTimeout()
            {
                mEntries = new Dictionary<string, Entry>();
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(TimeSpan.FromHours(1));
                        lock (this)
                        {
                            var toRemove = new List<string>();
                            foreach (var entry in mEntries)
                            {
                                if (entry.Value.AddedOn.AddSeconds(entry.Value.TimeoutInSeconds) < DateTime.Now)
                                {
                                    toRemove.Add(entry.Key);
                                    return default(TValue);
                                }
                            }
                            foreach (var key in toRemove)
                            {
                                mEntries.Remove(key);
                            }
                        }

                    }
                },
                    TaskCreationOptions.LongRunning);
            }

            private Dictionary<string, Entry> mEntries;

            public void Clear()
            {
                lock (this)
                {
                    mEntries.Clear();
                }
            }

            public void Add(string key, TValue value)
            {
                Add(key, TimeSpan.FromDays(1), value);
            }

            public void Add(string key, TimeSpan timeout, TValue value)
            {
                if (key == null)
                    throw new InvalidOperationException("Key can't be null");

                lock (this)
                {
                    mEntries.Add(key,
                        new Entry()
                        {
                            AddedOn = DateTime.Now,
                            TimeoutInSeconds = (int) timeout.TotalSeconds,
                            Value = value
                        });
                }
            }


            public bool TryGetValue(string key, out TValue value)
            {
                value = Get(key);

                if (value == null)
                    return false;

                return true;

            }

            public TValue Get(string key)
            {
                if (key == null)
                    throw new InvalidOperationException("Key can't be null");

                lock (this)
                {
                    Entry res;
                    if (!mEntries.TryGetValue(key, out res))
                        return default(TValue);

                    if (res.AddedOn.AddSeconds(res.TimeoutInSeconds) < DateTime.Now)
                    {
                        mEntries.Remove(key);
                        return default(TValue);
                    }
                    return res.Value;
                }
            }

            private struct Entry
            {
                public DateTime AddedOn;
                public int TimeoutInSeconds;
                public TValue Value;
            }

        }

        #endregion
    }
}