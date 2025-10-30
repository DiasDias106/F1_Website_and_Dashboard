using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace F1_website.Models
{
    public class paginacomunicaçãoviewmodel
    {
        public EmailViewModel Email { get; set; }
        public NewsletterViewModel Newsletter { get; set; }
        public LoginViewModel Login { get; set; }
        public RegisterViewModel Register { get; set; }
        public PainelViewModel Painel { get; set; }
        public List<Equipas> Equipas { get; set; }
        public List<Equipa> EquipasEF { get; set; }
        public Equipa Equipa { get; set; }
        public List<Pilotos> Pilotos { get; set; }
        public List<Piloto> PilotoEF { get; set; }
        public Piloto Piloto { get; set; }
        public List<ApplicationUser> UsersEF { get; set; }
        public IPagedList<Pista> PistasPaginadas { get; set; }
        public IPagedList<Calendario> CalendariosPaginados { get; set; }
        public List<SelectListItem> Pist { get; set; }
        public Calendario Calendario { get; set; }
        public string Seek { get; set; }
        public string Ordem { get; set; }
        public int PaginaAtual { get; set; }
        public Pista Pista { get; set; } = new Pista();
        public List<Pista> Pistas { get; set; }
        public IEnumerable<SelectListItem> EquipasDrop { get; set; }
        public List<Componente> ComponentesEF { get; set; }
        public Componente Componente { get; set; }
        public List<AspNetRoles> RolesEF { get; set; }
        public List<UserRoleViewModel> UsersWithRoles { get; set; }
        public AspNetRoles NovoRole { get; set; }
        public string SelectedUserId { get; set; }
        public string SelectedRoleId { get; set; }
        public List<SelectListItem> RolesList { get; set; }

        public string FiltroPais { get; set; }
        public string FiltroComprimento { get; set; }
        public List<string> PaisesDisponiveis { get; set; }

        public int TotalPistas { get; set; }
        public int PistasMaisCurtas { get; set; }
        public int PistasMedias { get; set; }
        public int PistasMaisLongas { get; set; }
        public int? FiltroPista { get; set; }
        public string FiltroPeriodo { get; set; }
        public List<Pista> PistasDisponiveis { get; set; }

        public int TotalEventos { get; set; }
        public int EventosProximos { get; set; }
        public int EventosConcluidos { get; set; }
        public int EventosAndamento { get; set; }

        public paginacomunicaçãoviewmodel()
        {
            Equipas = new List<Equipas>();
            EquipasEF = new List<Equipa>();
            Pilotos = new List<Pilotos>();
            PilotoEF = new List<Piloto>();
            UsersEF = new List<ApplicationUser>();
            Pist = new List<SelectListItem>();
            Pistas = new List<Pista>();
            ComponentesEF = new List<Componente>();
            RolesEF = new List<AspNetRoles>();
            UsersWithRoles = new List<UserRoleViewModel>();
            RolesList = new List<SelectListItem>();
            PaisesDisponiveis = new List<string>();
            PistasDisponiveis = new List<Pista>(); 
            Email = new EmailViewModel();
            Newsletter = new NewsletterViewModel();
            Login = new LoginViewModel();
            Register = new RegisterViewModel();
            Painel = new PainelViewModel();
            Pista = new Pista();
        }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public bool HasRole { get; set; }
    }
}