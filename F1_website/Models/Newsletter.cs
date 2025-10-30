using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace F1_website.Models
{
    public class Newsletter
    {
        private readonly string remetente = ""; //colocar email aqui
        private readonly string senha = "";//colocar pass de aplicação aqui

        public void EnviarEmail(string destinatario, string assunto, string corpo, bool isHtml = false)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(remetente, senha);
                    smtp.EnableSsl = true;

                    using (var mensagem = new MailMessage(remetente, destinatario, assunto, corpo))
                    {
                        mensagem.IsBodyHtml = isHtml;
                        smtp.Send(mensagem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro no envio do email: " + ex.ToString());
            }
        }
    }

    public class NewsletterViewModel
        {
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Introduza um email válido.")]
            public string Email { get; set; }
        }
    }
