using System;
using System.IO;
using System.Linq;
using RocksDbSharp;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public static class RocksDbFactory
    {
        internal static RocksDbSharp.RocksDb GetDatabase(string databasePath, SliceTransform prefixTransform)
        {
            var columnFamilies = new ColumnFamilies();

            if (File.Exists(databasePath) || Directory.Exists(databasePath))
            {
                var currentColumnFamilies = RocksDbSharp.RocksDb.ListColumnFamilies(new DbOptions(), databasePath);

                foreach (var columnFamily in currentColumnFamilies.ToHashSet())
                    columnFamilies.Add(columnFamily,
                        new ColumnFamilyOptions()
                            .SetPrefixExtractor(prefixTransform)
                            .SetBlockBasedTableFactory(new BlockBasedTableOptions()
                                .SetWholeKeyFiltering(true)
                                .SetIndexType(BlockBasedTableIndexType.Binary)));
            }

            var options = Native.Instance.rocksdb_options_create();
            Native.Instance.rocksdb_options_increase_parallelism(options, Environment.ProcessorCount);
            Native.Instance.rocksdb_options_optimize_level_style_compaction(options, 0);
            Native.Instance.rocksdb_options_set_create_if_missing(options, true);
            Native.Instance.rocksdb_cache_create_lru(UIntPtr.Zero);

            return RocksDbSharp.RocksDb.Open(new DbOptions()
                    .SetCreateIfMissing()
                    .SetCreateMissingColumnFamilies()
                    .IncreaseParallelism(10),
                databasePath,
                columnFamilies);
        }
    }
}