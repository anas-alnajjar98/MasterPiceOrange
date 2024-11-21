using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.BlogDtos
{
    public class BlogDto
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public string ImageUrl { get; set; }
        public string ApprovalStatus { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
    }
}
