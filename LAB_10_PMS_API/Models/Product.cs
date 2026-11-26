using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LAB_10_PMS_API.Models;

[Table("products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Image { get; set; } = string.Empty;

    public int Price { get; set; }

    public int Quantity { get; set; }

    [Required]
    [StringLength(150)]
    public string Status { get; set; } = "Active";
}