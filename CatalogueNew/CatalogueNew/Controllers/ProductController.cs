﻿using CatalogueNew.Models.Entities;
using CatalogueNew.Models.Infrastructure;
using CatalogueNew.Models.Services;
using CatalogueNew.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CatalogueNew.Web.Controllers
{
    public class ProductController : Controller
    {
        private ICategoryService categoryService;
        private IManufacturerService manufacturerService;
        private IProductService productService;
        private IImageService imageService;

        public ProductController(ICategoryService categoryService,
            IManufacturerService manufacturerService, IProductService productService, IImageService imageService)
        {
            this.categoryService = categoryService;
            this.manufacturerService = manufacturerService;
            this.productService = productService;
            this.imageService = imageService;
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ProductAdministration(int page = 1)
        {
            PagedList<Product> pageItems = productService.GetProducts(page);
            var pagingViewModel = new PagingViewModel(pageItems.PageCount, pageItems.CurrentPage, "ProductAdministration");

            var productListViewModel = new ProductListViewModel()
            {
                Products = pageItems.Items.ToList(),
                PagingViewModel = pagingViewModel
            };

            return View(productListViewModel);
        }

        public ActionResult Details(int id)
        {
            Product product = productService.Find(id);

            if (product == null)
            {
                HttpContext.Response.StatusCode = 404;
                return View("_NotFound");
            }

            ProductViewModel model = new ProductViewModel()
            {
                Product = product
            };

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Delete(int id)
        {
            productService.Remove(id);
            return RedirectToAction("ProductAdministration");
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Create()
        {
            ProductViewModel model = new ProductViewModel();
            model.Categories = new SelectList(categoryService.GetAll(), "CategoryID", "Name");
            model.Manufacturers = new SelectList(manufacturerService.GetAll(), "ManufacturerID", "Name");
            model.Product = new Product();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Create(ProductViewModel model)
        {
            var images = new List<Image>();

            var path = Server.MapPath("~/Images/TempImages/");

            if (model.FileAttributesCollection != null)
            {
                foreach (var attributes in model.FileAttributesCollection)
                {
                    string[] FileAttributes = attributes.Split('\\');

                    images.Add(new Image()
                    {
                        Value = FileAttributes[0].GetFileData(path),
                        LastUpdated = DateTime.UtcNow,
                        ImageName = FileAttributes[1],
                        MimeType = FileAttributes[2]
                    });
                    try
                    {
                        System.IO.File.Delete(path + FileAttributes[0]);
                    }
                    catch
                    {
                        System.IO.File.Delete(path + FileAttributes[0]);
                    }
                }
            }

            foreach (var image in images)
            {
                image.ResizeImage();
            }

            model.Product.Images = images;

            productService.Add(model.Product);

            return RedirectToAction("Index", "Product");
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Edit(int id)
        {
            Product product = productService.Find(id);
            if (product == null)
            {
                HttpContext.Response.StatusCode = 404;
                return View("_NotFound");
            }

            ProductViewModel model = new ProductViewModel()
            {
                Product = product,
                Categories = new SelectList(categoryService.GetAll(), "CategoryID", "Name"),
                Manufacturers = new SelectList(manufacturerService.GetAll(), "ManufacturerID", "Name")
            };

            return View("Create", model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Edit(int id, ProductViewModel model)
        {
            if (ModelState.IsValid)
            {

                Product product = productService.Find(id);
                product.Name = model.Product.Name;
                product.CategoryID = model.Product.CategoryID;
                product.ManufacturerID = model.Product.ManufacturerID;
                product.Year = model.Product.Year;
                product.Description = model.Product.Description;
                var images = new List<Image>();

                var path = Server.MapPath("~/Images/TempImages/");

                if (model.FileAttributesCollection != null)
                {
                    foreach (var attributes in model.FileAttributesCollection)
                    {
                        string[] FileAttributes = attributes.Split('\\');

                        images.Add(new Image()
                        {
                            Value = FileAttributes[0].GetFileData(path),
                            LastUpdated = DateTime.UtcNow,
                            ImageName = FileAttributes[1],
                            MimeType = FileAttributes[2]
                        });
                        try
                        {
                            System.IO.File.Delete(path + FileAttributes[0]);
                        }
                        catch
                        {
                            System.IO.File.Delete(path + FileAttributes[0]);
                        }
                    }
                }

                foreach (var image in images)
                {
                    image.ResizeImage();
                    product.Images.Add(image);
                }

                productService.Modify(product);
            }
            return RedirectToAction("Index", "Product");
        }

        public ActionResult Index(int? category, int? manufacturer, int page = 1)
        {
            var pageItems = productService.GetProducts(page, category, manufacturer);
            var pagingViewModel = new PagingViewModel(pageItems.PageCount, pageItems.CurrentPage, "Index");

            var productListViewModels = new ProductListViewModel(pageItems.Items.ToList(), pagingViewModel);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RenderProductsPartial", productListViewModels);
            }

            return View(productListViewModels);
        }

        public ActionResult ManufacturersCategoriesSelectList()
        {
            var manufacturersList = new List<SelectListItem>();
            var categoriesList = new List<SelectListItem>();
            var manufacturers = manufacturerService.GetAll();
            var categories = categoryService.GetAll();

            foreach (var manufacturer in manufacturers)
            {
                manufacturersList.Add(new SelectListItem()
                {
                    Text = manufacturer.Name,
                    Value = manufacturer.ManufacturerID.ToString()
                });
            }

            foreach (var category in categories)
            {
                categoriesList.Add(new SelectListItem()
                {
                    Text = category.Name,
                    Value = category.CategoryID.ToString()
                });
            }

            var selectListItems = new SelectListViewModel(manufacturersList, categoriesList);

            return PartialView("_SelectListPartial", selectListItems);
        }

        public JsonResult SaveUploadedFile(HttpPostedFileBase file)
        {
            bool isSavedSuccessfully = true;
            string uniqueFileName = String.Empty;

            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\TempImages", Server.MapPath(@"\")));
                    string pathString = originalDirectory.ToString();
                    bool isExists = Directory.Exists(pathString);

                    if (!isExists)
                    {
                        System.IO.Directory.CreateDirectory(pathString);
                    }
                    uniqueFileName = Guid.NewGuid().ToString();
                    var path = string.Format("{0}\\{1}", pathString, uniqueFileName);

                    file.SaveAs(path);
                }
            }
            catch (Exception)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return new JsonResult()
                {
                    Data = new
                    {
                        UniqueName = uniqueFileName,
                        ImgName = file.FileName,
                        MimeType = file.ContentType
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public void RemoveImage(string value)
        {
            string[] FileAttributes = value.Split('\\');
            var path = Server.MapPath("~/Images/TempImages/");
            try
            {
                System.IO.File.Delete(path + FileAttributes[0]);
            }
            catch
            {
                System.IO.File.Delete(path + FileAttributes[0]);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public void RemoveImageById(string value)
        {
            imageService.Remove(Int32.Parse(value));
        }
    }
}