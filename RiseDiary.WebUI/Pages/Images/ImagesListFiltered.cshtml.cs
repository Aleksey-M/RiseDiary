﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Images
{
    public class ImagesListFilteredModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ImagesListFilteredModel(DiaryDbContext context)
        {
            _context = context;
        }        
        public class ImageListOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public async Task<JsonResult> OnGetAsync(int recordId, string namePart)
        {
            var images = !string.IsNullOrWhiteSpace(namePart)
                ? _context.Images.Where(i => !i.Deleted && i.Name.IndexOf(namePart, StringComparison.CurrentCultureIgnoreCase) > -1)
                : _context.Images.Where(i => !i.Deleted);

            images = recordId != 0
                ? images.Where(i => !_context.RecordImages.Where(ri => !ri.Deleted).Any(ri => ri.RecordId == recordId && ri.ImageId == i.Id))
                : images;
            
            var filteredImages = await images
                .OrderByDescending(i => i.CreateDate)
                .Take(10)
                .Select(i => new ImageListOption { Id = i.Id, Name = i.Name })
                .ToListAsync();
            
            return new JsonResult(filteredImages);
        }
    }
}