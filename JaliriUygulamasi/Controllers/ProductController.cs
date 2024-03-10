using AutoMapper;
using Elasticsearch.Net;
using JaliriUygulamasi.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;

namespace JaliriUygulamasi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMapper _mapper;
        public ProductController(IMapper mapper)
        {
            _mapper = mapper;
        }

        AppDbContext context = new();

        [HttpGet("[action]")]
        public async Task<IActionResult> CreateData()
        {
            IList<Product> products = new List<Product>();
            var random = new Random();

            List<string> Colors = new List<string>()
            {
                "Black",
                "White",
                "Red",
                "Green",
                "Blue"
            };

            List<string> Sizes = new List<string>()
            {
                "Small",
                "Medium",
                "Large",
                "XLarge"
            };

            List<string> Brands = new List<string>()
            {
                "Jaliri",
                "Nike",
                "Adidas",
                "Lacoste"
            };

            for (int i = 0; i < 50; i++)
            {
                string name = new string(Enumerable.Repeat("abcdefgðhýijklmnoöprsþtuwyz", 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                string color = Colors[random.Next(5)];
                string size = Sizes[random.Next(4)];
                string barcode = new string(Enumerable.Repeat("abcdefgðhýijklmnoöprsþtuwyz0123456789", 10)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                string brand = Brands[random.Next(4)];
                var product = new Product()
                {
                    Name = name,
                    Color = color,
                    Size = size,
                    Barcode = barcode,
                    Brand = brand
                };
                products.Add(product);
            }

            await context.Set<Product>().AddRangeAsync(products);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetDataListWithEntityFramework(ProductModel product)
        {
            var products = context.Set<Product>().Where(x =>
            x.Name.Contains(product.Name)
            && x.Color.Contains(product.Color)
            && x.Size.Contains(product.Size)
            && x.Barcode.Contains(product.Barcode)
            && x.Brand.Contains(product.Brand)
            ).AsNoTracking().ToList();
            return Ok(products);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetDataListWithElasticSearch(ProductModel product)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("products");

            var client = new ElasticClient(settings);

            var searchResponse = client.Search<Product>(s => s
                .Query(q => q
                      .Bool(b => b
                        .Must(mu =>
                        {
                            QueryContainer queryContainer = new QueryContainer();
                            #region Case 1
                            List<string> fields = new List<string>()
                            {
                                "Name",
                                "Color",
                                "Size",
                                "Barcode",
                                "Brand"
                            };
                            foreach (var item in fields)
                            {
                                string? strVal = product.GetType().GetProperty(item).GetValue(product).ToString().ToLower();
                                if (!string.IsNullOrEmpty(strVal))
                                {
                                    queryContainer = queryContainer && new WildcardQuery
                                    {
                                        Field = item.ToLower(),
                                        Value = $"*{strVal}*"
                                    };
                                }
                            }
                            #endregion
                            #region Case 2
                            //if (!string.IsNullOrEmpty(product.Name))
                            //{
                            //    queryContainer = queryContainer && new QueryContainerDescriptor<Product>().Wildcard(w => w
                            //    .Field(f => f.Name)
                            //    .Value($"*{product.Name.ToLower()}*"));
                            //}
                            //if (!string.IsNullOrEmpty(product.Color))
                            //{
                            //    queryContainer = queryContainer && new QueryContainerDescriptor<Product>().Wildcard(w => w
                            //    .Field(f => f.Color)
                            //    .Value($"*{product.Color.ToLower()}*"));
                            //}
                            //if (!string.IsNullOrEmpty(product.Size))
                            //{
                            //    queryContainer = queryContainer && new QueryContainerDescriptor<Product>().Wildcard(w => w
                            //    .Field(f => f.Size)
                            //    .Value($"*{product.Size.ToLower()}*"));
                            //}
                            //if (!string.IsNullOrEmpty(product.Barcode))
                            //{
                            //    queryContainer = queryContainer && new QueryContainerDescriptor<Product>().Wildcard(w => w
                            //    .Field(f => f.Barcode)
                            //    .Value($"*{product.Barcode.ToLower()}*"));
                            //}
                            //if (!string.IsNullOrEmpty(product.Brand))
                            //{
                            //    queryContainer = queryContainer && new QueryContainerDescriptor<Product>().Wildcard(w => w
                            //    .Field(f => f.Brand)
                            //    .Value($"*{product.Brand.ToLower()}*"));
                            //}
                            #endregion

                            return queryContainer;
                        })
                    )
                ).Sort(sort => sort
                .Ascending(doc => doc.Id))
            );
            List<Product> products = new List<Product>();
            foreach (var item in searchResponse.Hits.ToList()) products.Add((Product)item.Source);
            return Ok(products);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Add(ProductModel product)
        {
            Product entity = _mapper.Map<Product>(product);
            context.Set<Product>().Add(entity);
            context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPatch("[action]")]
        public async Task<IActionResult> Update(ProductModel product)
        {
            Product current = context.Set<Product>().AsNoTracking().FirstOrDefault(x => x.Id == product.Id);
            current = _mapper.Map<Product>(product);
            context.Update(current);
            context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Product product = context.Products.AsNoTracking().FirstOrDefault(x => x.Id == id);
            context.Products.Remove(product);
            context.SaveChangesAsync();
            return Ok(product);
        }
    }
}