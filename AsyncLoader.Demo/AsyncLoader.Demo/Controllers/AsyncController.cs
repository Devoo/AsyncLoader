using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace System.Web
{
    public class AsyncController : Controller
    {
        [Route("async/data/{id}/{extra?}")]
        public string Data(string id, string extra = null)
        {
            return AsyncLoader.AsyncData(id, extra);
        }


    }
}