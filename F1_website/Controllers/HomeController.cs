using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Xml.Linq;
using F1_website.Models;
using System.Net;
using System.Net.Mail;

namespace F1_website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Contactos_Hub()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_Email()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_funcoes()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult email_page()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_loading()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_settings()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_Performance()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_analises()
        {
            return View();
        }
        [Authorize(Roles = "Admin,chefe")]
        public ActionResult dashboard_main()
        {
            return View();
        }
        [Authorize]
        public ActionResult Treeview1()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarNewsletter(NewsletterViewModel model, string returnUrl)
        {
            System.Diagnostics.Debug.WriteLine("Entrou em EnviarNewsletter com email: " + model.Email);

            if (!ModelState.IsValid)
            {
                TempData["Mensagem"] = "Introduza um email válido.";
                return Redirect(returnUrl ?? "/");
            }

            try
            {
                var newsletter = new Newsletter();

                string assunto = "Obrigado por subscrever a nossa newsletter!";
                string corpo = "<h2>Bem-vindo!</h2><p>Agradecemos a sua subscrição.</p>";
                bool isHtml = true;

                newsletter.EnviarEmail(model.Email, assunto, corpo, isHtml);

                TempData["Mensagem"] = "Subscreveu com sucesso!";
                return Redirect(returnUrl ?? "/");
            }
            catch (Exception ex)
            {
                TempData["Mensagem"] = "Erro ao enviar newsletter: " + ex.Message;
                return Redirect(returnUrl ?? "/");
            }
        }

        public ActionResult Email_Sucesso()
        {
            return View();
        }

        public ActionResult Email_Erro()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarEmail([Bind(Prefix = "Email")] EmailViewModel model, string origem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var email = new Email();
                    email.EnviarEmail(model.Destinatario, model.Assunto, model.Corpo, model.IsHtml);

                    TempData["Mensagem"] = "Email enviado com sucesso!";
                    return RedirectToAction(origem);
                }
                catch (Exception ex)
                {
                    TempData["Mensagem"] = "Erro ao enviar: " + ex.Message;
                }
            }

            var viewModel = new paginacomunicaçãoviewmodel
            {
                Email = model,
                Newsletter = new NewsletterViewModel()
            };

            return View(origem, viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarEmail2([Bind(Prefix = "Email")] EmailViewModel model, string origem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var email = new Email();
                    email.EnviarEmail(model.Destinatario, model.Assunto, model.Corpo, model.IsHtml);

                    TempData["Mensagem"] = "Email enviado com sucesso!";
                    return RedirectToAction(origem);
                }
                catch (Exception ex)
                {
                    TempData["Mensagem"] = "Erro ao enviar: " + ex.Message;
                }
            }

            var viewModel = new paginacomunicaçãoviewmodel
            {
                Email = model,
                Newsletter = new NewsletterViewModel()
            };

            return View(origem, viewModel);
        }

        public ActionResult Music_Max()
        {
            return View();
        }
        public ActionResult Music_smooth()
        {
            return View();
        }
        public ActionResult Music_lewis()
        {
            return View();
        }
        public ActionResult Music_leclerc()
        {
            return View();
        }
        public ActionResult Music_Fernando()
        {
            return View();
        }
        public ActionResult Music_Lando()
        {
            return View();
        }
        public ActionResult Music_Piastri()
        {
            return View();
        }
        public ActionResult Music_f1theme()
        {
            return View();
        }
        public ActionResult Music_Perez()
        {
            return View();
        }
        public ActionResult Creditos()
        {
            return View();
        }
        public ActionResult Musicas_hub()
        {
            return View();
        }
        [Authorize]
        public ActionResult Encriptador()
        {
            return View();
        }
        [Authorize]
        public ActionResult Pilotos(int? id)
        {
            BD bd = new BD();
            List<Pilotos> pilotos = bd.GetPilotos();

            if (id != null)
            {
                pilotos = pilotos.Where(a => a.nm_equipa == id).ToList();
            }

            var model = new paginacomunicaçãoviewmodel()
            {
                Pilotos = pilotos,
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel()
            };

            return PartialView(model);
        }
        [Authorize]
        public ActionResult Pilotos2(int? id)
        {
            BD bd = new BD();
            List<Pilotos> pilotos = bd.GetPilotos();

            if (id != null)
            {
                pilotos = pilotos.Where(a => a.nm_equipa == id).ToList();
            }

            var model = new paginacomunicaçãoviewmodel()
            {
                Pilotos = pilotos,
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel()
            };

            return View(model);
        }

        public ActionResult CreatePiloto()
        {
            Pilotos novo = new Pilotos();
            return PartialView(novo);
        }

        [HttpPost]
        public ActionResult CreatePiloto(Pilotos novo, HttpPostedFileBase imagem)
        {
            if (ModelState.IsValid)
            {
                BD bd = new BD();

                if (bd.GetPilotos().Any(x => x.id_piloto == novo.id_piloto))
                {
                    ModelState.AddModelError("Id", "Já existe um piloto com esse numero.");
                    return PartialView(novo);
                }
                if (imagem != null && imagem.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(imagem.FileName);
                    string path = Path.Combine(Server.MapPath("~/imagens/ImagensPilotos"), fileName);
                    imagem.SaveAs(path);
                    novo.imagem = "~/imagens/ImagensPilotos/" + fileName;
                }

                bd.GetPilotos().Add(novo);
                bd.gravarPilotos();


                return RedirectToAction("DetailPiloto", new { id_piloto = novo.id_piloto, message = "Piloto criado com sucesso" });
            }

            return PartialView(novo);
        }
        public ActionResult DeletePiloto(int? id_piloto)
        {
            if (id_piloto == null)
            {
                HttpNotFound();
            }
            BD bd = new BD();
            Pilotos pilotos = bd.GetPilotos().FirstOrDefault(x => x.id_piloto == id_piloto);
            if (pilotos != null)
            {
                bd.GetPilotos().Remove(pilotos);
                bd.gravarPilotos();

            }
            return RedirectToAction("Equipas", new { message = "Piloto eliminado com sucesso" });
        }

        public ActionResult EditPiloto(int? id_piloto)
        {
            if (id_piloto == null)
            {
                return HttpNotFound();
            }

            BD bd = new BD();
            Pilotos piloto = bd.GetPilotos().FirstOrDefault(x => x.id_piloto == id_piloto);

            if (piloto == null)
            {
                return HttpNotFound();
            }

            return PartialView(piloto);
        }

        [HttpPost]
        public ActionResult EditPiloto(Pilotos editado, HttpPostedFileBase novaImagem)
        {
            if (ModelState.IsValid)
            {
                BD bd = new BD();
                Pilotos piloto = bd.GetPilotos().FirstOrDefault(x => x.id_piloto == editado.id_piloto);

                if (piloto != null)
                {
                    piloto.id_piloto = editado.id_piloto;
                    piloto.nm_equipa = editado.nm_equipa;
                    piloto.nome = editado.nome;
                    piloto.descricao = editado.descricao;
                    piloto.titulos = editado.titulos;
                    piloto.nacionalidade = editado.nacionalidade;
                    piloto.pontos = editado.pontos;

                    if (novaImagem != null && novaImagem.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(novaImagem.FileName);
                        string path = Path.Combine(Server.MapPath("~/imagens/ImagensPilotos"), fileName);
                        novaImagem.SaveAs(path);
                        piloto.imagem = "~/imagens/ImagensPilotos/" + fileName;
                    }

                    bd.gravarPilotos();

                    return RedirectToAction("DetailPiloto", new { id_piloto = piloto.id_piloto, message = "Piloto alterado com sucesso" });

                }
                else
                {
                    return RedirectToAction("DetailPiloto", new { id_piloto = piloto.id_piloto, message = "Piloto alterado com sucesso" });

                }
            }

            return View(editado);
        }

        public ActionResult DetailPiloto(int? id_piloto)
        {
            if (id_piloto == null)
            {
                return HttpNotFound();
            }

            BD bd = new BD();
            Pilotos piloto = bd.GetPilotos().FirstOrDefault(x => x.id_piloto == id_piloto);

            if (piloto == null)
            {
                return RedirectToAction("DetailEquipa", new { message = "Piloto não encontrado" });
            }

            var paginaViewModel = new paginacomunicaçãoviewmodel
            {
                Pilotos = new List<Pilotos> { piloto }
            };

            return View(paginaViewModel);
        }

        public ActionResult DetailEquipa(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BD bd = new BD();
            Equipas equipa = bd.GetEquipas().FirstOrDefault(x => x.id == id);

            if (equipa == null)
            {
                return RedirectToAction("Equipas", new { message = "Equipa não encontrada" });
            }

            var viewModel = new paginacomunicaçãoviewmodel
            {
                Equipas = new List<Equipas> { equipa }
            };

            return View(viewModel);
        }

        public ActionResult EditEquipa(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BD bd = new BD();
            Equipas equipa = bd.GetEquipas().FirstOrDefault(x => x.id == id);

            if (equipa == null)
            {
                return HttpNotFound();
            }

            return PartialView(equipa);
        }

        [HttpPost]
        public ActionResult EditEquipa(Equipas editado, HttpPostedFileBase novaImagem)
        {
            if (ModelState.IsValid)
            {
                BD bd = new BD();
                Equipas equipa = bd.GetEquipas().FirstOrDefault(x => x.id == editado.id);

                if (equipa != null)
                {
                    equipa.id = editado.id;
                    equipa.nome = editado.nome;
                    equipa.descricao = editado.descricao;
                    equipa.titulos = editado.titulos;
                    equipa.fundador = editado.fundador;


                    if (novaImagem != null && novaImagem.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(novaImagem.FileName);
                        string path = Path.Combine(Server.MapPath("~/imagens/ImagensEquipas"), fileName);
                        novaImagem.SaveAs(path);
                        equipa.imagem = "~/imagens/ImagensEquipas/" + fileName;
                    }

                    bd.gravarEquipas();

                    return RedirectToAction("Equipas", new { message = "Equipa alterada com sucesso" });
                }
                else
                {
                    return RedirectToAction("Equipas", new { message = "Equipa não encontrada" });
                }
            }

            return PartialView(editado);
        }


        public ActionResult DeleteEquipa(int? id)
        {
            if (id == null)
            {
                HttpNotFound();
            }
            BD bd = new BD();
            Equipas equipa = bd.GetEquipas().FirstOrDefault(x => x.id == id);
            if (equipa != null)
            {
                bd.GetEquipas().Remove(equipa);
                bd.gravarEquipas();

            }
            return RedirectToAction("Equipas", new { message = "Equipa eliminada com sucesso" });
        }
        public ActionResult CreateEquipa()
        {
            Equipas novo = new Equipas();
            return PartialView(novo);
        }

        [HttpPost]
        public ActionResult CreateEquipa(Equipas novo, HttpPostedFileBase imagem)
        {
            if (ModelState.IsValid)
            {
                BD bd = new BD();

                if (bd.GetEquipas().Any(x => x.id == novo.id))
                {
                    ModelState.AddModelError("id", "Já existe uma Equipa com esse Id.");
                    return PartialView(novo);
                }

                if (imagem != null && imagem.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(imagem.FileName);
                    string path = Path.Combine(Server.MapPath("~/imagens/ImagensEquipas"), fileName);
                    imagem.SaveAs(path);
                    novo.imagem = "~/imagens/ImagensEquipas/" + fileName;
                }

                bd.GetEquipas().Add(novo);
                bd.gravarEquipas();

                return RedirectToAction("Equipas");
            }

            return PartialView(novo);
        }

        [Authorize]
        public ActionResult Equipas(string message)
        {
            ViewBag.message = message;
            BD bd = new BD();

            var model = new paginacomunicaçãoviewmodel()
            {
                Equipas = bd.GetEquipas(),
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel()
            };

            return View(model);
        }
        [Authorize]
        public ActionResult central_infos()
        {
            return View();
        }
        [Authorize]
        public ActionResult jogoForca()
        {
            return View();
        }
        public ActionResult Noticias()
        {
            return View();
        }
        public ActionResult Constituintes()
        {
            return View();
        }
        public ActionResult videos()
        {
            return View();
        }
        public ActionResult redBull_racing()
        {
            return View();
        }
        public ActionResult Aston_Martin()
        {
            return View();
        }
        public ActionResult William()
        {
            return View();
        }
        public ActionResult Alpha_Taurie()
        {
            return View();
        }
        public ActionResult Haas()
        {
            return View();
        }
        public ActionResult kick_sauber()
        {
            return View();
        }
        public ActionResult Alpine()
        {
            return View();
        }
        public ActionResult Mclaren()
        {
            return View();
        }
        public ActionResult Mercedes()
        {
            return View();
        }
        public ActionResult Ferrari()
        {
            return View();
        }
        public ActionResult f1Teams()
        {
            return View();
        }
        [Authorize]
        public ActionResult calculadora()
        {
            return View();
        }
        [Authorize]
        public ActionResult conversor_moedas()
        {
            return View();
        }
        [Authorize]
        public ActionResult converter_imagem()
        {
            return View();
        }
        [Authorize]
        public ActionResult falador()
        {
            return View();
        }
        [Authorize]
        public ActionResult jogos_hub()
        {
            return View();
        }
        public ActionResult Imc(int? peso, float? altura)
        {
            if (!peso.HasValue || !altura.HasValue || altura.Value == 0)
            {
                return Json(new { error = "Entrada inválida. Verifique os valores de peso e altura." }, JsonRequestBehavior.AllowGet);
            }

            var imc = (peso.Value / Math.Pow(altura.Value, 2)).ToString("F2");
            return Json(new { Imc = imc }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Imcpage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Imcpage(float? altura, int? peso)
        {
            if (altura == null || altura <= 0 || peso == null || peso <= 0)
            {
                ViewBag.imc = "Valores inválidos";
            }
            else
            {
                ViewBag.imc = (peso.Value / Math.Pow(altura.Value, 2)).ToString("F2");
            }
            return View();
        }
        [Authorize]
        public ActionResult Adivinha_num()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Adivinha_num(string nome1, int? txtadnum)
        {
            if (Session["count"] == null)
            {
                Session["count"] = 0;
            }

            if (Session["numero_escolhido"] == null)
            {
                Random random = new Random();
                Session["numero_escolhido"] = random.Next(1, 51);
            }

            int count = (int)Session["count"];
            int numero_escolhido = (int)Session["numero_escolhido"];

            if (string.IsNullOrWhiteSpace(nome1) || !txtadnum.HasValue)
            {
                ViewBag.adnum = "Por favor, preencha os campos corretamente!";
                return View();
            }

            if (count >= 5)
            {
                ViewBag.adnum = $"As tentativas acabaram! O número era: {numero_escolhido}";
                return View();
            }

            if (txtadnum == numero_escolhido)
            {
                ViewBag.adnum = "Parabéns, acertou no número!";
            }
            else if (txtadnum > numero_escolhido)
            {
                count++;
                Session["count"] = count;
                ViewBag.adnum = "Tente novamente! O número é menor.";
            }
            else
            {
                count++;
                Session["count"] = count;
                ViewBag.adnum = "Tente novamente! O número é maior.";
            }

            if (count == 5 && txtadnum != numero_escolhido)
            {
                ViewBag.adnum = $"As tentativas acabaram! O número era: {numero_escolhido}";
            }

            return View();
        }

        public ActionResult pagina_principal()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            var model = new paginacomunicaçãoviewmodel()
            {
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel(),
                Login = new LoginViewModel(),
                Equipas = new BD().GetEquipas(),
                Pilotos = new BD().GetPilotos()
            };
            return View(model);
        }
    }
}