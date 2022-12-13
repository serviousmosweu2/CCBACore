using CCBA.Integration.Core.DMF.Extensions.Services;
using System.IO;
using System.Net;

namespace CCBA.Infinity
{
    internal class Result : IResult
    {
        private Stream _stream;
        private HttpStatusCode _status;
        private string _exportFile;

        public Result(Stream stream, HttpStatusCode status, string exportFile)
        {
            _stream = stream;
            _status = status;
            _exportFile = exportFile;
        }

        public Stream ResultStream => _stream;

        public HttpStatusCode Status => _status;

        public string ExportFile => _exportFile;
    }
}