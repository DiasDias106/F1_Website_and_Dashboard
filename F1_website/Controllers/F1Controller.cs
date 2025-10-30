using ClosedXML.Excel;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Wordprocessing;
using F1_website.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using PagedList.Mvc;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace F1_website.Controllers
{
    public class F1Controller : Controller
    {
        public ActionResult ListaPistas(string msg, int? page, string ordem, string seek, string filtropais, string filtrocomprimento)
        {
            ViewBag.Msg = msg;

            try
            {
                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    int pagina = page ?? 1;
                    int pagesize = 5;

                    IQueryable<Pista> pistasQuery = db.Pista.AsQueryable();

                   
                    if (!string.IsNullOrEmpty(seek))
                    {
                        pistasQuery = pistasQuery.Where(p => p.nome.Contains(seek) ||
                                                           p.pais.Contains(seek) ||
                                                           p.localização.Contains(seek) ||
                                                           p.descricao.Contains(seek));
                    }

                   
                    if (!string.IsNullOrEmpty(filtropais))
                    {
                        pistasQuery = pistasQuery.Where(p => p.pais.Contains(filtropais));
                    }

                   
                    if (!string.IsNullOrEmpty(filtrocomprimento))
                    {
                        switch (filtrocomprimento)
                        {
                            case "curta":
                                pistasQuery = pistasQuery.Where(p => p.comprimento_km < 4);
                                break;
                            case "media":
                                pistasQuery = pistasQuery.Where(p => p.comprimento_km >= 4 && p.comprimento_km <= 5);
                                break;
                            case "longa":
                                pistasQuery = pistasQuery.Where(p => p.comprimento_km > 5);
                                break;
                        }
                    }

                    
                    ViewBag.ordemIdPista = String.IsNullOrEmpty(ordem) ? "idpistadesc" : "";
                    ViewBag.ordemNome = (ordem == "nomeasc") ? "nomedesc" : "nomeasc";
                    ViewBag.ordemPais = (ordem == "paisasc") ? "paisdesc" : "paisasc";
                    ViewBag.ordemComprimento = (ordem == "comprimentoasc") ? "comprimentodesc" : "comprimentoasc";

                    switch (ordem)
                    {
                        case "idpistadesc":
                            pistasQuery = pistasQuery.OrderByDescending(p => p.Id_pista);
                            break;
                        case "nomeasc":
                            pistasQuery = pistasQuery.OrderBy(p => p.nome);
                            break;
                        case "nomedesc":
                            pistasQuery = pistasQuery.OrderByDescending(p => p.nome);
                            break;
                        case "paisasc":
                            pistasQuery = pistasQuery.OrderBy(p => p.pais);
                            break;
                        case "paisdesc":
                            pistasQuery = pistasQuery.OrderByDescending(p => p.pais);
                            break;
                        case "comprimentoasc":
                            pistasQuery = pistasQuery.OrderBy(p => p.comprimento_km);
                            break;
                        case "comprimentodesc":
                            pistasQuery = pistasQuery.OrderByDescending(p => p.comprimento_km);
                            break;
                        default:
                            pistasQuery = pistasQuery.OrderBy(p => p.Id_pista);
                            break;
                    }

                    
                    List<Pista> pistas = pistasQuery.ToList();

                   
                    var paisesDisponiveis = db.Pista.Select(p => p.pais).Distinct().OrderBy(p => p).ToList();

                    var viewModel = new paginacomunicaçãoviewmodel
                    {
                        PistasPaginadas = pistas.ToPagedList(pagina, pagesize),
                        Seek = seek,
                        Ordem = ordem,
                        PaginaAtual = pagina,
                        FiltroPais = filtropais,
                        FiltroComprimento = filtrocomprimento,

                        
                        PaisesDisponiveis = paisesDisponiveis ?? new List<string>(),

                        TotalPistas = pistas.Count(),
                        PistasMaisCurtas = pistas.Where(p => p.comprimento_km < 4).Count(),
                        PistasMedias = pistas.Where(p => p.comprimento_km >= 4 && p.comprimento_km <= 5).Count(),
                        PistasMaisLongas = pistas.Where(p => p.comprimento_km > 5).Count(),

                        Email = new EmailViewModel(),
                        Newsletter = new NewsletterViewModel(),
                        Login = new LoginViewModel(),
                        Register = new RegisterViewModel(),
                        Painel = new PainelViewModel(),
                        Equipas = new List<Equipas>(),
                        Pilotos = new List<Pilotos>(),
                        Pista = new Pista()
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                // Log do erro
                ViewBag.Msg = "Erro ao carregar lista de pistas: " + ex.Message;

                // Retornar viewmodel vazio em caso de erro
                var emptyViewModel = new paginacomunicaçãoviewmodel
                {
                    PistasPaginadas = new List<Pista>().ToPagedList(1, 5),
                    Email = new EmailViewModel(),
                    Newsletter = new NewsletterViewModel(),
                    Login = new LoginViewModel(),
                    Register = new RegisterViewModel(),
                    Painel = new PainelViewModel(),
                    Equipas = new List<Equipas>(),
                    Pilotos = new List<Pilotos>(),
                    Pista = new Pista(),
                    PaisesDisponiveis = new List<string>()
                };

                return View(emptyViewModel);
            }
        }

        // Método para limpar filtros
        public ActionResult LimparFiltros()
        {
            return RedirectToAction("ListaPistas");
        }

        [HttpPost]
        public JsonResult BuscaRapida(string termo)
        {
            try
            {
                if (string.IsNullOrEmpty(termo))
                {
                    return Json(new List<object>());
                }

                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    var resultados = db.Pista
                        .Where(p => p.nome.Contains(termo) || p.pais.Contains(termo) || p.localização.Contains(termo))
                        .Take(5)
                        .Select(p => new {
                            id = p.Id_pista,
                            nome = p.nome,
                            pais = p.pais,
                            localizacao = p.localização,
                            comprimento = p.comprimento_km
                        })
                        .ToList();

                    return Json(resultados);
                }
            }
            catch (Exception ex)
            {
                return Json(new { erro = "Erro na busca: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPista(paginacomunicaçãoviewmodel vm, HttpPostedFileBase fich)
        {
            try
            {
                if (vm?.Pista == null)
                {
                    return RedirectToAction("ListaPistas", new { msg = "Dados inválidos para criação da pista." });
                }

                using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
                {
                    if (string.IsNullOrWhiteSpace(vm.Pista.nome))
                    {
                        return RedirectToAction("ListaPistas", new { msg = "O nome da pista é obrigatório." });
                    }

                    if (string.IsNullOrWhiteSpace(vm.Pista.pais))
                    {
                        return RedirectToAction("ListaPistas", new { msg = "O país da pista é obrigatório." });
                    }

                    if (vm.Pista.comprimento_km <= 0)
                    {
                        return RedirectToAction("ListaPistas", new { msg = "O comprimento da pista deve ser maior que zero." });
                    }

                    if (fich != null && fich.ContentLength > 0)
                    {
                        
                        if (!fich.ContentType.StartsWith("image/"))
                        {
                            return RedirectToAction("ListaPistas", new { msg = "Por favor, selecione apenas arquivos de imagem." });
                        }

                        if (fich.ContentLength > 5 * 1024 * 1024)
                        {
                            return RedirectToAction("ListaPistas", new { msg = "A imagem deve ter no máximo 5MB." });
                        }

                        try
                        {
                            string directoryPath = Server.MapPath("~/imagens/ImagensPistas/");
                            if (!Directory.Exists(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            string ext = Path.GetExtension(fich.FileName).ToLower();
                            string nomePistaSanitizado = Regex.Replace(vm.Pista.nome.ToLower().Trim(), @"[^a-z0-9]+", "-");
                            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                            string fichname = $"{nomePistaSanitizado}-{uniqueId}{ext}";

                            string fullPath = Path.Combine(directoryPath, fichname);

                            
                            fich.SaveAs(fullPath);

                            if (System.IO.File.Exists(fullPath))
                            {
                                vm.Pista.imagem = fichname;
                            }
                            else
                            {
                                return RedirectToAction("ListaPistas", new { msg = "Erro ao salvar a imagem no servidor." });
                            }
                        }
                        catch (Exception ex)
                        {
                            return RedirectToAction("ListaPistas", new { msg = "Erro ao processar imagem: " + ex.Message });
                        }
                    }

                    
                    vm.Pista.nome = vm.Pista.nome?.Trim();
                    vm.Pista.localização = vm.Pista.localização?.Trim();
                    vm.Pista.pais = vm.Pista.pais?.Trim();
                    vm.Pista.unidade_comprimento = vm.Pista.unidade_comprimento?.Trim();
                    vm.Pista.descricao = vm.Pista.descricao?.Trim();

                    db.Pista.Add(vm.Pista);
                    db.SaveChanges();

                    return RedirectToAction("ListaPistas", new { msg = "Pista inserida com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                if (vm?.Pista != null && !string.IsNullOrEmpty(vm.Pista.imagem))
                {
                    try
                    {
                        string directoryPath = Server.MapPath("~/imagens/ImagensPistas/");
                        string imagePath = Path.Combine(directoryPath, vm.Pista.imagem);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    catch
                    {
                    }
                }

                return RedirectToAction("ListaPistas", new { msg = "Erro ao inserir pista: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPista(int Id_pista, string nome, string localização, string pais, decimal comprimento_km, string unidade_comprimento, string descricao, HttpPostedFileBase fich)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    return RedirectToAction("ListaPistas", new { msg = "O nome da pista é obrigatório." });
                }

                if (string.IsNullOrWhiteSpace(pais))
                {
                    return RedirectToAction("ListaPistas", new { msg = "O país da pista é obrigatório." });
                }

                if (comprimento_km <= 0)
                {
                    return RedirectToAction("ListaPistas", new { msg = "O comprimento da pista deve ser maior que zero." });
                }

                using (var db = new F1DBSiteEntities1())
                {
                    var pista = db.Pista.Find(Id_pista);
                    if (pista == null)
                        return RedirectToAction("ListaPistas", new { msg = "Pista não encontrada." });

                    string imagemAnterior = pista.imagem;

                    pista.nome = nome.Trim();
                    pista.localização = localização?.Trim();
                    pista.pais = pais.Trim();
                    pista.comprimento_km = comprimento_km;
                    pista.unidade_comprimento = unidade_comprimento?.Trim();
                    pista.descricao = descricao?.Trim();

                    if (fich != null && fich.ContentLength > 0)
                    {
                        if (!fich.ContentType.StartsWith("image/"))
                        {
                            return RedirectToAction("ListaPistas", new { msg = "Por favor, selecione apenas arquivos de imagem." });
                        }

                        if (fich.ContentLength > 5 * 1024 * 1024)
                        {
                            return RedirectToAction("ListaPistas", new { msg = "A imagem deve ter no máximo 5MB." });
                        }

                        try
                        {
                            string directoryPath = Server.MapPath("~/imagens/ImagensPistas/");
                            if (!Directory.Exists(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            string ext = Path.GetExtension(fich.FileName).ToLower();
                            string nomePistaSanitizado = Regex.Replace(pista.nome.ToLower(), @"[^a-z0-9]+", "-");
                            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                            string novoNomeImagem = $"{nomePistaSanitizado}-{uniqueId}{ext}";
                            string novoPath = Path.Combine(directoryPath, novoNomeImagem);

                            
                            fich.SaveAs(novoPath);

                            if (System.IO.File.Exists(novoPath))
                            {
                                pista.imagem = novoNomeImagem;

                                if (!string.IsNullOrEmpty(imagemAnterior))
                                {
                                    string caminhoImagemAnterior = Path.Combine(directoryPath, imagemAnterior);
                                    if (System.IO.File.Exists(caminhoImagemAnterior))
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(caminhoImagemAnterior);
                                        }
                                        catch
                                        {
                                            
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return RedirectToAction("ListaPistas", new { msg = "Erro ao salvar a nova imagem." });
                            }
                        }
                        catch (Exception ex)
                        {
                            return RedirectToAction("ListaPistas", new { msg = "Erro ao processar imagem: " + ex.Message });
                        }
                    }

                    db.Entry(pista).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ListaPistas", new { msg = "Pista atualizada com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ListaPistas", new { msg = "Erro ao atualizar pista: " + ex.Message });
            }
        }

        public ActionResult DetailPista(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaPistas", new { msg = "ID inválido." });

            try
            {
                using (var db = new F1DBSiteEntities1())
                {
                    var pista = db.Pista.Find(id);
                    if (pista == null)
                        return RedirectToAction("ListaPistas", new { msg = "Pista não encontrada." });

                    var viewModel = new paginacomunicaçãoviewmodel
                    {
                        Pista = pista,
                        Email = new EmailViewModel(),
                        Newsletter = new NewsletterViewModel(),
                        Login = new LoginViewModel(),
                        Register = new RegisterViewModel(),
                        Painel = new PainelViewModel(),
                        Equipas = new List<Equipas>(),
                        Pilotos = new List<Pilotos>(),
                        PaisesDisponiveis = new List<string>()
                    };

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ListaPistas", new { msg = "Erro ao carregar detalhes da pista: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePista(int Id_pista)
        {
            try
            {
                using (var db = new F1DBSiteEntities1())
                {
                    var pista = db.Pista.Find(Id_pista);
                    if (pista == null)
                        return RedirectToAction("ListaPistas", new { msg = "Pista não encontrada." });

                    if (!string.IsNullOrEmpty(pista.imagem))
                    {
                        try
                        {
                            string directoryPath = Server.MapPath("~/imagens/ImagensPistas/");
                            string imagePath = Path.Combine(directoryPath, pista.imagem);
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                        catch
                        {
                        }
                    }

                    db.Pista.Remove(pista);
                    db.SaveChanges();

                    return RedirectToAction("ListaPistas", new { msg = "Pista removida com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ListaPistas", new { msg = "Erro ao remover pista: " + ex.Message });
            }
        }
        public ActionResult ListaCalendario(string msg, int? page, string ordem, string seek, int? filtropista, string filtroperiodo)
        {
            ViewBag.Msg = msg;

            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                db.Configuration.ProxyCreationEnabled = false; 

                int pagina = page ?? 1;
                int pageSize = 10;
                DateTime hoje = DateTime.Today; 

                List<Calendario> eventos = db.Calendario
                    .Include(e => e.Pista1)
                    .ToList();

               
                ViewBag.ordemIdEvento = string.IsNullOrEmpty(ordem) ? "idDesc" : "";
                ViewBag.ordemNomeEvento = ordem == "nomeAsc" ? "nomeDesc" : "nomeAsc";
                ViewBag.ordemDataInicio = ordem == "dataInicioAsc" ? "dataInicioDesc" : "dataInicioAsc";
                ViewBag.ordemDataFim = ordem == "dataFimAsc" ? "dataFimDesc" : "dataFimAsc";
                ViewBag.ordemPista = ordem == "pistaAsc" ? "pistaDesc" : "pistaAsc";

               
                if (!string.IsNullOrEmpty(seek))
                {
                    eventos = eventos.Where(e =>
                        (e.nome_evento ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.descricao ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.Pista1?.nome ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.Pista1?.pais ?? "").ToLower().Contains(seek.ToLower())
                    ).ToList();
                }

                
                if (filtropista.HasValue && filtropista.Value > 0)
                {
                    eventos = eventos.Where(e => e.pista == filtropista.Value).ToList();
                }

                // Filtro por período
                if (!string.IsNullOrEmpty(filtroperiodo))
                {
                    switch (filtroperiodo.ToLower())
                    {
                        case "passados":
                            eventos = eventos.Where(e => e.data_fim < hoje).ToList();
                            break;
                        case "futuros":
                            eventos = eventos.Where(e => e.data_inicio > hoje).ToList();
                            break;
                        case "andamento":
                            eventos = eventos.Where(e => e.data_inicio <= hoje && e.data_fim >= hoje).ToList();
                            break;
                    }
                }

                // Ordenação
                switch (ordem)
                {
                    case "idDesc":
                        eventos = eventos.OrderByDescending(e => e.Id_evento).ToList();
                        break;
                    case "nomeAsc":
                        eventos = eventos.OrderBy(e => e.nome_evento).ToList();
                        break;
                    case "nomeDesc":
                        eventos = eventos.OrderByDescending(e => e.nome_evento).ToList();
                        break;
                    case "dataInicioAsc":
                        eventos = eventos.OrderBy(e => e.data_inicio).ToList();
                        break;
                    case "dataInicioDesc":
                        eventos = eventos.OrderByDescending(e => e.data_inicio).ToList();
                        break;
                    case "dataFimAsc":
                        eventos = eventos.OrderBy(e => e.data_fim).ToList();
                        break;
                    case "dataFimDesc":
                        eventos = eventos.OrderByDescending(e => e.data_fim).ToList();
                        break;
                    case "pistaAsc":
                        eventos = eventos.OrderBy(e => e.Pista1?.nome).ToList();
                        break;
                    case "pistaDesc":
                        eventos = eventos.OrderByDescending(e => e.Pista1?.nome).ToList();
                        break;
                    default:
                        eventos = eventos.OrderBy(e => e.Id_evento).ToList();
                        break;
                }

                // Calcular estatísticas (usando a variável 'hoje' já declarada)
                int totalEventos = eventos.Count();
                int eventosProximos = eventos.Count(e => e.data_inicio > hoje);
                int eventosConcluidos = eventos.Count(e => e.data_fim < hoje);
                int eventosAndamento = eventos.Count(e => e.data_inicio <= hoje && e.data_fim >= hoje);

                // Obter lista de pistas para os dropdowns
                var todasPistas = db.Pista.OrderBy(p => p.nome).ToList();

                var viewModel = new paginacomunicaçãoviewmodel
                {
                    CalendariosPaginados = eventos.ToPagedList(pagina, pageSize),
                    Seek = seek,
                    Ordem = ordem,
                    FiltroPista = filtropista,
                    FiltroPeriodo = filtroperiodo,
                    PaginaAtual = pagina,
                    Pistas = todasPistas,
                    PistasDisponiveis = todasPistas,

                    // Estatísticas
                    TotalEventos = totalEventos,
                    EventosProximos = eventosProximos,
                    EventosConcluidos = eventosConcluidos,
                    EventosAndamento = eventosAndamento,

                    // Outros ViewModels necessários
                    Email = new EmailViewModel(),
                    Newsletter = new NewsletterViewModel(),
                    Login = new LoginViewModel(),
                    Register = new RegisterViewModel(),
                    Painel = new PainelViewModel(),
                    Equipas = new List<Equipas>(),
                    Pilotos = new List<Pilotos>()
                };

                return View(viewModel);
            }
        }

        public ActionResult LimparFiltrosCalendario()
        {
            return RedirectToAction("ListaCalendario");
        }

        public ActionResult ListaCalendarioPartial(string msg, int? page, string ordem, string seek, int? filtropista, string filtroperiodo)
        {
            ViewBag.Msg = msg;

            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                db.Configuration.ProxyCreationEnabled = false;

                int pagina = page ?? 1;
                int pageSize = 10;
                DateTime hoje = DateTime.Today;

                List<Calendario> eventos = db.Calendario
                    .Include(e => e.Pista1)
                    .ToList();

                ViewBag.ordemIdEvento = string.IsNullOrEmpty(ordem) ? "idDesc" : "";
                ViewBag.ordemNomeEvento = ordem == "nomeAsc" ? "nomeDesc" : "nomeAsc";
                ViewBag.ordemDataInicio = ordem == "dataInicioAsc" ? "dataInicioDesc" : "dataInicioAsc";
                ViewBag.ordemDataFim = ordem == "dataFimAsc" ? "dataFimDesc" : "dataFimAsc";
                ViewBag.ordemPista = ordem == "pistaAsc" ? "pistaDesc" : "pistaAsc";

                // Aplicar os mesmos filtros
                if (!string.IsNullOrEmpty(seek))
                {
                    eventos = eventos.Where(e =>
                        (e.nome_evento ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.descricao ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.Pista1?.nome ?? "").ToLower().Contains(seek.ToLower()) ||
                        (e.Pista1?.pais ?? "").ToLower().Contains(seek.ToLower())
                    ).ToList();
                }

                if (filtropista.HasValue && filtropista.Value > 0)
                {
                    eventos = eventos.Where(e => e.pista == filtropista.Value).ToList();
                }

                if (!string.IsNullOrEmpty(filtroperiodo))
                {
                    switch (filtroperiodo.ToLower())
                    {
                        case "passados":
                            eventos = eventos.Where(e => e.data_fim < hoje).ToList();
                            break;
                        case "futuros":
                            eventos = eventos.Where(e => e.data_inicio > hoje).ToList();
                            break;
                        case "andamento":
                            eventos = eventos.Where(e => e.data_inicio <= hoje && e.data_fim >= hoje).ToList();
                            break;
                    }
                }

                switch (ordem)
                {
                    case "idDesc":
                        eventos = eventos.OrderByDescending(e => e.Id_evento).ToList();
                        break;
                    case "nomeAsc":
                        eventos = eventos.OrderBy(e => e.nome_evento).ToList();
                        break;
                    case "nomeDesc":
                        eventos = eventos.OrderByDescending(e => e.nome_evento).ToList();
                        break;
                    case "dataInicioAsc":
                        eventos = eventos.OrderBy(e => e.data_inicio).ToList();
                        break;
                    case "dataInicioDesc":
                        eventos = eventos.OrderByDescending(e => e.data_inicio).ToList();
                        break;
                    case "dataFimAsc":
                        eventos = eventos.OrderBy(e => e.data_fim).ToList();
                        break;
                    case "dataFimDesc":
                        eventos = eventos.OrderByDescending(e => e.data_fim).ToList();
                        break;
                    case "pistaAsc":
                        eventos = eventos.OrderBy(e => e.Pista1?.nome).ToList();
                        break;
                    case "pistaDesc":
                        eventos = eventos.OrderByDescending(e => e.Pista1?.nome).ToList();
                        break;
                    default:
                        eventos = eventos.OrderBy(e => e.Id_evento).ToList();
                        break;
                }

                // Calcular estatísticas (usando a variável 'hoje' já declarada)
                int totalEventos = eventos.Count();
                int eventosProximos = eventos.Count(e => e.data_inicio > hoje);
                int eventosConcluidos = eventos.Count(e => e.data_fim < hoje);
                int eventosAndamento = eventos.Count(e => e.data_inicio <= hoje && e.data_fim >= hoje);

                var todasPistas = db.Pista.OrderBy(p => p.nome).ToList();

                var viewModel = new paginacomunicaçãoviewmodel
                {
                    CalendariosPaginados = eventos.ToPagedList(pagina, pageSize),
                    Seek = seek,
                    Ordem = ordem,
                    FiltroPista = filtropista,
                    FiltroPeriodo = filtroperiodo,
                    PaginaAtual = pagina,
                    Pistas = todasPistas,
                    PistasDisponiveis = todasPistas,
                    TotalEventos = totalEventos,
                    EventosProximos = eventosProximos,
                    EventosConcluidos = eventosConcluidos,
                    EventosAndamento = eventosAndamento,
                    Email = new EmailViewModel(),
                    Newsletter = new NewsletterViewModel(),
                    Login = new LoginViewModel(),
                    Register = new RegisterViewModel(),
                    Painel = new PainelViewModel(),
                    Equipas = new List<Equipas>(),
                    Pilotos = new List<Pilotos>()
                };

                return View(viewModel);
            }
        }
        public ActionResult AddCalendario()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var vm = new paginacomunicaçãoviewmodel
                {
                    Calendario = new Calendario(),
                    Pistas = db.Pista.ToList()
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCalendario(paginacomunicaçãoviewmodel vm)
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Calendario.Add(vm.Calendario);
                        db.SaveChanges();
                        return RedirectToAction("ListaCalendario", new { msg = "Evento adicionado com sucesso!" });
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException e)
                    {
                        var errorMessages = e.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);
                        var fullErrorMessage = string.Join("; ", errorMessages);
                        ModelState.AddModelError("", fullErrorMessage);
                    }
                }
                vm.Pistas = db.Pista.ToList();
                return View(vm);
            }
        }
    
        // GET: Editar evento de calendário
        public ActionResult EditCalendario(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaCalendario", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var calendario = db.Calendario.Find(id);
                if (calendario == null)
                    return RedirectToAction("ListaCalendario", new { msg = "Evento não encontrado" });

                var vm = new paginacomunicaçãoviewmodel
                {
                    Calendario = calendario,
                    Pistas = db.Pista.ToList() // para dropdown
                };

                return View(vm);
            }
        }

        // POST: Editar evento de calendário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCalendario(int Id_evento, string nome_evento, int pista, DateTime data_inicio, DateTime data_fim, int? duracao, string descricao)
        {
            using (var db = new F1DBSiteEntities1())
            {
                var calendario = db.Calendario.Find(Id_evento);
                if (calendario == null)
                    return RedirectToAction("ListaCalendario", new { msg = "Evento não encontrado" });

                // Atualiza campos
                calendario.nome_evento = nome_evento;
                calendario.pista = pista;
                calendario.data_inicio = data_inicio;
                calendario.data_fim = data_fim;
                calendario.duracao = duracao;
                calendario.descricao = descricao;

                db.Entry(calendario).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ListaCalendario", new { msg = "Evento atualizado com sucesso!" });
            }
        }
        public ActionResult DetailCalendario(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaCalendario", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var calendario = db.Calendario.Include("Pista1").FirstOrDefault(c => c.Id_evento == id);
                if (calendario == null)
                    return RedirectToAction("ListaCalendario", new { msg = "Evento não encontrado" });

                var viewModel = new paginacomunicaçãoviewmodel
                {
                    Calendario = calendario
                };

                return View(viewModel);
            }
        }
        public ActionResult DeleteCalendario(int? id)
        {
            if (id == null)
                return RedirectToAction("ListaCalendario", new { msg = "ID inválido" });

            using (var db = new F1DBSiteEntities1())
            {
                var calendario = db.Calendario.Include("Pista1").FirstOrDefault(c => c.Id_evento == id);
                if (calendario == null)
                    return RedirectToAction("ListaCalendario", new { msg = "Evento não encontrado" });

                var vm = new paginacomunicaçãoviewmodel { Calendario = calendario };
                return View(vm);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCalendario(int Id_evento)
        {
            using (var db = new F1DBSiteEntities1())
            {
                var calendario = db.Calendario.Find(Id_evento);
                if (calendario == null)
                    return RedirectToAction("ListaCalendario", new { msg = "Evento não encontrado" });

                db.Calendario.Remove(calendario);
                db.SaveChanges();
            }

            return RedirectToAction("ListaCalendario", new { msg = "Evento apagado com sucesso" });
        }
        public ActionResult ExportarPistasExcel()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var pistas = db.Pista.ToList();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Pistas");

                    // Cabeçalhos
                    ws.Cell(1, 1).Value = "Nome";
                    ws.Cell(1, 2).Value = "Localização";
                    ws.Cell(1, 3).Value = "País";
                    ws.Cell(1, 4).Value = "Comprimento (km)";
                    ws.Cell(1, 5).Value = "Unidade";
                    ws.Cell(1, 6).Value = "Descrição";

                    int linha = 2;
                    foreach (var p in pistas)
                    {
                        ws.Cell(linha, 1).Value = p.nome;
                        ws.Cell(linha, 2).Value = p.localização;
                        ws.Cell(linha, 3).Value = p.pais;
                        ws.Cell(linha, 4).Value = p.comprimento_km;
                        ws.Cell(linha, 5).Value = p.unidade_comprimento;
                        ws.Cell(linha, 6).Value = p.descricao;
                        linha++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "pistas.xlsx");
                    }
                }
            }
        }

        public ActionResult ExportarPistasPDF()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var pistas = db.Pista.ToList();
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
                iTextSharp.text.Paragraph subtitle = new iTextSharp.text.Paragraph("CIRCUIT DATABASE", subtitleFont);
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
                iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("🏁 LISTA DE CIRCUITOS 🏁", sectionFont);
                sectionTitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                sectionTitle.SpacingAfter = 15f;
                document.Add(sectionTitle);

                // Informações gerais
                var infoFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.DARK_GRAY);
                iTextSharp.text.Paragraph info = new iTextSharp.text.Paragraph($"Relatório gerado em: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | Total de Circuitos: {pistas.Count}", infoFont);
                info.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                info.SpacingAfter = 20f;
                document.Add(info);

                // Tabela principal com estilo F1
                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;
                float[] widths = new float[] { 5f, 20f, 20f, 15f, 10f, 10f, 20f };
                table.SetWidths(widths);
                table.SpacingAfter = 20f;

                // Cabeçalhos da tabela com estilo F1
                var headerFont = FontFactory.GetFont("Arial", 11, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                string[] headers = { "ID", "CIRCUITO", "LOCALIZAÇÃO", "PAÍS", "DISTÂNCIA", "UNIDADE", "DESCRIÇÃO" };

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
                foreach (var pista in pistas)
                {
                    BaseColor rowColor = isEvenRow ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                    // ID
                    AddStyledCell(table, pista.Id_pista.ToString(), bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Nome (destacado)
                    AddStyledCell(table, pista.nome ?? "N/A", bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Localização
                    AddStyledCell(table, pista.localização ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // País
                    AddStyledCell(table, pista.pais ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Comprimento
                    AddStyledCell(table, pista.comprimento_km.ToString("F2"), bodyFont, rowColor, iTextSharp.text.Element.ALIGN_RIGHT);

                    // Unidade
                    AddStyledCell(table, pista.unidade_comprimento ?? "km", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Descrição
                    string descricao = pista.descricao ?? "Sem descrição";
                    if (descricao.Length > 50)
                        descricao = descricao.Substring(0, 47) + "...";
                    AddStyledCell(table, descricao, bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

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
                iTextSharp.text.Paragraph footerText = new iTextSharp.text.Paragraph("🏎️ Formula 1 Circuit Database - Powered by Speed & Precision 🏎️", footerFont);
                footerText.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                footerCell.AddElement(footerText);

                footerTable.AddCell(footerCell);
                document.Add(footerTable);

                document.Close();
                byte[] byteInfo = workStream.ToArray();

                return File(byteInfo, "application/pdf", $"F1_Circuits_{DateTime.Now.ToString("yyyyMMdd")}.pdf");
            }
        }

        public ActionResult ExportarCalendarioExcel()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var eventos = db.Calendario
                    .Include(e => e.Pista1)
                    .OrderBy(e => e.data_inicio)
                    .ToList();

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Calendario F1");

                    // Header com estilo F1
                    ws.Range("A1:H1").Merge();
                    ws.Cell(1, 1).Value = "FORMULA 1 - CALENDÁRIO DE EVENTOS";
                    ws.Cell(1, 1).Style.Font.FontSize = 18;
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(220, 20, 60); // F1 Red
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Row(1).Height = 30;

                    // Linha com informações
                    ws.Range("A2:H2").Merge();
                    ws.Cell(2, 1).Value = $"Relatório gerado em: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | Total de Eventos: {eventos.Count}";
                    ws.Cell(2, 1).Style.Font.FontSize = 10;
                    ws.Cell(2, 1).Style.Font.Italic = true;
                    ws.Cell(2, 1).Style.Font.FontColor = XLColor.DarkGray;
                    ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(2).Height = 20;

                    // Cabeçalhos
                    string[] headers = { "ID", "EVENTO", "DESCRIÇÃO", "DATA INÍCIO", "DATA FIM", "CIRCUITO", "LOCALIZAÇÃO", "PAÍS" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(4, i + 1).Value = headers[i];
                        ws.Cell(4, i + 1).Style.Font.Bold = true;
                        ws.Cell(4, i + 1).Style.Font.FontColor = XLColor.White;
                        ws.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(15, 15, 15); // F1 Black
                        ws.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(4, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(4, i + 1).Style.Border.OutsideBorderColor = XLColor.FromArgb(220, 20, 60);
                    }
                    ws.Row(4).Height = 25;

                    // Dados
                    int linha = 5;
                    bool isEvenRow = false;
                    foreach (var evento in eventos)
                    {
                        var rowColor = isEvenRow ? XLColor.FromArgb(248, 248, 248) : XLColor.White;

                        // ID
                        ws.Cell(linha, 1).Value = evento.Id_evento;
                        ws.Cell(linha, 1).Style.Font.Bold = true;
                        ws.Cell(linha, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Nome do Evento
                        ws.Cell(linha, 2).Value = evento.nome_evento ?? "N/A";
                        ws.Cell(linha, 2).Style.Font.Bold = true;

                        // Descrição
                        ws.Cell(linha, 3).Value = evento.descricao ?? "Sem descrição";

                        // Data Início - CORREÇÃO AQUI
                        ws.Cell(linha, 4).Value = evento.data_inicio != DateTime.MinValue ? evento.data_inicio.ToString("dd/MM/yyyy") : "N/A";
                        ws.Cell(linha, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Data Fim - CORREÇÃO AQUI
                        ws.Cell(linha, 5).Value = evento.data_fim != DateTime.MinValue ? evento.data_fim.ToString("dd/MM/yyyy") : "N/A";
                        ws.Cell(linha, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Circuito
                        ws.Cell(linha, 6).Value = evento.Pista1?.nome ?? "N/A";
                        ws.Cell(linha, 6).Style.Font.Bold = true;

                        // Localização
                        ws.Cell(linha, 7).Value = evento.Pista1?.localização ?? "N/A";

                        // País
                        ws.Cell(linha, 8).Value = evento.Pista1?.pais ?? "N/A";
                        ws.Cell(linha, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Aplicar cor de fundo e bordas
                        for (int col = 1; col <= 8; col++)
                        {
                            ws.Cell(linha, col).Style.Fill.BackgroundColor = rowColor;
                            ws.Cell(linha, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            ws.Cell(linha, col).Style.Border.OutsideBorderColor = XLColor.LightGray;
                        }

                        linha++;
                        isEvenRow = !isEvenRow;
                    }

                    // Footer
                    ws.Range($"A{linha + 1}:H{linha + 1}").Merge();
                    ws.Cell(linha + 1, 1).Value = "🏎️ Formula 1 Calendar - Powered by Speed & Precision 🏎️";
                    ws.Cell(linha + 1, 1).Style.Font.FontSize = 12;
                    ws.Cell(linha + 1, 1).Style.Font.Italic = true;
                    ws.Cell(linha + 1, 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(linha + 1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(15, 15, 15);
                    ws.Cell(linha + 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(linha + 1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Row(linha + 1).Height = 25;

                    // Auto-ajustar colunas
                    ws.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"F1_Calendar_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
                    }
                }
            }
        }

        public ActionResult ExportarCalendarioPDF()
        {
            using (F1DBSiteEntities1 db = new F1DBSiteEntities1())
            {
                var eventos = db.Calendario
                    .Include(e => e.Pista1)
                    .OrderBy(e => e.data_inicio)
                    .ToList();

                MemoryStream workStream = new MemoryStream();
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 25, 25, 30, 30); // Landscape
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
                iTextSharp.text.Paragraph subtitle = new iTextSharp.text.Paragraph("CALENDAR DATABASE", subtitleFont);
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
                iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("🏁 CALENDÁRIO DE EVENTOS 🏁", sectionFont);
                sectionTitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                sectionTitle.SpacingAfter = 15f;
                document.Add(sectionTitle);

                // Informações gerais
                var infoFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.ITALIC, BaseColor.DARK_GRAY);
                iTextSharp.text.Paragraph info = new iTextSharp.text.Paragraph($"Relatório gerado em: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | Total de Eventos: {eventos.Count}", infoFont);
                info.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                info.SpacingAfter = 20f;
                document.Add(info);

                // Tabela principal com estilo F1
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                float[] widths = new float[] { 5f, 18f, 22f, 10f, 10f, 15f, 12f, 8f };
                table.SetWidths(widths);
                table.SpacingAfter = 20f;

                // Cabeçalhos da tabela com estilo F1
                var headerFont = FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                string[] headers = { "ID", "EVENTO", "DESCRIÇÃO", "INÍCIO", "FIM", "CIRCUITO", "LOCALIZAÇÃO", "PAÍS" };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = f1Black;
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cell.Padding = 6f;
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    cell.BorderColor = f1Red;
                    cell.BorderWidth = 2f;
                    table.AddCell(cell);
                }

                // Dados com alternância de cores
                var bodyFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, f1Black);
                var bodyFontBold = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, f1Black);

                bool isEvenRow = false;
                foreach (var evento in eventos)
                {
                    BaseColor rowColor = isEvenRow ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                    // ID
                    AddStyledCell(table, evento.Id_evento.ToString(), bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Nome do Evento (destacado)
                    AddStyledCell(table, evento.nome_evento ?? "N/A", bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Descrição
                    string descricao = evento.descricao ?? "Sem descrição";
                    if (descricao.Length > 35)
                        descricao = descricao.Substring(0, 32) + "...";
                    AddStyledCell(table, descricao, bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Data Início - CORREÇÃO AQUI
                    AddStyledCell(table, evento.data_inicio != DateTime.MinValue ? evento.data_inicio.ToString("dd/MM/yyyy") : "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Data Fim - CORREÇÃO AQUI
                    AddStyledCell(table, evento.data_fim != DateTime.MinValue ? evento.data_fim.ToString("dd/MM/yyyy") : "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

                    // Circuito
                    AddStyledCell(table, evento.Pista1?.nome ?? "N/A", bodyFontBold, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // Localização
                    string localizacao = evento.Pista1?.localização ?? "N/A";
                    if (localizacao.Length > 15)
                        localizacao = localizacao.Substring(0, 12) + "...";
                    AddStyledCell(table, localizacao, bodyFont, rowColor, iTextSharp.text.Element.ALIGN_LEFT);

                    // País
                    AddStyledCell(table, evento.Pista1?.pais ?? "N/A", bodyFont, rowColor, iTextSharp.text.Element.ALIGN_CENTER);

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
                iTextSharp.text.Paragraph footerText = new iTextSharp.text.Paragraph("🏎️ Formula 1 Calendar Database - Powered by Speed & Precision 🏎️", footerFont);
                footerText.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                footerCell.AddElement(footerText);

                footerTable.AddCell(footerCell);
                document.Add(footerTable);

                document.Close();
                byte[] byteInfo = workStream.ToArray();

                return File(byteInfo, "application/pdf", $"F1_Calendar_{DateTime.Now.ToString("yyyyMMdd")}.pdf");
            }
        }

        // Método auxiliar para criar células decorativas
        private PdfPCell CreateDecorativeCell(BaseColor color)
        {
            PdfPCell cell = new PdfPCell();
            cell.BackgroundColor = color;
            cell.Border = PdfPCell.NO_BORDER;
            cell.FixedHeight = 5f;
            return cell;
        }

        // Método auxiliar para adicionar células com estilo
        private void AddStyledCell(PdfPTable table, string content, iTextSharp.text.Font font, BaseColor backgroundColor, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
            cell.Padding = 5f;
            cell.Border = PdfPCell.NO_BORDER;
            cell.BorderWidthBottom = 0.5f;
            cell.BorderColorBottom = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

    }
}