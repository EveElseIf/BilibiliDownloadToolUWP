using BilibiliDownloadTool.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BilibiliDownloadTool.Services
{
    public class SqliteData
    {
        private IFreeSql _freeSql;
        public async Task InitializeDb()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.db", CreationCollisionOption.OpenIfExists);
            _freeSql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, $"Data Source={file.Path}")
                .UseAutoSyncStructure(true)
                .Build();
        }
        public async Task<IList<CompleteItem>> GetCompleteItems()
        {
            var items = await _freeSql.Select<CompleteItem>().ToListAsync();
            return items;
        }
        public async Task<long> AddCompleteItem(CompleteItem complete)
        {
            return await _freeSql.Insert<CompleteItem>().AppendData(complete).ExecuteIdentityAsync();
        }
        public async Task DeleteCompleteItem(int id)
        {
            await _freeSql.Delete<CompleteItem>().Where(c => c.Id == id).ExecuteAffrowsAsync();
        }
    }
}
