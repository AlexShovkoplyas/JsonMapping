using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonFun
{
    class Program
    {
        private static JToken input;

        static void Main(string[] args)
        {
            //Test();
            var inputAsString = File.ReadAllText(@"input.json");
            input = JToken.Parse(inputAsString);

            var schemaAsString = File.ReadAllText(@"schema.json");
            JSchema schema = JSchema.Parse(schemaAsString);

            var output = Generate(schema);

            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }

        public static JToken Generate(JSchema schema, params int[] indices)
        {
            switch (schema.Type)
            {
                case JSchemaType.Object:
                    var jObject = new JObject();
                    if (schema.Properties != null)
                    {
                        foreach (var prop in schema.Properties)
                        {
                            jObject.Add(prop.Key, Generate(prop.Value, indices));
                        }
                    }
                    return jObject;

                case JSchemaType.Array:
                    var jArray = new JArray();
                    for (int i = 0; i < ArrayCount(schema); i++)
                    {
                        jArray.Add(Generate(schema.Items[0], indices.Concat(new[] { i }).ToArray()));
                    }
                    return jArray;

                case JSchemaType.String:
                    return new JValue((string)Transform(schema, indices));

                case JSchemaType.Number:
                    return new JValue((double)Transform(schema, indices));

                default:
                    return null;
            }
        }

        private static int ArrayCount(JSchema schema)
        {
            var jsonPath = (string) schema.Items[0].ExtensionData["transform"];
            return input.SelectTokens(jsonPath).Count();
        }

        static JToken Transform(JSchema schema, params int[] indices)
        {
            var jsonPath = (string)schema.ExtensionData["transform"];
            if (indices.Length > 0)
            {
                foreach (var index in indices)
                {
                    jsonPath = jsonPath.ReplaceFirst("[*]", $"[{index}]");
                }
            }

            return input.SelectToken(jsonPath);
        }
    }
}
