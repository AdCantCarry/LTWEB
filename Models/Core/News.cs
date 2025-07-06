using System;

namespace TechNova.Models.Core
{
    public class News
    {
        public int NewsId { get; set; }         // khóa chính
        public string Title { get; set; }
        public string Content { get; set; }     // mô tả dài
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublished { get; set; }   // đã xuất bản?
    }
}
