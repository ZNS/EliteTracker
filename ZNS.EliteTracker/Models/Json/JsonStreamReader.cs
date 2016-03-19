using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace ZNS.EliteTracker.Models.Json
{
    /// <summary>
    /// Disposable json stream deserializer
    /// </summary>
    public class JsonStreamReader<T> : IDisposable
    {
        JsonTextReader _Reader;
        JsonSerializer _Serializer;
        StreamReader _StreamReader;
        T _Current;

        public JsonStreamReader(string filePath)
        {
            _Serializer = new JsonSerializer();
            _StreamReader = new StreamReader(filePath);
            _Reader = new JsonTextReader(_StreamReader);
        }

        public T Current
        {
            get
            {
                return _Current;
            }
        }

        public bool Next()
        {
            if (_Reader.Read())
            {
                if (_Reader.TokenType == JsonToken.StartObject)
                {
                    _Current = _Serializer.Deserialize<T>(_Reader);
                    return true;
                }
                else
                {
                    return Next();
                }
            }
            return false;
        }

        void IDisposable.Dispose()
        {
            _Reader.Close();
            if (_StreamReader.BaseStream != null)
            {
                _StreamReader.BaseStream.Dispose();
            }
            _StreamReader.Dispose();
        }
    }
}