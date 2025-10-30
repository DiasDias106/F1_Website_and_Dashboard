using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;

namespace F1_website.Models
{
    public class Email
    {
        private readonly string remetente = "";//introduzir email aqui
        private readonly string senha = "";//introduzir pass de aplicação aqui

        public void EnviarEmail(string Destinatario, string Assunto, string Corpo, bool isHtml = false)
        {
            try
            {
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(remetente, senha),
                    EnableSsl = true
                };

                var mensagem = new MailMessage(remetente, Destinatario, Assunto, Corpo)
                {
                    IsBodyHtml = isHtml
                };

                smtp.Send(mensagem);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Erro no envio do email: " + ex.Message);
            }
        }
    }
    public class EmailViewModel
    {
        [Required(ErrorMessage = "O destinatario e obrigatorio.")]
        [EmailAddress(ErrorMessage = "introduza um email valido")]
        public string Destinatario { get; set; }

        [Required(ErrorMessage = "O assunto é obrigatório.")]
        public string Assunto { get; set; }

        [Required(ErrorMessage = "O corpo da mensagem é obrigatório.")]
        public string Corpo { get; set; }

        public bool IsHtml { get; set; } = false;
    }
}