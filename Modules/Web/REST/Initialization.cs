﻿using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using Nyan.Core.Modules.Log;
using Nyan.Core.Settings;
using Nyan.Modules.Web.REST.auth;
using Nyan.Modules.Web.REST.formatters;

namespace Nyan.Modules.Web.REST
{
    public static class Initialization
    {
        public static string UrlPrefix { get { return "api"; } }

        public static string UrlPrefixRelative { get { return "~/api"; } }

        public static void Register(HttpConfiguration config)
        {
            // Force load of all Controllers:
            if (Current.WebApiCORSDomains != null)
            {
                var corsAttr = new EnableCorsAttribute(Current.WebApiCORSDomains, "*", "*") {SupportsCredentials = true};
                config.EnableCors(corsAttr);
            }

            config.SuppressHostPrincipal(); //Isolates WebApi Auth form Host (IIS) Auth

            config.Filters.Add(new NyanAuthenticationFilter());

            config.MapHttpAttributeRoutes(new CustomDirectRouteProvider());

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.Add(new XmlMediaTypeFormatter());
            config.Formatters.Add(new CsvMediaTypeFormatter());

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            //config.Formatters.Add(new CsvMediaTypeFormatter());


            Current.Log.Add("WebApi REST       : Routes registered.", Message.EContentType.Info);
        }
    }
}