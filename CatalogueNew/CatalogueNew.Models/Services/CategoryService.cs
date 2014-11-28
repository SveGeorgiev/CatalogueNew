﻿using CatalogueNew.Models.Entities;
using CatalogueNew.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueNew.Models.Services
{
    public class CategoryService : ICategoryService
    {
        private ICatalogueContext data;
        private readonly int pageSize = Int32.Parse(ConfigurationManager.AppSettings["PageSize"]);

        public CategoryService(ICatalogueContext data)
        {
            this.data = data;
        }

        public Category Find(int? id)
        {
            return data.Categories.Find(id);
        }

        public void Add(Category category)
        {
            data.Categories.Add(category);
            data.SaveChanges();
        }


        public void Modify(Category category)
        {
            data.Entry(category).State = EntityState.Modified;
            data.SaveChanges();
        }

        public void Remove(Category category)
        {
            data.Categories.Remove(category);
            data.SaveChanges();
        }

        public void Remove(int id)
        {
            var category = this.Find(id);
            data.Categories.Remove(category);
            data.SaveChanges();
        }

        public CategoryList GetCategories(int? page)
        {
            var categories = data.Categories;
            int pageNumber = page.GetValueOrDefault(1);
            var getCategories = categories.OrderBy(x => x.CategoryID).Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var pages = ((int)(Math.Ceiling((double)categories.Count() / pageSize)));

            var categoryList = new CategoryList()
            {
                Categories = getCategories,
                Count = pages
            };

            return categoryList;
        }
    }
}
