using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Mod.Utility.Logging.Aws.Converters
{
    /// <summary>
    /// Exception Converter
    /// simply writes the .ToString form of the Exception, rather than serializing all of the Exception's properties
    /// deserialization is not supported
    /// </summary>
    public class ExceptionStringConverter : JsonConverter
    {
        /// <summary>
        /// we can convert exceptions
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Exception).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// we don't support reading
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// we do support writing
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// reading not implemented
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("reading not implemented");
        }

        /// <summary>
        /// Write stringified form of the Exception
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var exc = value as Exception;
            if (exc == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(exc.ToString());
            }
        }
    }
}
