using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlbumFotos.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AlbumFotos.Controllers
{
    public class AlbunsController : Controller
    {
        private readonly Contexto _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AlbunsController(Contexto context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Albuns
        public async Task<IActionResult> Index()
        {
            return View(await _context.Albuns.ToListAsync());
        }

        // GET: Albuns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albuns
                .FirstOrDefaultAsync(m => m.AlbumId == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albuns/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Albuns/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlbumId,Destino,FotoTopo,Inicio,Fim")] Album album, IFormFile arquivo)
        {
            if (ModelState.IsValid)
            {
                var linkUpload = Path.Combine(_hostingEnvironment.WebRootPath, "Imagens");

                if(arquivo != null)
                {
                    using (var fileStream = new FileStream(Path.Combine(linkUpload, arquivo.FileName), FileMode.Create))
                    {
                        await arquivo.CopyToAsync(fileStream);
                        album.FotoTopo = "~/Imagens/" + arquivo.FileName;
                    }
                }

                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(album);
        }

        // GET: Albuns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albuns.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            TempData["FotoTopo"] = album.FotoTopo;

            return View(album);
        }

        // POST: Albuns/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,Destino,FotoTopo,Inicio,Fim")] Album album, IFormFile arquivo)
        {
            if (id != album.AlbumId)
            {
                return NotFound();
            }

            if (String.IsNullOrEmpty(album.FotoTopo))
                album.FotoTopo = TempData["FotoTopo"].ToString();

            if (ModelState.IsValid)
            {
                try
                {
                    var linkUpload = Path.Combine(_hostingEnvironment.WebRootPath, "Imagens");

                    if(arquivo != null)
                    {
                        using (var fileStream = new FileStream(Path.Combine(linkUpload, arquivo.FileName), FileMode.Create))
                        {
                            await arquivo.CopyToAsync(fileStream);
                            album.FotoTopo = "~/Imagens/" + arquivo.FileName;
                        }
                    }

                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.AlbumId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(album);
        }        

        // POST: Albuns/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int AlbumId)
        {
            var album = await _context.Albuns.FindAsync(AlbumId);
            IEnumerable<string> links = _context.Imagens.Where(i => i.AlbumId == AlbumId).Select(i => i.Link);

            foreach(var link in links)
            {
                var linkImagem = link.Replace("~", "wwwroot");
                System.IO.File.Delete(linkImagem);
            }

            _context.Imagens.RemoveRange(_context.Imagens.Where(x => x.AlbumId == AlbumId));

            string linkFotoAlbum = album.FotoTopo;
            linkFotoAlbum = linkFotoAlbum.Replace("~", "wwwroot");
            System.IO.File.Delete(linkFotoAlbum);
            _context.Albuns.Remove(album);
            await _context.SaveChangesAsync();
            return Json("Album excluído com sucesso");
        }

        private bool AlbumExists(int id)
        {
            return _context.Albuns.Any(e => e.AlbumId == id);
        }
    }
}
