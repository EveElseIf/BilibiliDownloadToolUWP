using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Pages;
using FreeSql.DataAnnotations;

namespace BilibiliDownloadTool.Entities
{
    public class CompleteItem
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public BiliDownloadType Type { get; set; }
        public static explicit operator Complete(CompleteItem item) => new Complete()
        {
            Name = item.Name,
            Path = item.Path,
            Size = item.Size,
            Id = item.Id,
            Type = item.Type
        };
    }
}
