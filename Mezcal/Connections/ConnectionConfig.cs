using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal.Connections
{
    public class ConnectionConfig
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public string Type {get; set;}

        public ConnectionConfig(string url, string username, string password)
        {
            this.Url = url;
            this.UserName = username;
            this.Password = password;
        }

        public static ConnectionConfig GetConnectionConfig(JToken config)
        {
            string name = config["name"].ToString();
            string url = config["url"].ToString();
            string username = config["username"].ToString();
            string password = config["p"].ToString();
            bool isDefault = (config["default"] == null) ? false : bool.Parse(config["default"].ToString());
            string type = config["type"].ToString();

            var envConfig = new ConnectionConfig(url, username, password);
            envConfig.Name = name;
            envConfig.Type = type;
            envConfig.IsDefault = isDefault;

            return envConfig;
        }
    }
}
