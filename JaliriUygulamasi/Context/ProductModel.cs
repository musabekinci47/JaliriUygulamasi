using System.ComponentModel.DataAnnotations;

namespace JaliriUygulamasi.Context
{
    public class ProductModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Barcode { get; set; }
        public string? Brand { get; set; }
    }


}
