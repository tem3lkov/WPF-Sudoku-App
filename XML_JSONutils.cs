using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
namespace StudentXMLSerialization
{
   static  class XMLutils
    {
        static T DeserializeXml<T>(string sourceXML) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            T result = null;

            using (TextReader reader = new StringReader(sourceXML))
            {
                result = (T)serializer.Deserialize(reader)!;
            }

            return result;
        }
        static List<T>? DeserializeJSON<T>(string sourceXML) where T : class
        {
            List<T> result = default(List<T>)!;

            using (StreamReader reader = new StreamReader(sourceXML))
            {
                try
                {
                   result = JsonSerializer.Deserialize<List<T>>(reader.ReadToEnd())!;
                    
                }
                catch (JsonException e)
                {
                    Console.WriteLine(  e.Message);
                }
            }  
            return result;  
        }
        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }
        public static string JSONSerializeToString(this object objectInstance)
        {
            var objType = objectInstance.GetType();
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                 sb.Append(JsonSerializer.Serialize(objectInstance));
            }

            return sb.ToString();
        }
        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }
        public static T JSONDeserializeFromString<T>(this string objectData) where T : class
        {
   
            object? result =null;

            using (TextReader reader = new StringReader(objectData))
            {
                
                result = JsonSerializer.Deserialize<T>(objectData)!;  
            }

            return (T) result;
        }

        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }
}