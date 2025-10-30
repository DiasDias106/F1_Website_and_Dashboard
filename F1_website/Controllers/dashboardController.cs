using ClosedXML.Excel;
using F1_website.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace F1_website.Controllers
{
    public class dashboardController : Controller
    {
        public ActionResult ListaEquipas(string msg, string ordem, string seek)
        {
            ViewBag.Msg = msg;
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                List<Equipa> equipas;
                try
                {
                    equipas = db.Equipa.ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.Erro = ex.Message;
                    equipas = new List<Equipa>();
                }

                if (!string.IsNullOrEmpty(seek))
                {
                    equipas = equipas.Where(e =>
                        (!string.IsNullOrEmpty(e.nome) && e.nome.Contains(seek)) ||
                        (!string.IsNullOrEmpty(e.descricao) && e.descricao.Contains(seek)) ||
                        (!string.IsNullOrEmpty(e.dono) && e.dono.Contains(seek))
                    ).ToList();
                }

                switch (ordem)
                {
                    case "idequipadesc":
                        equipas = equipas.OrderByDescending(e => e.id_equipa).ToList();
                        break;
                    case "nomeasc":
                        equipas = equipas.OrderBy(e => e.nome ?? "").ToList();
                        break;
                    case "nomedesc":
                        equipas = equipas.OrderByDescending(e => e.nome ?? "").ToList();
                        break;
                    case "titulosasc":
                        equipas = equipas.OrderBy(e => e.titulos ?? 0).ToList();
                        break;
                    case "titulosdesc":
                        equipas = equipas.OrderByDescending(e => e.titulos ?? 0).ToList();
                        break;
                    default:
                        equipas = equipas.OrderBy(e => e.id_equipa).ToList();
                        break;
                }

                ViewBag.ordemId = string.IsNullOrEmpty(ordem) ? "idequipadesc" : "";
                ViewBag.ordemNome = ordem == "nomeasc" ? "nomedesc" : "nomeasc";
                ViewBag.ordemTitulos = ordem == "titulosasc" ? "titulosdesc" : "titulosasc";

                // Criar ViewModel
                var viewModel = new paginacomunicaçãoviewmodel
                {
                    EquipasEF = equipas ?? new List<Equipa>(),
                    Ordem = ordem,
                    Seek = seek
                };
                return View(viewModel);
            }
        }

        [HttpGet]
        public ActionResult AddEquipa()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEquipa(Equipa equipa, HttpPostedFileBase imagemFile)
        {
            if (ModelState.IsValid)
            {
                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    try
                    {
                        // Processar upload de imagem se fornecido
                        if (imagemFile != null && imagemFile.ContentLength > 0)
                        {
                            string imagemNome = ProcessarUploadImagem(imagemFile);
                            if (imagemNome == null)
                            {
                                return View(equipa);
                            }
                            equipa.imagem = imagemNome;
                        }

                        // Adicionar a nova equipa à base de dados
                        db.Equipa.Add(equipa);
                        db.SaveChanges();

                        // Redirecionar para a lista com mensagem de sucesso
                        return RedirectToAction("ListaEquipas", new { msg = "Equipa adicionada com sucesso!" });
                    }
                    catch (Exception ex)
                    {
                        // Em caso de erro, mostrar mensagem de erro
                        ViewBag.Erro = "Erro ao adicionar equipa: " + ex.Message;
                        return View(equipa);
                    }
                }
            }

            // Se o modelo não é válido, retornar à view com os dados preenchidos
            return View(equipa);
        }

        [HttpGet]
        public ActionResult EditarEquipa(int id)
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var equipa = db.Equipa.Find(id);
                if (equipa == null)
                {
                    return HttpNotFound();
                }

                return View(equipa);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEquipa(Equipa equipa, HttpPostedFileBase imagemFile)
        {
            if (ModelState.IsValid)
            {
                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    try
                    {
                        // Obter equipa existente para preservar imagem anterior se necessário
                        var equipaExistente = db.Equipa.Find(equipa.id_equipa);
                        if (equipaExistente == null)
                        {
                            return HttpNotFound();
                        }

                        // Processar novo upload se fornecido
                        if (imagemFile != null && imagemFile.ContentLength > 0)
                        {
                            string novaImagem = ProcessarUploadImagem(imagemFile);
                            if (novaImagem == null)
                            {
                                // Erro já foi definido no ViewBag dentro do método
                                return View(equipa);
                            }

                            // Eliminar imagem anterior se existir
                            EliminarImagemAnterior(equipaExistente.imagem);

                            equipa.imagem = novaImagem;
                        }
                        else
                        {
                            // Se não há novo upload, manter imagem existente
                            equipa.imagem = equipaExistente.imagem;
                        }

                        // Atualizar apenas os campos necessários
                        db.Entry(equipaExistente).CurrentValues.SetValues(equipa);
                        db.SaveChanges();

                        return RedirectToAction("ListaEquipas", new { msg = "Equipa atualizada com sucesso!" });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Erro = "Erro ao editar equipa: " + ex.Message;
                        return View(equipa);
                    }
                }
            }
            return View(equipa);
        }

        public ActionResult DetailEquipa(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaEquipas", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var equipa = db.Equipa.FirstOrDefault(e => e.id_equipa == id);
                if (equipa == null)
                    return RedirectToAction("ListaEquipas", new { msg = "Equipa não encontrada" });

                var componentesDaEquipa = db.Componente
                    .Where(c => c.equipa_id == id)
                    .ToList();

                var todasEquipas = db.Equipa.ToList();

                var viewModel = new paginacomunicaçãoviewmodel
                {
                    Equipa = equipa,
                    ComponentesEF = componentesDaEquipa ?? new List<Componente>(),
                    EquipasEF = todasEquipas ?? new List<Equipa>()
                };

                return View(viewModel);
            }
        }

        public ActionResult DeleteEquipa(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaEquipas", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var equipa = db.Equipa.FirstOrDefault(e => e.id_equipa == id);
                if (equipa == null)
                    return RedirectToAction("ListaEquipas", new { msg = "Equipa não encontrada" });

                var vm = new paginacomunicaçãoviewmodel { Equipa = equipa };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEquipa(int id_equipa)
        {
            using (var db = new F1DBSiteEntities1())
            {
                var equipa = db.Equipa.Find(id_equipa);
                if (equipa == null)
                    return RedirectToAction("ListaEquipas", new { msg = "Equipa não encontrada" });

                // Eliminar imagem associada antes de apagar a equipa
                EliminarImagemAnterior(equipa.imagem);

                db.Equipa.Remove(equipa);
                db.SaveChanges();
            }
            return RedirectToAction("ListaEquipas", new { msg = "Equipa apagada com sucesso" });
        }

        // Método auxiliar para processar upload de imagens
        private string ProcessarUploadImagem(HttpPostedFileBase imagemFile)
        {
            try
            {
                // Validar tipo de ficheiro
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var fileExtension = Path.GetExtension(imagemFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ViewBag.Erro = "Tipo de ficheiro não permitido. Use JPG, JPEG, PNG, GIF ou BMP.";
                    return null;
                }

                // Validar tamanho do ficheiro (máximo 5MB)
                if (imagemFile.ContentLength > 5 * 1024 * 1024)
                {
                    ViewBag.Erro = "O ficheiro é demasiado grande. Tamanho máximo: 5MB.";
                    return null;
                }

                // Criar nome único para o ficheiro
                string fileName = Path.GetFileNameWithoutExtension(imagemFile.FileName);
                string extension = Path.GetExtension(imagemFile.FileName);

                // Limitar o tamanho do nome original e criar nome único
                if (fileName.Length > 50)
                {
                    fileName = fileName.Substring(0, 50);
                }

                string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}_{fileName}{extension}";
                string uploadPath = Server.MapPath("~/imagens/ImagensEquipas/");

                // Criar diretório se não existir
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Caminho completo do ficheiro
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                // Verificar se o ficheiro já existe (improvável mas possível)
                int counter = 1;
                string originalFileName = uniqueFileName;
                while (System.IO.File.Exists(filePath))
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                    string ext = Path.GetExtension(originalFileName);
                    uniqueFileName = $"{nameWithoutExt}_{counter}{ext}";
                    filePath = Path.Combine(uploadPath, uniqueFileName);
                    counter++;
                }

                // Guardar ficheiro
                imagemFile.SaveAs(filePath);

                // Verificar se o ficheiro foi realmente guardado
                if (System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Imagem guardada com sucesso: {filePath}");
                    return uniqueFileName;
                }
                else
                {
                    ViewBag.Erro = "Falha ao guardar o ficheiro no servidor.";
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar upload: {ex.Message}");
                ViewBag.Erro = "Erro ao processar upload da imagem: " + ex.Message;
                return null;
            }
        }

        // Método auxiliar para eliminar imagem anterior
        private void EliminarImagemAnterior(string nomeImagem)
        {
            if (!string.IsNullOrEmpty(nomeImagem))
            {
                try
                {
                    string imagePath = Server.MapPath("~/imagens/ImagensEquipas/" + nomeImagem);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        System.Diagnostics.Debug.WriteLine($"Imagem anterior eliminada: {imagePath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao eliminar imagem anterior: {ex.Message}");
                    // Não falha a operação principal se não conseguir eliminar a imagem anterior
                }
            }
        }

        public ActionResult ListaPilotos(string msg, string ordem, string seek)
        {
            ViewBag.Msg = msg;

            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                List<Piloto> pilotos;
                List<Equipa> equipas;

                try
                {
                    pilotos = db.Piloto.Include("Equipa").ToList();
                    equipas = db.Equipa.ToList(); // Preenche lista de equipas para dropdown na view
                }
                catch (Exception ex)
                {
                    ViewBag.Erro = ex.Message;
                    pilotos = new List<Piloto>();
                    equipas = new List<Equipa>();
                }

                // Aplicar filtro aos pilotos
                if (!string.IsNullOrEmpty(seek))
                {
                    pilotos = pilotos.Where(p =>
                        (!string.IsNullOrEmpty(p.nome) && p.nome.Contains(seek)) ||
                        (!string.IsNullOrEmpty(p.nacionalidade) && p.nacionalidade.Contains(seek)) ||
                        (p.Equipa != null && !string.IsNullOrEmpty(p.Equipa.nome) && p.Equipa.nome.Contains(seek))
                    ).ToList();
                }

                // Aplicar ordenação
                switch (ordem)
                {
                    case "idpilotodesc":
                        pilotos = pilotos.OrderByDescending(p => p.Id_piloto).ToList();
                        break;
                    case "nomeasc":
                        pilotos = pilotos.OrderBy(p => p.nome ?? "").ToList();
                        break;
                    case "nomedesc":
                        pilotos = pilotos.OrderByDescending(p => p.nome ?? "").ToList();
                        break;
                    case "nacionalidadeasc":
                        pilotos = pilotos.OrderBy(p => p.nacionalidade ?? "").ToList();
                        break;
                    case "nacionalidadedesc":
                        pilotos = pilotos.OrderByDescending(p => p.nacionalidade ?? "").ToList();
                        break;
                    case "equipanomeasc":
                        pilotos = pilotos.OrderBy(p => p.Equipa?.nome ?? "").ToList();
                        break;
                    case "equipanomedesc":
                        pilotos = pilotos.OrderByDescending(p => p.Equipa?.nome ?? "").ToList();
                        break;
                    default:
                        pilotos = pilotos.OrderBy(p => p.Id_piloto).ToList();
                        break;
                }

                // Criar ViewModel e preencher listas
                var model = new paginacomunicaçãoviewmodel
                {
                    PilotoEF = pilotos,
                    EquipasEF = equipas
                };

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult AddPiloto()
        {
            var viewModel = new paginacomunicaçãoviewmodel();

            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                viewModel.Piloto = new Piloto();
                viewModel.EquipasDrop = db.Equipa
                    .OrderBy(e => e.nome)
                    .Select(e => new SelectListItem
                    {
                        Value = e.id_equipa.ToString(),
                        Text = e.nome
                    }).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPiloto(paginacomunicaçãoviewmodel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    try
                    {
                        db.Piloto.Add(viewModel.Piloto);
                        db.SaveChanges();
                        return RedirectToAction("ListaPilotos", new { msg = "Piloto adicionado com sucesso!" });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Erro = "Erro ao adicionar piloto: " + ex.Message;
                    }
                }
            }

            // Se ModelState inválido ou erro, recarrega EquipasDrop para manter o dropdown preenchido
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                viewModel.EquipasDrop = db.Equipa
                    .OrderBy(e => e.nome)
                    .Select(e => new SelectListItem
                    {
                        Value = e.id_equipa.ToString(),
                        Text = e.nome
                    }).ToList();
            }

            return View(viewModel);
        }



        [HttpGet]
        public ActionResult EditarPiloto(int id)
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var piloto = db.Piloto.Find(id);
                if (piloto == null)
                {
                    return HttpNotFound();
                }

                // Lista de equipas para dropdown
                ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", piloto.equipa_id);

                return View(piloto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPiloto(Piloto piloto)
        {
            if (ModelState.IsValid)
            {
                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    try
                    {
                        db.Entry(piloto).State = EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("ListaPilotos", new { msg = "Piloto atualizado com sucesso!" });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Erro = "Erro ao editar piloto: " + ex.Message;
                        ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", piloto.equipa_id);
                        return View(piloto);
                    }
                }
            }

            // Modelo inválido, recarregar dropdown e mostrar view
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", piloto.equipa_id);
            }
            return View(piloto);
        }
        public ActionResult DetailPiloto(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaPilotos", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var piloto = db.Piloto.Include("Equipa").FirstOrDefault(p => p.Id_piloto == id);
                if (piloto == null)
                    return RedirectToAction("ListaPilotos", new { msg = "Piloto não encontrado" });

                var viewModel = new paginacomunicaçãoviewmodel
                {
                    Piloto = piloto
                };
                return View(viewModel);
            }
        }

        public ActionResult DeletePiloto(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaPilotos", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var piloto = db.Piloto.Include("Equipa").FirstOrDefault(p => p.Id_piloto == id);
                if (piloto == null)
                    return RedirectToAction("ListaPilotos", new { msg = "Piloto não encontrado" });

                var vm = new paginacomunicaçãoviewmodel { Piloto = piloto };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePiloto(int id_piloto)
        {
            using (var db = new F1DBSiteEntities1())
            {
                var piloto = db.Piloto.Find(id_piloto);
                if (piloto == null)
                    return RedirectToAction("ListaPilotos", new { msg = "Piloto não encontrado" });

                db.Piloto.Remove(piloto);
                db.SaveChanges();
            }
            return RedirectToAction("ListaPilotos", new { msg = "Piloto apagado com sucesso" });
        }

        public ActionResult ListaComponentes(string msg, string ordem, string seek)
        {
            ViewBag.Msg = msg;

            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                List<Componente> componentes;

                try
                {
                    // Incluir dados da equipa (para mostrar o nome da equipa)
                    componentes = db.Componente.Include("Equipa").ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.Erro = ex.Message;
                    componentes = new List<Componente>();
                }

                // Filtro por busca (nome, descrição ou nome da equipa)
                if (!string.IsNullOrEmpty(seek))
                {
                    componentes = componentes.Where(c =>
                        (!string.IsNullOrEmpty(c.nome) && c.nome.Contains(seek)) ||
                        (!string.IsNullOrEmpty(c.descricao) && c.descricao.Contains(seek)) ||
                        (c.Equipa != null && !string.IsNullOrEmpty(c.Equipa.nome) && c.Equipa.nome.Contains(seek))
                    ).ToList();
                }

                // Ordenação
                switch (ordem)
                {
                    case "idcompdesc":
                        componentes = componentes.OrderByDescending(c => c.id_componente).ToList();
                        break;
                    case "nomeasc":
                        componentes = componentes.OrderBy(c => c.nome ?? "").ToList();
                        break;
                    case "nomedesc":
                        componentes = componentes.OrderByDescending(c => c.nome ?? "").ToList();
                        break;
                    case "precoasc":
                        componentes = componentes.OrderBy(c => c.preco).ToList();
                        break;
                    case "precodesc":
                        componentes = componentes.OrderByDescending(c => c.preco).ToList();
                        break;
                    default:
                        componentes = componentes.OrderBy(c => c.id_componente).ToList();
                        break;
                }

                // ViewBags para ordenação dos cabeçalhos
                ViewBag.ordemId = string.IsNullOrEmpty(ordem) ? "idcompdesc" : "";
                ViewBag.ordemNome = ordem == "nomeasc" ? "nomedesc" : "nomeasc";
                ViewBag.ordemPreco = ordem == "precoasc" ? "precodesc" : "precoasc";

                // Construir ViewModel
                var viewModel = new paginacomunicaçãoviewmodel
                {
                    Ordem = ordem,
                    Seek = seek,
                    ComponentesEF = componentes
                };

                return View(viewModel);
            }
        }
        // GET: Dashboard/AddComponente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComponente(Componente componente, HttpPostedFileBase fotoFile)
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                try
                {
                    // Processar upload de ficheiro se fornecido
                    if (fotoFile != null && fotoFile.ContentLength > 0)
                    {
                        // Validar tipo de ficheiro
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var fileExtension = Path.GetExtension(fotoFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("fotoFile", "Tipo de ficheiro não permitido. Use JPG, JPEG, PNG, GIF ou BMP.");
                            ViewBag.Equipas = new SelectList(db.Equipa.ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }

                        // Validar tamanho do ficheiro (máximo 5MB)
                        if (fotoFile.ContentLength > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("fotoFile", "O ficheiro é demasiado grande. Tamanho máximo: 5MB.");
                            ViewBag.Equipas = new SelectList(db.Equipa.ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }

                        // Criar nome único para o ficheiro
                        string fileName = Path.GetFileName(fotoFile.FileName);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                        string uploadPath = Server.MapPath("~/imagens/ImagensComponentes/");

                        // Criar diretório se não existir
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Caminho completo do ficheiro
                        string filePath = Path.Combine(uploadPath, uniqueFileName);

                        try
                        {
                            // Guardar ficheiro
                            fotoFile.SaveAs(filePath);

                            // Verificar se o ficheiro foi realmente guardado
                            if (System.IO.File.Exists(filePath))
                            {
                                componente.foto = uniqueFileName;
                                System.Diagnostics.Debug.WriteLine($"Ficheiro guardado com sucesso: {filePath}");
                            }
                            else
                            {
                                throw new Exception("Falha ao guardar o ficheiro no servidor.");
                            }
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao salvar ficheiro: {fileEx.Message}");
                            ModelState.AddModelError("fotoFile", "Erro ao guardar ficheiro: " + fileEx.Message);
                            ViewBag.Equipas = new SelectList(db.Equipa.ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }
                    }

                    // Validar modelo antes de guardar na base de dados
                    if (ModelState.IsValid)
                    {
                        db.Componente.Add(componente);
                        db.SaveChanges();

                        // Para pedidos AJAX
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = true, message = "Componente adicionado com sucesso!" });
                        }

                        return RedirectToAction("ListaComponentes", new { msg = "Componente adicionado com sucesso!" });
                    }
                    else
                    {
                        // Debug dos erros de validação
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            System.Diagnostics.Debug.WriteLine("Erro de validação: " + error.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro geral ao adicionar componente: " + ex.Message);

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Erro ao adicionar componente: " + ex.Message });
                    }

                    ViewBag.Erro = "Erro ao adicionar componente: " + ex.Message;
                }

                // Se chegou aqui, houve erro - recarregar dropdown
                ViewBag.Equipas = new SelectList(db.Equipa.ToList(), "id_equipa", "nome", componente.equipa_id);
                return View(componente);
            }
        }

        // Método para editar componente com upload de imagem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarComponente(Componente componente, HttpPostedFileBase fotoFile)
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                try
                {
                    // Obter componente existente para preservar imagem anterior se necessário
                    var componenteExistente = db.Componente.Find(componente.id_componente);
                    if (componenteExistente == null)
                    {
                        return HttpNotFound();
                    }

                    // Processar novo upload se fornecido
                    if (fotoFile != null && fotoFile.ContentLength > 0)
                    {
                        // Validar tipo de ficheiro
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var fileExtension = Path.GetExtension(fotoFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("fotoFile", "Tipo de ficheiro não permitido. Use JPG, JPEG, PNG, GIF ou BMP.");
                            ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }

                        // Validar tamanho do ficheiro (máximo 5MB)
                        if (fotoFile.ContentLength > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("fotoFile", "O ficheiro é demasiado grande. Tamanho máximo: 5MB.");
                            ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }

                        // Eliminar imagem anterior se existir
                        if (!string.IsNullOrEmpty(componenteExistente.foto))
                        {
                            string oldImagePath = Server.MapPath("~/imagens/ImagensComponentes/" + componenteExistente.foto);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldImagePath);
                                    System.Diagnostics.Debug.WriteLine($"Imagem anterior eliminada: {oldImagePath}");
                                }
                                catch (Exception deleteEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Erro ao eliminar imagem anterior: {deleteEx.Message}");
                                    // Continua mesmo se não conseguir eliminar a imagem anterior
                                }
                            }
                        }

                        // Guardar nova imagem
                        string fileName = Path.GetFileName(fotoFile.FileName);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                        string uploadPath = Server.MapPath("~/imagens/ImagensComponentes/");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, uniqueFileName);

                        try
                        {
                            fotoFile.SaveAs(filePath);

                            if (System.IO.File.Exists(filePath))
                            {
                                componente.foto = uniqueFileName;
                                System.Diagnostics.Debug.WriteLine($"Nova imagem guardada: {filePath}");
                            }
                            else
                            {
                                throw new Exception("Falha ao guardar o ficheiro no servidor.");
                            }
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao salvar novo ficheiro: {fileEx.Message}");
                            ModelState.AddModelError("fotoFile", "Erro ao guardar ficheiro: " + fileEx.Message);
                            ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", componente.equipa_id);
                            return View(componente);
                        }
                    }
                    else
                    {
                        // Se não há novo upload, manter imagem existente
                        componente.foto = componenteExistente.foto;
                    }

                    if (ModelState.IsValid)
                    {
                        db.Entry(componente).State = EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("ListaComponentes", new { msg = "Componente atualizado com sucesso!" });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao editar componente: {ex.Message}");
                    ViewBag.Erro = "Erro ao editar componente: " + ex.Message;
                }

                // Se o modelo for inválido ou houve erro
                ViewBag.Equipas = new SelectList(db.Equipa.OrderBy(e => e.nome).ToList(), "id_equipa", "nome", componente.equipa_id);
                return View(componente);
            }
        }

        // Método auxiliar para eliminar imagem ao apagar componente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComponente(int id_componente)
        {
            using (var db = new F1DBSiteEntities1())
            {
                var componente = db.Componente.Find(id_componente);
                if (componente == null)
                    return RedirectToAction("ListaComponentes", new { msg = "Componente não encontrado" });

                // Eliminar ficheiro de imagem se existir
                if (!string.IsNullOrEmpty(componente.foto))
                {
                    string imagePath = Server.MapPath("~/imagens/ImagensComponentes/" + componente.foto);
                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath);
                            System.Diagnostics.Debug.WriteLine($"Imagem eliminada: {imagePath}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao eliminar imagem: {ex.Message}");
                            // Continua mesmo se não conseguir eliminar a imagem
                        }
                    }
                }

                db.Componente.Remove(componente);
                db.SaveChanges();
            }
            return RedirectToAction("ListaComponentes", new { msg = "Componente apagado com sucesso" });
        }

        // Método auxiliar para validar e redimensionar imagens (opcional)
        private bool IsValidImage(HttpPostedFileBase file)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(file.InputStream))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public ActionResult ExportarEquipasExcel()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var equipas = db.Equipa.ToList();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Equipas");

                    // Cabeçalhos
                    ws.Cell(1, 1).Value = "ID";
                    ws.Cell(1, 2).Value = "Nome";
                    ws.Cell(1, 3).Value = "Descrição";
                    ws.Cell(1, 4).Value = "Dono";
                    ws.Cell(1, 5).Value = "Títulos";
                    ws.Cell(1, 6).Value = "Imagem";

                    int linha = 2;
                    foreach (var e in equipas)
                    {
                        ws.Cell(linha, 1).Value = e.id_equipa;
                        ws.Cell(linha, 2).Value = e.nome;
                        ws.Cell(linha, 3).Value = e.descricao;
                        ws.Cell(linha, 4).Value = e.dono;
                        ws.Cell(linha, 5).Value = e.titulos;
                        ws.Cell(linha, 6).Value = e.imagem;
                        linha++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "equipas.xlsx");
                    }
                }
            }
        }

        public ActionResult ExportarEquipasPDF()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var equipas = db.Equipa.ToList();
                MemoryStream workStream = new MemoryStream();
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, workStream);
                document.Open();

                // Cores F1
                BaseColor f1Red = new BaseColor(220, 20, 60);      // Vermelho F1
                BaseColor f1Black = new BaseColor(15, 15, 15);     // Preto F1
                BaseColor f1Silver = new BaseColor(192, 192, 192); // Prata
                BaseColor f1Gold = new BaseColor(255, 215, 0);     // Dourado

                // Header com logo fictício F1
                PdfPTable headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;
                headerTable.SpacingAfter = 20f;

                // Célula com fundo vermelho F1
                PdfPCell headerCell = new PdfPCell();
                headerCell.BackgroundColor = f1Red;
                headerCell.Border = PdfPCell.NO_BORDER;
                headerCell.PaddingTop = 15f;
                headerCell.PaddingBottom = 15f;

                // Título principal
                var f1TitleFont = FontFactory.GetFont("Arial", 24, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                iTextSharp.text.Paragraph f1Title = new iTextSharp.text.Paragraph("FORMULA 1", f1TitleFont);
                f1Title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                headerCell.AddElement(f1Title);

                // Subtítulo
                var subtitleFont = FontFactory.GetFont("Arial", 14, iTextSharp.text.Font.NORMAL, BaseColor.WHITE);
                iTextSharp.text.Paragraph subtitle = new iTextSharp.text.Paragraph("TEAMS DATABASE", subtitleFont);
                subtitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                subtitle.SpacingBefore = 5f;
                headerCell.AddElement(subtitle);

                headerTable.AddCell(headerCell);
                document.Add(headerTable);

                // Linha decorativa
                PdfPTable decorativeLine = new PdfPTable(3);
                decorativeLine.WidthPercentage = 100;
                decorativeLine.SetWidths(new float[] { 1f, 2f, 1f });
                decorativeLine.SpacingAfter = 20f;

                decorativeLine.AddCell(CreateDecorativeCell(f1Black));
                decorativeLine.AddCell(CreateDecorativeCell(f1Silver));
                decorativeLine.AddCell(CreateDecorativeCell(f1Gold));

                document.Add(decorativeLine);

                // Título da seção
                var sectionFont = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, f1Black);
                iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("🏁 LISTA DE EQUIPAS 🏁", sectionFont);
                sectionTitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                sectionTitle.SpacingAfter = 15f;
                document.Add(sectionTitle);

                // Informações gerais
                var infoFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.DARK_GRAY);
                iTextSharp.text.Paragraph info = new iTextSharp.text.Paragraph($"Relatório gerado em: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | Total de Equipas: {equipas.Count}", infoFont);
                info.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                info.SpacingAfter = 20f;
                document.Add(info);

                // Tabela principal com estilo F1 - CORRIGIDA para 6 colunas
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                float[] widths = new float[] { 8f, 25f, 30f, 18f, 10f, 18f };
                table.SetWidths(widths);
                table.SpacingAfter = 20f;

                // Cabeçalhos da tabela com estilo F1 - CORRIGIDOS
                var headerFont = FontFactory.GetFont("Arial", 11, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                string[] headers = { "ID", "EQUIPA", "DESCRIÇÃO", "DONO", "TÍTULOS", "IMAGEM" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = f1Black;
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cell.Padding = 8f;
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    cell.BorderColor = f1Red;
                    cell.BorderWidth = 2f;
                    table.AddCell(cell);
                }

                // Dados com alternância de cores
                var bodyFont = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, f1Black);
                var bodyFontBold = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.BOLD, f1Black);

                bool isEvenRow = false;
                foreach (var equipa in equipas)
                {
                    BaseColor rowColor = isEvenRow ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                    // ID
                    AddStyledCell(table, equipa.id_equipa.ToString(), bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Nome (destacado)
                    AddStyledCell(table, equipa.nome ?? "N/A", bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Descrição
                    string descricao = equipa.descricao ?? "N/A";
                    if (descricao.Length > 40)
                        descricao = descricao.Substring(0, 37) + "...";
                    AddStyledCell(table, descricao, bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Dono
                    AddStyledCell(table, equipa.dono ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Títulos
                    AddStyledCell(table, (equipa.titulos ?? 0).ToString(), bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Imagem - CORRIGIDO
                    AddStyledCell(table, equipa.imagem ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    isEvenRow = !isEvenRow;
                }

                document.Add(table);

                // Footer
                PdfPTable footerTable = new PdfPTable(1);
                footerTable.WidthPercentage = 100;

                PdfPCell footerCell = new PdfPCell();
                footerCell.BackgroundColor = f1Black;
                footerCell.Border = PdfPCell.NO_BORDER;
                footerCell.Padding = 10f;

                var footerFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.WHITE);
                iTextSharp.text.Paragraph footerText = new iTextSharp.text.Paragraph("🏎️ Formula 1 Teams Database - Powered by Speed & Precision 🏎️", footerFont);
                footerText.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                footerCell.AddElement(footerText);

                footerTable.AddCell(footerCell);
                document.Add(footerTable);

                document.Close();
                byte[] byteInfo = workStream.ToArray();

                return File(byteInfo, "application/pdf", $"F1_Teams_{DateTime.Now.ToString("yyyyMMdd")}.pdf");
            }
        }

        // Método auxiliar para criar células decorativas (necessário para o PDF)
        private PdfPCell CreateDecorativeCell(BaseColor color)
        {
            PdfPCell cell = new PdfPCell();
            cell.BackgroundColor = color;
            cell.Border = PdfPCell.NO_BORDER;
            cell.FixedHeight = 3f;
            return cell;
        }

        // Método auxiliar para adicionar células estilizadas (necessário para o PDF)
        private void AddStyledCell(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
            cell.Padding = 6f;
            cell.Border = PdfPCell.NO_BORDER;
            table.AddCell(cell);
        }
        public ActionResult ExportarPilotosExcel()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var pilotos = db.Piloto.Include("Equipa").ToList();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Pilotos");

                    // Cabeçalhos - ajustados para as propriedades existentes
                    ws.Cell(1, 1).Value = "ID";
                    ws.Cell(1, 2).Value = "Nome";
                    ws.Cell(1, 3).Value = "Nacionalidade";
                    ws.Cell(1, 4).Value = "Equipa";

                    int linha = 2;
                    foreach (var p in pilotos)
                    {
                        ws.Cell(linha, 1).Value = p.Id_piloto;
                        ws.Cell(linha, 2).Value = p.nome;
                        ws.Cell(linha, 3).Value = p.nacionalidade;
                        ws.Cell(linha, 4).Value = p.Equipa?.nome ?? "Sem Equipa";
                        linha++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "pilotos.xlsx");
                    }
                }
            }
        }

        public ActionResult ExportarPilotosPDF()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var pilotos = db.Piloto.Include("Equipa").ToList();
                MemoryStream workStream = new MemoryStream();
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, workStream);
                document.Open();

                // Cores F1
                BaseColor f1Red = new BaseColor(220, 20, 60);      // Vermelho F1
                BaseColor f1Black = new BaseColor(15, 15, 15);     // Preto F1
                BaseColor f1Silver = new BaseColor(192, 192, 192); // Prata
                BaseColor f1Gold = new BaseColor(255, 215, 0);     // Dourado

                // Header com logo fictício F1
                PdfPTable headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;
                headerTable.SpacingAfter = 20f;

                // Célula com fundo vermelho F1
                PdfPCell headerCell = new PdfPCell();
                headerCell.BackgroundColor = f1Red;
                headerCell.Border = PdfPCell.NO_BORDER;
                headerCell.PaddingTop = 15f;
                headerCell.PaddingBottom = 15f;

                // Título principal
                var f1TitleFont = FontFactory.GetFont("Arial", 24, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                iTextSharp.text.Paragraph f1Title = new iTextSharp.text.Paragraph("FORMULA 1", f1TitleFont);
                f1Title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                headerCell.AddElement(f1Title);

                // Subtítulo
                var subtitleFont = FontFactory.GetFont("Arial", 14, iTextSharp.text.Font.NORMAL, BaseColor.WHITE);
                iTextSharp.text.Paragraph subtitle = new iTextSharp.text.Paragraph("DRIVERS DATABASE", subtitleFont);
                subtitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                subtitle.SpacingBefore = 5f;
                headerCell.AddElement(subtitle);

                headerTable.AddCell(headerCell);
                document.Add(headerTable);

                // Linha decorativa
                PdfPTable decorativeLine = new PdfPTable(3);
                decorativeLine.WidthPercentage = 100;
                decorativeLine.SetWidths(new float[] { 1f, 2f, 1f });
                decorativeLine.SpacingAfter = 20f;

                decorativeLine.AddCell(CreateDecorativeCellPilotos(f1Black));
                decorativeLine.AddCell(CreateDecorativeCellPilotos(f1Silver));
                decorativeLine.AddCell(CreateDecorativeCellPilotos(f1Gold));

                document.Add(decorativeLine);

                // Título da seção
                var sectionFont = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, f1Black);
                iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("🏎️ LISTA DE PILOTOS 🏎️", sectionFont);
                sectionTitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                sectionTitle.SpacingAfter = 15f;
                document.Add(sectionTitle);

                // Informações gerais
                var infoFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.DARK_GRAY);
                iTextSharp.text.Paragraph info = new iTextSharp.text.Paragraph($"Relatório gerado em: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | Total de Pilotos: {pilotos.Count}", infoFont);
                info.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                info.SpacingAfter = 20f;
                document.Add(info);

                // Tabela principal com estilo F1 - 4 colunas para pilotos
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                float[] widths = new float[] { 10f, 30f, 25f, 35f };
                table.SetWidths(widths);
                table.SpacingAfter = 20f;

                // Cabeçalhos da tabela com estilo F1
                var headerFont = FontFactory.GetFont("Arial", 11, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                string[] headers = { "ID", "PILOTO", "NACIONALIDADE", "EQUIPA" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = f1Black;
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cell.Padding = 8f;
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    cell.BorderColor = f1Red;
                    cell.BorderWidth = 2f;
                    table.AddCell(cell);
                }

                // Dados com alternância de cores
                var bodyFont = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, f1Black);
                var bodyFontBold = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.BOLD, f1Black);

                bool isEvenRow = false;
                foreach (var piloto in pilotos)
                {
                    BaseColor rowColor = isEvenRow ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                    // ID
                    AddStyledCellPilotos(table, piloto.Id_piloto.ToString(), bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Nome (destacado)
                    AddStyledCellPilotos(table, piloto.nome ?? "N/A", bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Nacionalidade
                    AddStyledCellPilotos(table, piloto.nacionalidade ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Equipa
                    AddStyledCellPilotos(table, piloto.Equipa?.nome ?? "Sem Equipa", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    isEvenRow = !isEvenRow;
                }

                document.Add(table);

                // Footer
                PdfPTable footerTable = new PdfPTable(1);
                footerTable.WidthPercentage = 100;

                PdfPCell footerCell = new PdfPCell();
                footerCell.BackgroundColor = f1Black;
                footerCell.Border = PdfPCell.NO_BORDER;
                footerCell.Padding = 10f;

                var footerFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.WHITE);
                iTextSharp.text.Paragraph footerText = new iTextSharp.text.Paragraph("🏎️ Formula 1 Drivers Database - Powered by Speed & Precision 🏎️", footerFont);
                footerText.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                footerCell.AddElement(footerText);

                footerTable.AddCell(footerCell);
                document.Add(footerTable);

                document.Close();
                byte[] byteInfo = workStream.ToArray();

                return File(byteInfo, "application/pdf", $"F1_Drivers_{DateTime.Now.ToString("yyyyMMdd")}.pdf");
            }
        }

        // Métodos auxiliares para pilotos (renomeados para evitar conflitos)
        private PdfPCell CreateDecorativeCellPilotos(BaseColor color)
        {
            PdfPCell cell = new PdfPCell();
            cell.BackgroundColor = color;
            cell.Border = PdfPCell.NO_BORDER;
            cell.FixedHeight = 3f;
            return cell;
        }

        private void AddStyledCellPilotos(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
            cell.Padding = 6f;
            cell.Border = PdfPCell.NO_BORDER;
            table.AddCell(cell);
        }
    }
}