using System.ComponentModel.DataAnnotations;

namespace JaliriUygulamasi.Context
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string? Color { get; set; }
        [MaxLength(50)]
        public string? Size { get; set; }
        [MaxLength(50)]
        public string? Barcode { get; set; }
        [MaxLength(50)]
        public string? Brand { get; set; }
    }

}
