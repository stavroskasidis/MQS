using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MQS.Core
{
    /// <summary>
    /// A basic implemetation of IMessage that uses JSON to serialize and deserialize itself.
    /// </summary>
    public abstract class BasicMessage : IMessage
    {
        private JsonSerializerSettings settings;

        public BasicMessage()
        {
            settings = new JsonSerializerSettings();
            settings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
        }

        private string ToJson()
        {   
            return JsonConvert.SerializeObject(this, this.GetType(),Formatting.Indented,settings);
        }

        private void FromJson(string jsonString)
        {
            object clone = JsonConvert.DeserializeObject(jsonString,this.GetType(), settings);

            //if(parsedData==null || parsedData["Type"] == null || parsedData["Data"] == null)
            //{
            //    throw new Exception("Json string is invalid.");
            //}

            //if (parsedData["Type"].ToString() != this.GetType().Name)
            //{
            //    throw new Exception("Type missmatch. Expected type: '"+this.GetType().Name+"'");
            //}

            //object clone = JsonConvert.DeserializeObject((parsedData as Newtonsoft.Json.Linq.JObject)["Data"].ToString(), this.GetType(), settings);

            foreach (PropertyInfo prop in clone.GetType().GetProperties())
            {
                if (!prop.CanWrite)
                {
                    continue;
                }
                object value = prop.GetValue(clone, null);
                prop.SetValue(this, value, null);
            }
            foreach (FieldInfo field in clone.GetType().GetFields())
            {
                object value = field.GetValue(clone);
                field.SetValue(this, value);
            }
        }

        public string Serialize()
        {
            return this.ToJson();
        }

        public void Deserialize(string serializedMessage)
        {
            FromJson(serializedMessage);
        }
    }
}


//Dictionary<string, string> dict = new Dictionary<string, string>();
//IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties());
//foreach (PropertyInfo prop in props)
//{
//    string propName = prop.Name;

//    object propValue = prop.GetValue(this, null);
//    if (propValue != null && propValue.GetType().IsSubclassOf(typeof(Enum)))
//        dict.Add(propName, ((int)propValue).ToString());
//    else
//        dict.Add(propName, propValue != null ? propValue.ToString() : "");
//}