using System;
using System.Threading.Tasks;

namespace EventSourcing.KSQL
{
    public class KsqlStreamCreator
    {
        private readonly KsqlQueryExecutor _ksqlQueryExecutor;

        public KsqlStreamCreator(KsqlQueryExecutor ksqlQueryExecutor) => _ksqlQueryExecutor = ksqlQueryExecutor;

        private async Task EnsureStreamExists(KsqlQuery streamOrTableCreateScript)
        {
            try
            {
                var response = await _ksqlQueryExecutor.ExecuteQuery(streamOrTableCreateScript, default, false);
                Console.WriteLine(response);
            }
            catch
            {
            }
        }

        public async Task EnsureStreamExists(params KsqlQuery[] streamOrTableCreateScripts)
        {
            foreach (var streamOrTableCreateScript in streamOrTableCreateScripts) await EnsureStreamExists(streamOrTableCreateScript);
        }
    }
}