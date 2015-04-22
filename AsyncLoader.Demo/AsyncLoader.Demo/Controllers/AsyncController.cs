using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace System.Web
{

    public class AsyncController : Controller
    {
        [Route("async/data/{id}")]
        public string Data(string id)
        {
            return AsyncLoader.AsyncData(id);
        }
    }
}