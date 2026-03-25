using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuanLyDoanVien.Filters;

namespace QuanLyDoanVien
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // JSON settings
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss"
            };
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/json"));
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Global authorize filter
            config.Filters.Add(new ApiAuthorizeAttribute());

            // CORS (same origin, but configure for flexibility)
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Attribute routing first
            config.MapHttpAttributeRoutes();

            // Convention routes
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

