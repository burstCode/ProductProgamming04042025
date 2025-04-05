using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductProgamming04042025.Pages.Models
{
    public class Article
    {
        public int ID { set; get; }
        [Required]
        [Column(TypeName = "varchar(200)")]  // ���������� ��� ����������
        public string Title { get; set; } = "";

        [Required]
        [Column(TypeName = "text")]  // ��� ������� �������
        public string Content { get; set; } = "";
    }
}