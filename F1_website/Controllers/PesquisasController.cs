using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F1_website.Models;

namespace F1_website.Controllers
{
    public class PesquisasController : Controller
    {
        public JsonResult GetPaginas()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var paginas = db.paginas.Select(p => new Pesquisas
                {
                    titulo = p.titulo,
                    caminho = p.caminho
                }).ToList();

                return Json(paginas, JsonRequestBehavior.AllowGet);
            }
        }
    }
}