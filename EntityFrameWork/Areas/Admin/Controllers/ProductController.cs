using EntityFrameWork.Areas.Admin.View_Models;
using EntityFrameWork.Data;
using EntityFrameWork.Helpers;
using EntityFrameWork.Models;
using EntityFrameWork.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace EntityFrameWork.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly AppDbContext _context;

    public ProductController(IProductService productService,ICategoryService categoryService, IWebHostEnvironment webHostEnvironment, AppDbContext context)
    {
        _productService = productService;
        _categoryService = categoryService;
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1, int take = 2)
    {

        List<Product> products = await _productService.GetPaginatedDatas(page, take);

        List<ProductListVM> mappedDatas = GetMappedDatas(products);

        int pageCount = await GetPageCountAsync(take);

        Paginate<ProductListVM> paginatedDatas = new(mappedDatas, page, pageCount);

        ViewBag.Take = (take);

        return View(paginatedDatas);
    }


    private List<ProductListVM> GetMappedDatas(List<Product> products)
    {
        List<ProductListVM> mappedDatas = new();

        foreach (var product in products)
        {

            ProductListVM productVm = new()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Count = product.Count,
                Description = product.Description,
                CategoryName = product.Category.Name,
                MainImage = product.Images.Where(m => m.IsMain).FirstOrDefault()?.Image

            };

            mappedDatas.Add(productVm);
        }

        return mappedDatas;
    }


    private async Task<int> GetPageCountAsync(int take)
    {
        var product = await _productService.GetCountAsync();

        return (int)Math.Ceiling((decimal)product / take);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        IEnumerable<Category> categories = await _categoryService.GetAll();

        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateVM model)
    {

        try
        {
            IEnumerable<Category> categories = await _categoryService.GetAll();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            foreach (var photo in model.Photos)
            {
                if (!photo.ContentType.Contains("image/"))  // Typesinin image olb olmadiqini yoxlayur 
                {
                    ModelState.AddModelError("Photo", "File type must be image");

                    return View();

                }
      
            }


            List<ProductImage> productImages = new();

            foreach (var photo in model.Photos)
            {
                string fileName = Guid.NewGuid().ToString() + " " + photo.FileName; // herdefe yeni ad duzeldirik . 

                string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName); // root duzeldirik . 

                using (FileStream stream = new FileStream(path, FileMode.Create)) // Kompa sekil yuklemek ucun muhit yaradiriq stream yaradiriq 
                {
                    await photo.CopyToAsync(stream);
                }

                ProductImage image = new ProductImage()
                {
                    Image = fileName
                };

                productImages.Add(image);

                
                productImages.FirstOrDefault().IsMain = true;

                decimal convertedPrice = decimal.Parse(model.Price.Replace(".", ","));

                Product newProduct = new()
                {
                    Name = model.Name,
                    Price=convertedPrice,
                    Count=model.Count,
                    Description=model.Description,
                    CategoryId=model.CategoryId,
                    Images=productImages
                    
                };

                await _context.ProductImages.AddRangeAsync(productImages);

                await _context.Products.AddAsync(newProduct);

                await _context.SaveChangesAsync();
               
            }
            return RedirectToAction(nameof(Index));

        }
        catch (Exception)
        {

            throw;
        }

       
    }


    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {

        if (id == null) return BadRequest();
        
        Product product = await _productService.GetFullDataById((int)id);

        if (product == null) return NotFound();

        ViewBag.Desc = Regex.Replace(product.Description, "<.*?>", String.Empty);

        return View(product);

    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteProduct(int? id)
    {
        Product product = await _productService.GetFullDataById((int)id);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));

    }

    private async Task<SelectList> GetCategoriesAsync()
    {
        IEnumerable<Category> categories = await _categoryService.GetAll(); 
        return new SelectList(categories, "Id", "Name");
    }



    [HttpGet]
    public async Task<IActionResult> Detail(int? id)
    {
        var product = await _productService.GetFullDataById((int)id);
        return View(product);
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return BadRequest();
        var dbProduct = await _productService.GetFullDataById((int)id);
        if(dbProduct == null) return NotFound();
        ViewBag.categories = await GetCategoriesAsync();


        ProductUpdateVM model = new()
        {
            Images = dbProduct.Images,
            Name = dbProduct.Name,
            Description = dbProduct.Description,
            Price = dbProduct.Price,
            Count = dbProduct.Count,
            CategoryId = dbProduct.CategoryId,

        };

        return View(model);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ProductUpdateVM model)
    {


        try
        {
            if (id == null) return BadRequest();
            Product dbProduct = await _productService.GetFullDataById((int)id);
            if (dbProduct == null) return NotFound();
            ViewBag.categories = await GetCategoriesAsync();

            ProductUpdateVM newProduct = new()
            {
                Images = dbProduct.Images,
                Name = dbProduct.Name,
                Description = dbProduct.Description,
                Price = dbProduct.Price,
                Count = dbProduct.Count,
                CategoryId = dbProduct.CategoryId,
            };


            if (!ModelState.IsValid)
            {
                return View(newProduct);
            }

            if (model.Photos != null)
            {
                foreach (var item in model.Photos)
                {
                    if (!item.ContentType.Contains("image/"))  
                    {
                        ModelState.AddModelError("Photo", "File type must be image");

                        return View(newProduct);

                    }
                }

                foreach (var item in dbProduct.Images)
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", item.Image);


                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                List<ProductImage> productImages = new();

                foreach (var item in model.Photos)
                {

                    string fileName = Guid.NewGuid().ToString() + " " + item.FileName;
                    string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);
                    using (FileStream stream = new FileStream(newPath, FileMode.Create)) 
                    {
                        await item.CopyToAsync(stream);
                    }

                    ProductImage newProductImage = new()
                    {
                        Image = fileName
                    };
                    productImages.Add(newProductImage);
                }
                productImages.FirstOrDefault().IsMain = true;
                _context.ProductImages.AddRange(productImages);
                dbProduct.Images = productImages;
            }
            else
            {
                {
                    Product proda = new()
                    {
                        Images = dbProduct.Images
                    };
                }

               
            }
            dbProduct.Name = model.Name;
            dbProduct.Price = model.Price;
            dbProduct.Count = model.Count;
            dbProduct.Description = model.Description;
            dbProduct.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {

            throw;
        }

       


    }








}




    

