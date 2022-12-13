using CCBA.Integrations.DataTransformation.Models;
using System.Threading.Tasks;

namespace CCBA.Integrations.DataTransformation.Interfaces
{
    public interface IDataTransformationService
    {
        public Task<DataMappingResponse> GetDataMappingAsync(string sourceSystem, string targetSystem, string integrationName, string fieldName, string sourceValue);
    }
}