using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;

namespace template.comunicacion
{
    public class PetitionHTTP
    {
        private EnumMethods _enumsMethods = new EnumMethods();
        private HttpWebRequest _petition;
        private HttpWebResponse _httpWebResponse;
        private string _urlServer;
        public string urlServer
        {
            get { return _urlServer; }
            set { _urlServer = value; }
        }
        public PetitionHTTP(string _urlServer)
        {
            this._urlServer = _urlServer;
        }
        public HttpWebRequest loadPetition(string _controller, eMethodType _methodType, eContentType _contentType = eContentType.JSON, eAccept _accept = eAccept.JSON)
        {
            this._petition = (HttpWebRequest)WebRequest.Create(_urlServer + _controller);
            this._petition.ContentType = _enumsMethods.getContentType(_contentType);
            this._petition.Accept = _enumsMethods.getAccept(_accept);
            this._petition.Method = _enumsMethods.getMethodType(_methodType);
            return this._petition;
        }
        public void addBody(string _content)
        {
            using (StreamWriter _streamWriter = new StreamWriter(this._petition.GetRequestStream()))
            {
                _streamWriter.Write(_content);
                _streamWriter.Flush();
                _streamWriter.Close();
            }
        }
        public void addHeader(string _key, string _value)
        {
            this._petition.Headers.Add(_key, _value);
        }
        public string makePetition()
        {
            try
            {
                this._httpWebResponse = (HttpWebResponse)this._petition.GetResponse();
                Stream _responseStream = this._httpWebResponse.GetResponseStream();
                string _stringResponse = string.Empty;
                using (StreamReader _streamReader = new StreamReader(_responseStream))
                {
                    while (!_streamReader.EndOfStream)
                        _stringResponse += _streamReader.ReadLine();
                }
                return _stringResponse;
            }
            catch (Exception ex)
            {
                return (string)null;
            }
        }
        public IEnumerable<T> EjecutarStoredProcedure<T>(string storedProcedure,string conexion, SqlParameter[] parameters, Func<SqlDataReader, T> body)
        {
            List<T> results = new List<T>();
            SqlConnection connection = new SqlConnection(conexion);
            SqlCommand command = new SqlCommand(storedProcedure, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(body(reader));
                }
                reader.Close();
            connection.Close();
            return results;
        }
    }
}
