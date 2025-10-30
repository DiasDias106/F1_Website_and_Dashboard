using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


namespace F1_website.Models
{
        public class Equipas
        {
            public int id { get; set; }
            public string nome { get; set; }
            public string imagem { get; set; }
            public string descricao { get; set; }
            public int titulos { get; set; }

            public string fundador { get; set; }
        }
        public class Pilotos
        {
            public int id_piloto { get; set; }
            public int nm_equipa { get; set; }
            public string nome { get; set; }
            public string imagem { get; set; }
            public string descricao { get; set; }
            public int titulos { get; set; }
            public string nacionalidade { get; set; }

            public int pontos { get; set; }
        }

        public class BD
        {
            List<Equipas> equipas;
            List<Pilotos> pilotos;

            public void carregarEquipas()
            {
                HttpServerUtility server = HttpContext.Current.Server;
                String path = server.MapPath(@"~/App_Data/Equipas.json");
                if (File.Exists(path))
                {
                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(List<Equipas>));
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    equipas = (List<Equipas>)dc.ReadObject(fs);
                    fs.Close();
                }
                else
                {
                    equipas = new List<Equipas>();
                }
            }
            public void carregarPilotos()
            {
                HttpServerUtility server = HttpContext.Current.Server;
                String path = server.MapPath(@"~/App_Data/Pilotos.json");
                if (File.Exists(path))
                {
                    DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(List<Pilotos>));
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    pilotos = (List<Pilotos>)dc.ReadObject(fs);
                    fs.Close();
                }
                else
                {
                    pilotos = new List<Pilotos>();
                }
            }
            public void gravarEquipas()
            {
                HttpServerUtility server = HttpContext.Current.Server;
                String path = server.MapPath(@"~/App_Data/Equipas.json");
                DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(List<Equipas>));
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                dc.WriteObject(fs, equipas);
                fs.Close();
            }
            public void gravarPilotos()
            {
                HttpServerUtility server = HttpContext.Current.Server;
                String path = server.MapPath(@"~/App_Data/Pilotos.json");
                DataContractJsonSerializer dc = new DataContractJsonSerializer(typeof(List<Pilotos>));
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                dc.WriteObject(fs, pilotos);
                fs.Close();
            }

            public BD()
            {
                carregarEquipas();
                carregarPilotos();

            }

            public List<Equipas> GetEquipas() { return equipas; }

            public List<Pilotos> GetPilotos()
            {
                return pilotos;
            }
        }
    }