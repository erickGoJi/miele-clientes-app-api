using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mieleApp.api.Models.Service;
using mieleApp.biz.EntitiesDTO;
using mieleApp.biz.EntitiesDTO.Service;
using mieleApp.biz.Repository.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using template.biz.Entities;
using template.biz.Servicies;
using template.comunicacion;
using template.dal.db_context;
using template.dal.Repository;

namespace mieleApp.dal.Repository.Service
{
    public class ServiceRepository : GenericRepository<ServicioApp>, IServiceRepository
    {
        public ServiceRepository(Db_TemplateContext context) : base(context) { }

        // Envia un Mail de cada tipo a el mail que envien
        public void pruebaMail(string email)
        {
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailAvisoVisita.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            EmailService emailService = new EmailService();
            emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Confimacion de Visita agendada", To = email });

            reader = new StreamReader(Path.GetFullPath("TemplateMail/Email.html"));
            body = string.Empty;
            body = reader.ReadToEnd();
            emailService = new EmailService();
            emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Bienvenido", To = email });

            reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailConfirm.html"));
            body = string.Empty;
            body = reader.ReadToEnd();
            emailService = new EmailService();
            emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Confirma cuenta", To = email });
        }


        public List<RelCategoriaProductoTipoProducto> HoraServicio(int Actividad, int[] Categorias, long idDireccion)
        {
            List<RelCategoriaProductoTipoProducto> _horas = new List<RelCategoriaProductoTipoProducto>();
            _horas = _context.RelCategoriaProductoTipoProducto.Where(c => c.IdTipoServicio == Actividad && Categorias.Contains(c.IdCategoria)).OrderBy(c => c.Id).ToList();
            var _direccion = _context.CatDireccion.FirstOrDefault(c => c.Id == idDireccion);
            var _codigo = _context.CatCoberturaCodigoPostal.FirstOrDefault(c => c.Codigo == _direccion.Cp);
            int suma = 0;

            foreach (var item in _horas)
            {
                var _hora = Convert.ToDecimal(item.HorasTecnicos);
                _hora = Math.Ceiling(_hora);
                item.HorasTecnicos = _hora.ToString();
                if (Actividad == 1)
                    item.PrecioVisita = 0;
                if (Actividad == 3)
                    if (suma != 0)
                        item.PrecioVisita = 490;
                if (_codigo != null)
                    item.Viaticos = _codigo.Costo;
                if (item.Viaticos == null)
                    item.Viaticos = 0;
                suma++;
            }

            return _horas;
        }
        public List<RelCategoriaProductoTipoProducto> HoraVisita(long idVisita)
        {
            var _visita = _context.Visita
                 .Select(c => c)
                 .Include(c => c.RelServicioProducto)
                 .Include(c => c.IdServicioNavigation)
                 .FirstOrDefault(c => c.Id == idVisita);
            if (_visita == null)
            {
                var _visitaapp = _context.VisitaApp
                        .Select(c => c)
                        .Include(c => c.IdServicioNavigation)
                        .Include(c => c.RelServicioCategoriaApp)
                        .FirstOrDefault(c => c.Id == idVisita);
                int[] _categoriaApp = new int[_visitaapp.RelServicioCategoriaApp.Count];
                int j = 0;
                foreach (var item in _visitaapp.RelServicioCategoriaApp)
                {
                    _categoriaApp[j] = item.IdSubLinea;
                    j++;
                }
                return HoraServicio(_visitaapp.IdServicioNavigation.IdTipoServicio, _categoriaApp, _visitaapp.IdDireccion);
            }
            int[] _categoria = new int[_visita.RelServicioProducto.Count];
            int i = 0;
            foreach (var item in _visita.RelServicioProducto)
            {
                _categoria[i] = item.IdCategoria;
                i++;
            }
            return HoraServicio(_visita.IdServicioNavigation.IdTipoServicio, _categoria, _visita.IdDireccion);
        }
        public responseServices getEstatusServicioVisita(long _idVista, bool app)
        {
            responseServices _service = new responseServices();
            if (app)
            {
                var _visita = _context.VisitaApp.Where(c => c.Id == _idVista).Include(c => c.IdServicioNavigation).FirstOrDefault();
                _service.idEstado = Convert.ToInt16(_visita.IdServicioNavigation.IdEstatusServicio);
                _service.visitas = new List<VisitaDTO>() {
                new VisitaDTO(){
                Estatus = _visita.Estatus
                }
                };
            }
            else
            {
                var _visita = _context.Visita.Where(c => c.Id == _idVista).Include(c => c.IdServicioNavigation).FirstOrDefault();
                _service.idEstado = Convert.ToInt16(_visita.IdServicioNavigation.IdEstatusServicio);
                _service.visitas = new List<VisitaDTO>() {
                new VisitaDTO(){
                Estatus = _visita.Estatus
                }
                };
            }
            return _service;
        }

        // ya esta reestructurado

        public List<Visita> ServiceOrder(DateTime _inicio, DateTime _fin, int Actividad, int[] Categorias)
        {
            //int _actividadID = 2;
            //int[] _categoriaID = { 2, 4 };
            //var _consultar = _context
            var _consult = (from a in _context.Servicio
                            join b in _context.Visita on a.Id equals b.IdServicio
                            //join vs in _context.CatEstatusVisita on b.Estatus equals vs.Id into _CatEstatus_Visita
                            //from CatEstatus_Visita in _CatEstatus_Visita.DefaultIfEmpty()
                            join tv in _context.RelTecnicoVisita on b.Id equals tv.IdVista
                            //join c in _context.CatTipoServicio on a.IdTipoServicio equals c.Id
                            //join d in _context.Users on tv.IdTecnico equals d.Id
                            //join e in _context.Tecnicos on d.Id equals e.Id
                            join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                            join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                            where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto) && b.FechaVisita >= _inicio && b.FechaVisita <= _fin
                            select new Visita
                            {
                                //id_tecnico = d.Id,
                                //desc_tipo_servicio = c.DescTipoServicio,
                                //tecnico = d.Name + ' ' + d.Paterno + ' ' + d.Materno,
                                //Id = a.Id,
                                Hora = b.Hora,
                                HoraFin = b.HoraFin,
                                FechaVisita = b.FechaVisita,
                                AsignacionRefacciones = _context.Clientes.FirstOrDefault(c => c.Email == "festivo@dia.mx").Id == a.IdCliente ? true : false
                                //tecnico_color = e.Color,
                                //a.NoServicio,
                                //f.IdActividad,
                                //h.IdCategoriaProducto
                                //desc_estatus_visita = CatEstatus_Visita.DescEstatusVisita == null ? "No iniciada" : CatEstatus_Visita.DescEstatusVisita,
                                //cliente = b.IdServicioNavigation.IdClienteNavigation.Nombre + " " + b.IdServicioNavigation.IdClienteNavigation.Paterno + " " + (b.servicio.cliente.materno == null ? "" : b.servicio.cliente.materno),
                                //b.Pagado,
                                //b.PagoPendiente,
                            }).Distinct().ToList();
            var _consult2 = (from a in _context.ServicioApp
                             join b in _context.VisitaApp on a.Id equals b.IdServicio
                             join tv in _context.RelTecnicoVisitaApp on b.Id equals tv.IdVista
                             join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                             join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                             where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto) && b.FechaVisita >= _inicio && b.FechaVisita <= _fin
                             select new Visita
                             {
                                 Hora = b.Hora,
                                 HoraFin = b.HoraFin,
                                 FechaVisita = b.FechaVisita,
                                 AsignacionRefacciones = false
                             }).Distinct().ToList();
            //formato
            _consult.AddRange(_consult2);
            foreach (var visita in _consult)
            {
                var _hora = Convert.ToDecimal(visita.Hora);
                _hora = Math.Floor(_hora);
                visita.Hora = _hora.ToString("00") + ":00:00";
                visita.Hora = visita.FechaVisita.ToString("yyyy-MM-dd") + "T" + visita.Hora;
                var _horaFin = Convert.ToDecimal(visita.HoraFin);
                _horaFin = Math.Ceiling(_horaFin);
                visita.HoraFin = _horaFin.ToString("00") + ":00:00";
                visita.HoraFin = visita.FechaVisita.ToString("yyyy-MM-dd") + "T" + visita.HoraFin;
                //visita.FechaVisita
            }
            //if (tipo_servicio == 1) {
            //}
            //else {
            //}
            //var filtro = _consult.Select(c=>c.)
            return _consult;
        }

        public List<dto_calendario> ServiceOrder_new(DateTime _inicio, DateTime _fin, DateTime _selected, int Actividad, int[] Categorias, int no_tecnico)
        {
            var _consult = (from a in _context.Servicio
                            join b in _context.Visita on a.Id equals b.IdServicio
                            join tv in _context.RelTecnicoVisita on b.Id equals tv.IdVista
                            join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                            join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                            where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto) && b.FechaVisita >= _inicio && b.FechaVisita <= _fin
                            select new Visita
                            {
                                Id = b.Id,
                                Hora = b.Hora,
                                HoraFin = b.HoraFin,
                                FechaVisita = b.FechaVisita,
                                AsignacionRefacciones = _context.Clientes.FirstOrDefault(c => c.Email == "festivo@dia.mx").Id == a.IdCliente ? true : false
                            }).Distinct().ToList();
            var _consult2 = (from a in _context.ServicioApp
                             join b in _context.VisitaApp on a.Id equals b.IdServicio
                             join tv in _context.RelTecnicoVisitaApp on b.Id equals tv.IdVista
                             join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                             join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                             where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto) && b.FechaVisita >= _inicio && b.FechaVisita <= _fin
                             select new Visita
                             {
                                 Id = b.Id,
                                 Hora = b.Hora,
                                 HoraFin = b.HoraFin,
                                 FechaVisita = b.FechaVisita,
                                 AsignacionRefacciones = false
                             }).Distinct().ToList();
            //formato
            _consult.AddRange(_consult2);
            foreach (var visita in _consult)
            {
                var _hora = Convert.ToDecimal(visita.Hora);
                _hora = Math.Floor(_hora);
                visita.Hora = _hora.ToString("00") + ":00:00";
                visita.Hora = visita.FechaVisita.ToString("yyyy-MM-dd") + "T" + visita.Hora;
                var _horaFin = Convert.ToDecimal(visita.HoraFin);
                _horaFin = Math.Ceiling(_horaFin);
                visita.HoraFin = _horaFin.ToString("00") + ":00:00";
                visita.HoraFin = visita.FechaVisita.ToString("yyyy-MM-dd") + "T" + visita.HoraFin;
            }
            var _consult_xdia = _consult.Where(c => c.FechaVisita.Date == _selected.Date).ToList();
            long[] _ids_visita = new long[500];
            int count = 0;
            foreach (var visita in _consult_xdia)
            {
                _ids_visita[count] = visita.Id;
                count = count + 1;
            }
            var _tecnicos = (from tv in _context.RelTecnicoVisita
                             join co in _context.Visita on tv.IdVista equals co.Id
                             join e in _context.Tecnicos on tv.IdTecnico equals e.Id
                             join f in _context.TecnicosActividad on e.Id equals f.IdUser
                             join h in _context.TecnicosProducto on e.Id equals h.IdUser
                             where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto)
                             // && co.FechaVisita == _selected
                             && !_ids_visita.Contains(tv.IdVista)
                             select new Tecnicos
                             {
                                 Id = e.Id,
                                 VehiculoInfo = e.VehiculoInfo,
                                 VehiculoPlacas = e.VehiculoPlacas,
                                 Color = e.IdNavigation.Name + ' ' + e.IdNavigation.Paterno + ' ' + e.IdNavigation.Materno,
                                 Noalmacen = e.IdNavigation.Avatar
                             }).OrderByDescending(c=>c.Id).Distinct().ToList();
            var _tecnicos2 = (from tv in _context.RelTecnicoVisitaApp
                              join co in _context.VisitaApp on tv.IdVista equals co.Id
                              join e in _context.Tecnicos on tv.IdTecnico equals e.Id
                              join f in _context.TecnicosActividad on e.Id equals f.IdUser
                              join h in _context.TecnicosProducto on e.Id equals h.IdUser
                              where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto)
                              //&& co.FechaVisita == _selected
                              //&& !_ids_visita.Contains(tv.IdVista)
                              select new Tecnicos
                              {
                                  Id = e.Id,
                                  VehiculoInfo = e.VehiculoInfo,
                                  VehiculoPlacas = e.VehiculoPlacas,
                                  Color = e.IdNavigation.Name + ' ' + e.IdNavigation.Paterno + ' ' + e.IdNavigation.Materno,
                                  Noalmacen = e.IdNavigation.Avatar
                              }).OrderByDescending(c => c.Id).Distinct().ToList();
            _tecnicos.AddRange(_tecnicos2);
            _tecnicos = _tecnicos.Take(no_tecnico).ToList();
            List<dto_calendario> _dis = new List<dto_calendario>();

            _dis.Add(new dto_calendario
            {
                visita = _consult,
                tecnico = _tecnicos
            });

            return _dis;
        }

        public IActionResult GetDisponibilidad(DateTime fecha_visita, int Actividad, int[] Categorias, int tipo_servicio, int horas_visita, int no_tecnicos)
        {
            IActionResult response;

            var _consult = (from a in _context.Servicio
                            join b in _context.Visita on a.Id equals b.IdServicio
                            where b.FechaVisita.Date == fecha_visita.Date
                            select new
                            {
                                Hora = b.Hora,
                                HoraFin = b.HoraFin,
                                FechaVisita = b.FechaVisita,
                                AsignacionRefacciones = _context.Clientes.FirstOrDefault(c => c.Email == "festivo@dia.mx").Id == a.IdCliente ? true : false,
                                total_horas = (Convert.ToDecimal(b.HoraFin) - Convert.ToDecimal(b.Hora)).ToString(),
                                Tecnicos = (from user in _context.Users
                                            join tecnico in _context.Tecnicos on user.Id equals tecnico.Id
                                            join rel_tecnico in _context.RelTecnicoVisita on tecnico.Id equals rel_tecnico.IdTecnico
                                            join visita in _context.Visita on rel_tecnico.IdVista equals visita.Id
                                            join tec_actividad in _context.TecnicosActividad on tecnico.Id equals tec_actividad.IdUser
                                            join tec_prodcutos in _context.TecnicosProducto on tecnico.Id equals tec_prodcutos.IdUser
                                            where tec_actividad.IdActividad == Actividad && Categorias.Contains(tec_prodcutos.IdCategoriaProducto) && visita.Hora != null && visita.FechaVisita.Date == fecha_visita.Date
                                            select new
                                            {
                                                Id = tecnico.Id,
                                                //VehiculoInfo = tecnico.VehiculoInfo,
                                                //VehiculoPlacas = tecnico.VehiculoPlacas,
                                                //Nombre = user.Name + ' ' + user.Paterno + ' ' + user.Materno,
                                                //Noalmacen = user.Avatar,
                                                //id_visita = visita.Id,
                                                visita.FechaVisita,
                                                visita.Hora,
                                                visita.HoraFin,
                                                total_horas = (Convert.ToDecimal(visita.HoraFin) - Convert.ToDecimal(visita.Hora)).ToString(),
                                            }).Distinct().ToList()

                            }).Distinct().ToList();
            var _consult2 = (from a in _context.ServicioApp
                             join b in _context.VisitaApp on a.Id equals b.IdServicio
                             where b.FechaVisita.Date == fecha_visita.Date
                             select new
                             {
                                 Hora = b.Hora,
                                 HoraFin = b.HoraFin,
                                 FechaVisita = b.FechaVisita,
                                 AsignacionRefacciones = false,
                                 total_horas = (Convert.ToDecimal(b.HoraFin) - Convert.ToDecimal(b.Hora)).ToString(),
                                 Tecnicos = (from user in _context.Users
                                             join tecnico in _context.Tecnicos on user.Id equals tecnico.Id
                                             join rel_tecnico in _context.RelTecnicoVisitaApp on tecnico.Id equals rel_tecnico.IdTecnico
                                             join visita in _context.VisitaApp on rel_tecnico.IdVista equals visita.Id
                                             join tec_actividad in _context.TecnicosActividad on tecnico.Id equals tec_actividad.IdUser
                                             join tec_prodcutos in _context.TecnicosProducto on tecnico.Id equals tec_prodcutos.IdUser
                                             where tec_actividad.IdActividad == Actividad && Categorias.Contains(tec_prodcutos.IdCategoriaProducto) && visita.Hora != null && visita.FechaVisita.Date == fecha_visita.Date
                                             select new
                                             {
                                                 Id = tecnico.Id,
                                                 //VehiculoInfo = tecnico.VehiculoInfo,
                                                 //VehiculoPlacas = tecnico.VehiculoPlacas,
                                                 //Nombre = user.Name + ' ' + user.Paterno + ' ' + user.Materno,
                                                 //Noalmacen = user.Avatar,
                                                 //id_visita = visita.Id,
                                                 visita.FechaVisita,
                                                 visita.Hora,
                                                 visita.HoraFin,
                                                 total_horas = (Convert.ToDecimal(visita.HoraFin) - Convert.ToDecimal(visita.Hora)).ToString(),
                                             }).Distinct().ToList()
                             }).Distinct().ToList();
            //formato
            _consult.AddRange(_consult2);
            decimal _total_horas = 0;
            decimal _total_horas_tecnico = 0;
            decimal _total_horas_tecnico_2 = 0;
            int _disponible = 0;
            int[] _valor = new int[10];
            int[] _valor_tecnico = new int[10];
            long[] _id_tecnicos = new long[10];
            int sub = 0;
            int total = 0;
            int sub_tecnico = 0;
            int total_tecnico = 0;
            int sub_tecnico_2 = 0;
            int total_tecnico_2 = 0;
            //_consult = _consult.OrderByDescending(c => c.Hora).ToList();
            decimal horas = 0;
            decimal horas_tecnico = 0;
            decimal horas_tecnico_2 = 0;
            int dias_adicionales = 0;
            List<tecnico_disponibilidad> _disponibilidad_tec = new List<tecnico_disponibilidad>();
            List<Disponibilidad> _dis = new List<Disponibilidad>();

            for (int i = 0; i < _consult.Count(); i++)
            {
                horas = Convert.ToDecimal(_consult[i].HoraFin) - Convert.ToDecimal(_consult[i].Hora);

                int indice = Convert.ToInt32(_consult[i].Hora) - 8;
                //_valor[indice] = 0;
                for (int j = indice; j < indice + horas; j++)
                {
                    _valor[j] = 1;
                }

                _total_horas = Convert.ToDecimal(_total_horas) + Convert.ToDecimal(_consult[i].total_horas);

            }

            for (int y = 0; y < _valor.Count(); y++)
            {
                if (_valor[y] == 1)
                {
                    if (sub != 0)
                    {
                        if (total < sub)
                        {
                            total = sub;
                        }
                    }
                    sub = 0;
                }
                else
                {
                    sub = sub + 1;
                    if (_valor.Count() == y + 1)
                    {
                        total = sub;
                    }

                }
            }

            if (tipo_servicio == 1)
            {               

                //if (fecha_visita.Date.DayOfWeek == DayOfWeek.Monday || fecha_visita.Date.DayOfWeek == DayOfWeek.Tuesday || fecha_visita.Date.DayOfWeek == DayOfWeek.Wednesday)
                //{
                //    dias_adicionales = 3;
                //}

                if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Monday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Tuesday)

                {
                    if (fecha_visita.Date == DateTime.Now.AddDays(1).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(2).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(3).Date) _disponible = 72;
                    dias_adicionales = 3;
                }
                if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Wednesday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Thursday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Friday)
                {
                    if (fecha_visita.Date == DateTime.Now.AddDays(1).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(2).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(3).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(4).Date) _disponible = 72;
                    if (fecha_visita.Date == DateTime.Now.AddDays(5).Date) _disponible = 72;
                    dias_adicionales = 5;
                }

                if (fecha_visita.Date > DateTime.Now.AddDays(dias_adicionales).Date)
                {
                    _disponible = _total_horas != 0 ? 0 : 1;

                    if (_disponible == 1)
                    {
                        var _result = (from user in _context.Users
                                       join tecnico in _context.Tecnicos on user.Id equals tecnico.Id
                                       join rel_tecnico in _context.RelTecnicoVisita on tecnico.Id equals rel_tecnico.IdTecnico
                                       join tec_actividad in _context.TecnicosActividad on tecnico.Id equals tec_actividad.IdUser
                                       join tec_prodcutos in _context.TecnicosProducto on tecnico.Id equals tec_prodcutos.IdUser
                                       where tec_actividad.IdActividad == Actividad && Categorias.Contains(tec_prodcutos.IdCategoriaProducto)
                                       select new
                                       {
                                           id = tecnico.Id,
                                           vehiculoInfo = tecnico.VehiculoInfo,
                                           vehiculoPlacas = tecnico.VehiculoPlacas,
                                           nombre = user.Name + ' ' + user.Paterno + ' ' + user.Materno,
                                           noalmacen = user.Avatar
                                       }).Distinct().Take(no_tecnicos).ToList();

                        for (int i = 0; i < _result.Count(); i++)
                        {
                            _disponibilidad_tec.Add(new tecnico_disponibilidad
                            {
                                id = _result[i].id,
                                vehiculoInfo = _result[i].vehiculoInfo == null ? "" : _result[i].vehiculoInfo,
                                vehiculoPlacas = _result[i].vehiculoPlacas == null ? "" : _result[i].vehiculoPlacas,
                                nombre = _result[i].nombre,
                                noalmacen = _result[i].noalmacen
                            });
                        }

                        _dis.Add(new Disponibilidad
                        {
                            disponible = _disponible,
                            tecnico_disponibilidad = _disponibilidad_tec
                        });

                        return new ObjectResult(_dis);

                    }
                }

                _dis.Add(new Disponibilidad
                {
                    disponible = _disponible,
                    tecnico_disponibilidad = null
                });

                return new ObjectResult(_dis);
            }
            else
            {
                if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Wednesday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Monday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Tuesday)
                {
                    if (fecha_visita.Date == DateTime.Now.AddDays(1).Date) _disponible = 48;
                    if (fecha_visita.Date == DateTime.Now.AddDays(2).Date) _disponible = 48;
                    dias_adicionales = 2;
                }
                if (DateTime.Now.Date.DayOfWeek == DayOfWeek.Thursday || DateTime.Now.Date.DayOfWeek == DayOfWeek.Friday)
                {
                    if (fecha_visita.Date == DateTime.Now.AddDays(1).Date) _disponible = 48;
                    if (fecha_visita.Date == DateTime.Now.AddDays(2).Date) _disponible = 48;
                    if (fecha_visita.Date == DateTime.Now.AddDays(3).Date) _disponible = 48;
                    if (fecha_visita.Date == DateTime.Now.AddDays(4).Date) _disponible = 48;
                    dias_adicionales = 4;
                }

                if (fecha_visita.Date > DateTime.Now.AddDays(dias_adicionales).Date)
                {
                    _disponible = horas_visita < total ? 1 : 0;

                    if (_disponible == 1)
                    {
                        for (int i = 0; i < _consult.Count(); i++)
                        {
                            for (int z = 0; z < _consult[i].Tecnicos.Count(); z++)
                            {
                                _id_tecnicos[z] = _consult[i].Tecnicos[z].Id;
                            }
                        }

                        var _result = (from user in _context.Users
                                       join tecnico in _context.Tecnicos on user.Id equals tecnico.Id
                                       join rel_tecnico in _context.RelTecnicoVisita on tecnico.Id equals rel_tecnico.IdTecnico
                                       join tec_actividad in _context.TecnicosActividad on tecnico.Id equals tec_actividad.IdUser
                                       join tec_prodcutos in _context.TecnicosProducto on tecnico.Id equals tec_prodcutos.IdUser
                                       where tec_actividad.IdActividad == Actividad && Categorias.Contains(tec_prodcutos.IdCategoriaProducto) && !_id_tecnicos.Contains(tecnico.Id)
                                       select new
                                       {
                                           Id = tecnico.Id,
                                           VehiculoInfo = tecnico.VehiculoInfo,
                                           VehiculoPlacas = tecnico.VehiculoPlacas,
                                           Nombre = user.Name + ' ' + user.Paterno + ' ' + user.Materno,
                                           Noalmacen = user.Avatar
                                       }).Distinct().Take(no_tecnicos).ToList();


                        for (int i = 0; i < _result.Count(); i++)
                        {
                            _disponibilidad_tec.Add(new tecnico_disponibilidad
                            {
                                id = _result[i].Id,
                                vehiculoInfo = _result[i].VehiculoInfo == null ? "" : _result[i].VehiculoInfo,
                                vehiculoPlacas = _result[i].VehiculoPlacas == null ? "" : _result[i].VehiculoPlacas,
                                nombre = _result[i].Nombre,
                                noalmacen = _result[i].Noalmacen
                            });
                        }

                        _dis.Add(new Disponibilidad
                        {
                            disponible = _disponible,
                            tecnico_disponibilidad = _disponibilidad_tec
                        });

                        return new ObjectResult(_dis);

                    }

                }

                _dis.Add(new Disponibilidad
                {
                    disponible = _disponible,
                    tecnico_disponibilidad = null
                });

                return new ObjectResult(_dis);
            }
        }

        public List<Tecnicos> GetTecnicos(DateTime _inicio, DateTime _fin, int Actividad, int[] Categorias)
        {
            //int _actividadID = 2;
            //int[] _categoriaID = { 2, 4 };
            //var _consultar = _context
            var _consult = (from
                             d in _context.Users
                            join e in _context.Tecnicos on d.Id equals e.Id
                            join f in _context.TecnicosActividad on e.Id equals f.IdUser
                            join h in _context.TecnicosProducto on e.Id equals h.IdUser
                            where f.IdActividad == Actividad && Categorias.Contains(h.IdCategoriaProducto)
                            select new Tecnicos
                            {
                                Id = e.Id,
                                VehiculoInfo = e.VehiculoInfo,
                                VehiculoPlacas = e.VehiculoPlacas,
                                Color = d.Name + ' ' + d.Paterno + ' ' + d.Materno,
                                Noalmacen = d.Avatar

                            }).Distinct().ToList();
            //formato
            _consult = _consult.Take(2).ToList();
            return _consult;
        }
        // ya esta reestructurado



        public List<responseServices> GetAllServicesShow(long idCliente, bool pendientes)
        {
            List<responseServices> _reponse = new List<responseServices>();
            List<responseServices> _reponse2 = new List<responseServices>();

            _reponse = (from _sa in _context.ServicioApp
                        where _sa.IdCliente == idCliente
                        select new responseServices
                        {
                            id = _sa.Id,
                            idEstado = Convert.ToInt32(_sa.IdEstatusServicio),
                            estado = _context.CatEstatusServicio.FirstOrDefault(c => c.Id == _sa.IdEstatusServicio).DescEstatusServicio,
                            descripcion = _sa.DescripcionActividades,
                            actualizado = Convert.ToDateTime(_sa.Actualizado),
                            //visitas =  _context.VisitaApp.Where(c=>c.IdServicio == _sa.Id).Cast<Visita>().ToList()
                            visitas = (from _visita in _context.VisitaApp
                                       where _visita.IdServicio == _sa.Id
                                       select new VisitaDTO
                                       {
                                           Id = _visita.Id,
                                           Hora = _visita.Hora,
                                           HoraFin = _visita.HoraFin,
                                           FechaVisita = _visita.FechaVisita,
                                           IdServicio = _visita.IdServicio,
                                           RelTecnicoVisita = _context.RelTecnicoVisita.Where(c => c.IdVista == _visita.Id).Include(c => c.IdTecnicoNavigation).ToList(),
                                           Estatus = _visita.Estatus,
                                           //Estatus = _sa.IdEstatusServicio,
                                           estatusDescripcion = _context.CatEstatusVisita.FirstOrDefault(c => c.Id == _visita.Estatus).DescEstatusVisita,
                                           // estatusDescripcion = _context.CatEstatusServicio.FirstOrDefault(c => c.Id == _sa.IdEstatusServicio).DescEstatusServicio,
                                           app = true,
                                           Pagado = _visita.Pagado,
                                           PagoPendiente = _visita.PagoPendiente,
                                           //tecnico = consultTecnicoData(_context.RelTecnicoVisita.Where(c => c.IdVista == _visita.Id).Include(c => c.IdTecnicoNavigation).ToList())
                                           tecnico = (from _tecnico in _context.RelTecnicoVisitaApp
                                                      join _user in _context.Users on _tecnico.IdTecnico equals _user.Id
                                                      where _tecnico.IdVista == _visita.Id
                                                      select new tecnicoDTO
                                                      {
                                                          idTecnico = _tecnico.IdTecnico,
                                                          nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                                                          responsable = _tecnico.TecnicoResponsable
                                                      }).ToList()
                                       }).ToList()
                        }).ToList();

            var _idWeb = _context.RelUserUserApp.FirstOrDefault(c => c.IdClientApp == idCliente);
            if (_idWeb != null)
                _reponse2 = (from _sa in _context.Servicio
                             where _sa.IdCliente == _idWeb.IdClient
                             select new responseServices
                             {
                                 id = _sa.Id,
                                 idEstado = Convert.ToInt32(_sa.IdEstatusServicio),
                                 estado = _context.CatEstatusServicio.FirstOrDefault(c => c.Id == _sa.IdEstatusServicio).DescEstatusServicio,
                                 descripcion = _sa.DescripcionActividades,
                                 actualizado = Convert.ToDateTime(_sa.Actualizado),
                                 visitas = (from _visita in _context.Visita
                                            where _visita.IdServicio == _sa.Id
                                            select new VisitaDTO
                                            {
                                                Id = _visita.Id,
                                                Hora = _visita.Hora,
                                                HoraFin = _visita.HoraFin,
                                                FechaVisita = _visita.FechaVisita,
                                                IdServicio = _visita.IdServicio,
                                                RelTecnicoVisita = _context.RelTecnicoVisita.Where(c => c.IdVista == _visita.Id).Include(c => c.IdTecnicoNavigation).ToList(),
                                                Estatus = _visita.Estatus,
                                                pagadaCotizacion = _visita.CotizacionPagada,
                                                //Estatus = _sa.IdEstatusServicio,
                                                estatusDescripcion = _context.CatEstatusVisita.FirstOrDefault(c => c.Id == _visita.Estatus).DescEstatusVisita,
                                                //estatusDescripcion = _context.CatEstatusServicio.FirstOrDefault(c => c.Id == _sa.IdEstatusServicio).DescEstatusServicio,
                                                app = false,
                                                Pagado = _visita.Pagado,
                                                PagoPendiente = _visita.PagoPendiente,
                                                //tecnico = consultTecnicoData(_context.RelTecnicoVisita.Where(c => c.IdVista == _visita.Id).Include(c => c.IdTecnicoNavigation).ToList())
                                                tecnico = (from _tecnico in _context.RelTecnicoVisita
                                                           join _user in _context.Users on _tecnico.IdTecnico equals _user.Id
                                                           where _tecnico.IdVista == _visita.Id
                                                           select new tecnicoDTO
                                                           {
                                                               idTecnico = _tecnico.IdTecnico,
                                                               nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                                                               responsable = _tecnico.TecnicoResponsable
                                                           }).ToList()
                                            }).ToList()
                                 //visitas = _context.Visita.Where(c => c.IdServicio == _sa.Id).Include("RelTecnicoVisita.IdTecnicoNavigation").ToList()
                             }).ToList();

            var _union = _reponse.Union(_reponse2).ToList();
            if (pendientes)
                _union = _union.Where(c => c.idEstado != 15).ToList();
            return _union.OrderByDescending(c => c.actualizado).ToList();
        }

        public List<tecnicoDTO> consultTecnicoData(List<RelTecnicoVisita> _relacion)
        {
            List<tecnicoDTO> _tecnicos = new List<tecnicoDTO>();
            for (int i = 0; i < _relacion.Count; i++)
            {
                Users _user = new Users();
                _user = _context.Users.FirstOrDefault(c => c.Id == _relacion[i].IdTecnico);
                _tecnicos.Add(new tecnicoDTO()
                {
                    idTecnico = _relacion[i].IdTecnico,
                    nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                    responsable = _relacion[i].TecnicoResponsable
                });
            }
            return _tecnicos;
        }

        //public bool ServiceOrderSaveVisita(long idCliente, long idServicio, string hora, string horaFin, DateTime fecha, int idDireccion, long[] idTecnicos)
        public bool ServiceOrderSaveVisita(ServiceAddDTO _service)
        {
            long idCliente = _service.IdCliente;
            long idServicio = _service.Id;
            string hora = _service.Hour;
            string horaFin = _service.HourEnd;
            DateTime fecha = _service.FechaServicio;
            int idDireccion = _service.IdAdress;
            long[] idTecnicos = _service.IdTecnicos;

            VisitaApp _visita = new VisitaApp()
            {
                Cantidad = _service.Cantidad,
                Factura = false,
                Estatus = 2,
                FechaEntregaRefaccion = new DateTime(),
                FechaVisita = fecha,
                Garantia = false,
                IdDireccion = idDireccion,
                IdServicio = idServicio,
                Pagado = _service.Pagado,
                PagoPendiente = !_service.Pagado,
                Hora = hora,
                HoraFin = horaFin,
                TerminosCondiciones = true,
                FechaCancelacion = new DateTime(),
                FechaCompletado = new DateTime(),
                PreDiagnostico = false,
                AsignacionRefacciones = false
            };
            _context.VisitaApp.Add(_visita);
            _context.SaveChanges();


            List<RelTecnicoVisitaApp> _rel = new List<RelTecnicoVisitaApp>();
            foreach (var item in idTecnicos)
            {
                _rel.Add(new RelTecnicoVisitaApp() { IdTecnico = item, IdVista = _visita.Id });
            }
            //_rel.Take(1).FirstOrDefault().TecnicoResponsable = true;
            _context.RelTecnicoVisitaApp.AddRange(_rel);
            _context.SaveChanges();

            List<RelServicioCategoriaApp> _servicioRelacion = new List<RelServicioCategoriaApp>();
            for (int i = 0; i < _service.Productos.Count; i++)
            {
                _servicioRelacion.Add(new RelServicioCategoriaApp()
                {
                    IdServicio = idServicio,
                    IdVisita = _visita.Id,
                    IdSubLinea = Convert.ToInt32(_service.Productos[i].idProducto),
                    Cantidad = _service.Productos[i].cantidad
                });
            }
            _context.RelServicioCategoriaApp.AddRange(_servicioRelacion);
            _context.SaveChanges();

            List<ServicioFotosApp> _fotos = new List<ServicioFotosApp>();
            for (int i = 0; i < _service.fotos.Length; i++)
            {
                _fotos.Add(new ServicioFotosApp()
                {
                    IdVisitaApp = _visita.Id,
                    UrlFoto = _service.fotos[i].ToString()
                });
            }
            _context.ServicioFotosApp.AddRange(_fotos);
            _context.SaveChanges();

            var _user = _context.Clientes.FirstOrDefault(c => c.Id == _service.IdCliente);
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailAvisoVisita.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", $"{_user.Nombre} {_user.Paterno} {_user.Materno}");
            EmailService emailService = new EmailService();
            emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Confimacion de Visita agendada", To = _user.Email });

            _context.Notificaciones.Add(new Notificaciones()
            {
                Creado = DateTime.Now,
                Creadopor = 0,
                Descripcion = $"El servicio con el folio {idServicio} fue creado a través de la app de cliente y requiere un merge para el usuario {_user.Nombre} {_user.Paterno} {_user.Materno}.",
                EstatusLeido = false,
                RolNotificado = 10008,
                Evento = "Realizar Merge servicio pendiente",
                Url = "merge"
            });
            _context.SaveChanges();

            return true;
        }

        public servicioDetalle getCancelarServicio(bool app, long idVisita, string exp)
        {
            servicioDetalle _servicioDetalle = new servicioDetalle();
            if (app)
            {
                var _visitaApp = _context.VisitaApp.FirstOrDefault(c => c.Id == idVisita);
                _visitaApp.Estatus = 5;

                var _servicioApp = _context.ServicioApp.FirstOrDefault(c => c.Id == _visitaApp.IdServicio);
                _servicioApp.IdEstatusServicio = 13;

                _context.ServicioApp.Update(_servicioApp);
                _context.VisitaApp.Update(_visitaApp);
                _context.SaveChanges();
            }
            else
            {
                var _visita = _context.Visita.FirstOrDefault(c => c.Id == idVisita);
                _visita.Estatus = 5;

                var _servicio = _context.Servicio.FirstOrDefault(c => c.Id == _visita.IdServicio);
                _servicio.IdEstatusServicio = 13;

                _context.Servicio.Update(_servicio);
                _context.Visita.Update(_visita);
                _context.SaveChanges();
            }
            return _servicioDetalle;
        }
        public servicioDetalle getDetalleServicio(bool app, long idVisita)
        {
            servicioDetalle _servicioDetalle = new servicioDetalle();
            if (app)
            {
                VisitaApp _visitaApp = _context.VisitaApp.Where(c => c.Id == idVisita)
                    .Include("RelServicioCategoriaApp.IdSubLineaNavigation")
                    .Include("RelTecnicoVisitaApp.IdTecnicoNavigation")
                    .Include(c => c.IdServicioNavigation)
                    .ThenInclude(h => h.IdTipoServicioNavigation)
                    .FirstOrDefault();
                //var _direccion = _context.CatDireccion.FirstOrDefault(c=>c.Id == _visitaApp.IdDireccion);

                var _direccionConsulta = (from direccion in _context.CatDireccion
                                          join b in _context.CatEstado on direccion.IdEstado equals b.Id into _estado
                                          from estado in _estado.DefaultIfEmpty()
                                          join c in _context.CatMunicipio on direccion.IdMunicipio equals c.Id into _municipio
                                          from municipio in _municipio.DefaultIfEmpty()
                                          join d in _context.CatLocalidad on direccion.IdLocalidad equals d.Id into _localidad
                                          from localidad in _localidad.DefaultIfEmpty()
                                          where direccion.Id == _visitaApp.IdDireccion
                                          select new
                                          {
                                              direccion = (direccion.CalleNumero != null ? direccion.CalleNumero : "") + " " + (direccion.NumExt != null ? direccion.NumExt : "") + ", " + (localidad.DescLocalidad != null ? localidad.DescLocalidad : "") + ", " + (direccion.Cp != null ? direccion.Cp : "") + ", " + (estado.DescEstado != null ? estado.DescEstado : "") + ", " + (municipio.DescMunicipio != null ? municipio.DescMunicipio : ""),
                                          }).FirstOrDefault();

                List<productoDTO> _productos = new List<productoDTO>();
                foreach (var _servicioCategoria in _visitaApp.RelServicioCategoriaApp)
                {
                    _productos.Add(new productoDTO()
                    {
                        idProducto = _context.CatSubLineaProducto.FirstOrDefault(c => c.Id == _servicioCategoria.IdSubLinea).IdLineaProducto,
                        nombre = _servicioCategoria.IdSubLineaNavigation.Descripcion,
                        cantidad = _servicioCategoria.Cantidad
                    });
                }

                List<tecnicoDTO> _tecnicos = new List<tecnicoDTO>();
                foreach (var _tecnico in _visitaApp.RelTecnicoVisitaApp)
                {
                    Users _user = _context.Users.FirstOrDefault(c => c.Id == _tecnico.IdTecnico);
                    _tecnicos.Add(new tecnicoDTO()
                    {
                        idTecnico = _tecnico.IdTecnico,
                        nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                        avatar = _user.Avatar,
                        responsable = _tecnico.TecnicoResponsable,
                        automovil = _tecnico.IdTecnicoNavigation.VehiculoInfo,
                        placas = _tecnico.IdTecnicoNavigation.VehiculoPlacas
                    });
                }

                _servicioDetalle = new servicioDetalle()
                {
                    descripcion = _visitaApp.IdServicioNavigation.IdTipoServicioNavigation.DescTipoServicio,
                    direccion = _direccionConsulta.direccion,
                    fecha = _visitaApp.FechaVisita,
                    hora = _visitaApp.Hora + "," + _visitaApp.HoraFin,
                    productos = _productos,
                    tecnicos = _tecnicos
                };
            }
            else
            {
                Visita _visita = _context.Visita.Where(c => c.Id == idVisita)
                    .Include(c => c.RelServicioProducto)
                    .Include("RelTecnicoVisita.IdTecnicoNavigation")
                    .Include(c => c.IdServicioNavigation)
                    .FirstOrDefault();
                //var _direccion = _context.CatDireccion.FirstOrDefault(c=>c.Id == _visitaApp.IdDireccion);

                var _direccionConsulta = (from direccion in _context.CatDireccion
                                          join b in _context.CatEstado on direccion.IdEstado equals b.Id into _estado
                                          from estado in _estado.DefaultIfEmpty()
                                          join c in _context.CatMunicipio on direccion.IdMunicipio equals c.Id into _municipio
                                          from municipio in _municipio.DefaultIfEmpty()
                                          join d in _context.CatLocalidad on direccion.IdLocalidad equals d.Id into _localidad
                                          from localidad in _localidad.DefaultIfEmpty()
                                          where direccion.Id == _visita.IdDireccion
                                          select new
                                          {
                                              direccion = (direccion.CalleNumero != null ? direccion.CalleNumero : "") + " " + (direccion.NumExt != null ? direccion.NumExt : "") + ", " + (localidad.DescLocalidad != null ? localidad.DescLocalidad : "") + ", " + (direccion.Cp != null ? direccion.Cp : "") + ", " + (estado.DescEstado != null ? estado.DescEstado : "") + ", " + (municipio.DescMunicipio != null ? municipio.DescMunicipio : ""),
                                          }).FirstOrDefault();

                List<productoDTO> _productos = new List<productoDTO>();
                foreach (var _servicioCategoria in _visita.RelServicioProducto)
                {
                    var _idproducto = _context.ClienteProductos.Where(c => c.Id == _servicioCategoria.IdProducto).FirstOrDefault();
                    var _producto = _context.CatProductos.Where(c => c.Id == _idproducto.IdProducto).Include(c => c.IdSublineaNavigation).FirstOrDefault();
                    _productos.Add(new productoDTO()
                    {
                        idProducto = Convert.ToInt64(_producto.IdLinea),
                        nombre = _producto.IdSublineaNavigation.Descripcion,
                        cantidad = 1
                    });
                }

                List<tecnicoDTO> _tecnicos = new List<tecnicoDTO>();
                foreach (var _tecnico in _visita.RelTecnicoVisita)
                {
                    Users _user = _context.Users.FirstOrDefault(c => c.Id == _tecnico.IdTecnico);
                    _tecnicos.Add(new tecnicoDTO()
                    {
                        idTecnico = _tecnico.IdTecnico,
                        nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                        avatar = _user.Avatar,
                        responsable = _tecnico.TecnicoResponsable,
                        automovil = _tecnico.IdTecnicoNavigation.VehiculoInfo,
                        placas = _tecnico.IdTecnicoNavigation.VehiculoPlacas
                    });
                }

                _servicioDetalle = new servicioDetalle()
                {
                    descripcion = _visita.IdServicioNavigation.DescripcionActividades,
                    direccion = _direccionConsulta.direccion,
                    fecha = _visita.FechaVisita,
                    hora = _visita.Hora + "," + _visita.HoraFin,
                    encuesta = Convert.ToInt16(_visita.IdServicioNavigation.Encuesta),
                    productos = _productos,
                    tecnicos = _tecnicos
                };
            }
            return _servicioDetalle;
        }
        public void updateEncuesta(long _idVisita, int estatusEncuesta)
        {
            var _data = _context.Servicio.FirstOrDefault(c => c.Id == _context.Visita.FirstOrDefault(h => h.Id == _idVisita).IdServicio);
            _data.Encuesta = Convert.ToByte(estatusEncuesta);
            _context.Servicio.Update(_data);
            _context.SaveChanges();
        }

        public Servicio addService(Servicio _service, ServiceAddDTO service)
        {

            //Servicio _servicio = new Servicio();
            _context.Servicio.Add(_service);
            _context.SaveChanges();

            _service.NoServicio = _service.Id.ToString();
            _context.Servicio.Update(_service);
            _context.SaveChanges();

            service.Id = _service.Id;

            //divicion
            long idCliente = service.IdCliente;
            long idServicio = service.Id;
            string hora = service.Hour;
            string horaFin = service.HourEnd;
            DateTime fecha = service.FechaServicio;
            int idDireccion = service.IdAdress;
            long[] idTecnicos = service.IdTecnicos;

            Visita _visita = new Visita()
            {
                Cantidad = service.Cantidad,
                Factura = false,
                Estatus = 2,
                FechaEntregaRefaccion = new DateTime(),
                FechaVisita = fecha,
                Garantia = false,
                IdDireccion = idDireccion,
                IdServicio = idServicio,
                Pagado = service.Pagado,
                PagoPendiente = !service.Pagado,
                Hora = hora,
                HoraFin = horaFin,
                TerminosCondiciones = true,
                FechaCancelacion = new DateTime(),
                FechaCompletado = new DateTime(),
                PreDiagnostico = false,
                AsignacionRefacciones = false,
                VisitaPagada = service.Visita_pagada
            };
            _context.Visita.Add(_visita);
            _context.SaveChanges();


            List<RelTecnicoVisita> _rel = new List<RelTecnicoVisita>();
            foreach (var item in idTecnicos)
            {
                _rel.Add(new RelTecnicoVisita() { IdTecnico = item, IdVista = _visita.Id });
            }
            //_rel.Take(1).FirstOrDefault().TecnicoResponsable = true;
            _context.RelTecnicoVisita.AddRange(_rel);
            _context.SaveChanges();

            //List<RelServicioProducto> _servicioRelacion = new List<RelServicioProducto>();
            //for (int i = 0; i < service.Productos.Count; i++)
            //{
            //    var _producto = _context.CatProductos.FirstOrDefault(c => c.IdSublinea == Convert.ToInt32(service.Productos[i].idProducto));
            //    _servicioRelacion.Add(new RelServicioProducto()
            //    {
            //        Estatus = 0,
            //        Garantia = false,
            //        IdVista = _visita.Id,
            //        PrimeraVisita = true,
            //        Reparacion = 0,
            //        IdProducto = _producto.Id,
            //        IdCategoria = _producto.IdCategoria
            //    });
            //}
            //_context.RelServicioProducto.AddRange(_servicioRelacion);
            //_context.SaveChanges();

            long _estatusCompra = 0;
            if (service.IdTipoServicio == 1)
                _estatusCompra = 1007;
            else
                _estatusCompra = 1006;
            //version antes de tomar en cuenta de la cantidad
            //List<ClienteProductos> _clienteProducto = new List<ClienteProductos>();
            //for (int i = 0; i < service.Productos.Count; i++)
            //{
            //    var _producto = _context.CatProductos.FirstOrDefault(c => c.IdSublinea == Convert.ToInt32(service.Productos[i].idProducto));
            //    if (existeProductoCliente(idCliente, _producto.Id))
            //        _clienteProducto.Add(new ClienteProductos()
            //        {
            //            FechaCompra = DateTime.Now,
            //            IdCliente = idCliente,
            //            IdEsatusCompra = _estatusCompra,
            //            IdProducto = _producto.Id,
            //            Actualizado = DateTime.Now,
            //            Actualizadopor = 0,
            //            Creado = DateTime.Now,
            //            Creadopor = 0,
            //            IdCotizacion = 0,
            //            IdVisita = 0,
            //            NoProducto = 0,
            //            Garantia = false
            //        });
            //}
            List<ClienteProductos> _clienteProducto = new List<ClienteProductos>();
            for (int i = 0; i < service.Productos.Count; i++)
            {
                var _producto = _context.CatProductos.FirstOrDefault(c => c.IdSublinea == Convert.ToInt32(service.Productos[i].idProducto));
                for (int j = 0; j < existeProductoCliente(idCliente, _producto.Id, service.Productos[i].cantidad); j++)
                    _clienteProducto.Add(new ClienteProductos()
                    {
                        FechaCompra = DateTime.Now,
                        IdCliente = idCliente,
                        IdEsatusCompra = _estatusCompra,
                        IdProducto = _producto.Id,
                        Actualizado = DateTime.Now,
                        Actualizadopor = 0,
                        Creado = DateTime.Now,
                        Creadopor = 0,
                        IdCotizacion = 0,
                        IdVisita = 0,
                        NoProducto = 0,
                        Garantia = false
                    });
            }
            _context.ClienteProductos.AddRange(_clienteProducto);
            _context.SaveChanges();

            //version antes de tomar en cuenta de la cantidad
            //List<RelServicioProducto> _servicioRelacion = new List<RelServicioProducto>();
            //for (int i = 0; i < service.Productos.Count; i++)
            //{
            //    var _producto = _context.CatProductos.FirstOrDefault(c => c.IdSublinea == Convert.ToInt32(service.Productos[i].idProducto));
            //    _servicioRelacion.Add(new RelServicioProducto()
            //    {
            //        Estatus = 0,
            //        Garantia = false,
            //        IdVista = _visita.Id,
            //        PrimeraVisita = true,
            //        Reparacion = 0,
            //        IdProducto = getIdProductoCliente(idCliente, _producto.Id),
            //        IdCategoria = _producto.IdCategoria
            //    });
            //}
            List<RelServicioProducto> _servicioRelacion = new List<RelServicioProducto>();
            //for (int i = 0; i < service.Productos.Count; i++)
            //{
            var _productos = _context.ClienteProductos.Where(c => c.IdCliente == idCliente).ToList();

            foreach (var producto in _productos)
            {
                producto.CatProductos = _context.CatProductos.FirstOrDefault(c => c.Id == producto.IdProducto);
                var buscar = service.Productos.FirstOrDefault(c => c.idProducto == producto.CatProductos.IdSublinea);
                if (buscar != null)
                {
                    int cantidad = buscar.cantidad;
                    if (_servicioRelacion.Where(c => c.IdCategoria == producto.CatProductos.IdSublinea).Count() < cantidad)
                        _servicioRelacion.Add(new RelServicioProducto()
                        {
                            Estatus = 0,
                            Garantia = false,
                            IdVista = _visita.Id,
                            PrimeraVisita = true,
                            Reparacion = 0,
                            IdProducto = producto.Id,
                            IdCategoria = Convert.ToInt32(producto.CatProductos.IdSublinea)
                        });
                }
            }




            //}
            _context.RelServicioProducto.AddRange(_servicioRelacion);
            _context.SaveChanges();

            //List<ClienteProductos> _clienteProducto = new List<ClienteProductos>();
            //for (int i = 0; i < _servicioRelacion.Count; i++)
            //{
            //    if (existeProductoCliente(idCliente, _servicioRelacion[i].IdProducto))
            //        _clienteProducto.Add(new ClienteProductos()
            //        {
            //            FechaCompra = DateTime.Now,
            //            IdCliente = idCliente,
            //            IdEsatusCompra = _estatusCompra,
            //            IdProducto = _servicioRelacion[i].IdProducto,
            //            Actualizado = DateTime.Now,
            //            Actualizadopor = 0,
            //            Creado = DateTime.Now,
            //            Creadopor = 0,
            //            IdCotizacion = 0,
            //            IdVisita = 0,
            //            NoProducto = 0,
            //            Garantia = false
            //        });
            //}
            //_context.ClienteProductos.AddRange(_clienteProducto);
            //_context.SaveChanges();
            List<ServicioFotos> _fotos = new List<ServicioFotos>();
            for (int i = 0; i < service.fotos.Length; i++)
            {
                _fotos.Add(new ServicioFotos()
                {
                    IdVisita = _visita.Id,
                    UrlFoto = service.fotos[i].ToString()
                });
            }
            _context.ServicioFotos.AddRange(_fotos);
            _context.SaveChanges();

            var _user = _context.Clientes.FirstOrDefault(c => c.Id == service.IdCliente);
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailAvisoVisita.html"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            body = body.Replace("{username}", $"{_user.Nombre} {_user.Paterno} {_user.Materno}");
            EmailService emailService = new EmailService();
            emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Confimacion de Visita agendada", To = _user.Email });

            configLocal _config = getConfiguracionPettion();
            object _enviar = new { id = _visita.Id, id_servicio = _visita.IdServicio };
            PetitionHTTP _petition = new PetitionHTTP(_config.SendMail.url);
            _petition.loadPetition(_config.SendMail.controller, eMethodType.POST);
            for (int i = 0; i < _config.SendMail.headers.Count; i++)
            {
                _petition.addHeader(_config.SendMail.headers[i].key, _config.SendMail.headers[i].Value);
            }
            _petition.addBody(JsonConverter.Object_Json(_enviar));
            var response = _petition.makePetition();

            _context.Notificaciones.Add(new Notificaciones()
            {
                Creado = DateTime.Now,
                Creadopor = 0,
                Descripcion = "El servicio con el folio " + _service.Id + " fue agendado a través de la app de clientes.",
                EstatusLeido = false,
                RolNotificado = 10008,
                Evento = "Reagendar",
                Url = "editarservicio/" + _service.Id
            });
            _context.SaveChanges();

            return _service;
        }
        public int existeProductoCliente(long idCliente, long idProducto, int cantidad)
        {
            int _existe = 0;
            var encontrado = _context.ClienteProductos.Where(c => c.IdProducto == idProducto && c.IdCliente == idCliente).ToList();
            if (encontrado != null)
                _existe = cantidad - encontrado.Count();
            else
                _existe = cantidad;
            return _existe;
        }
        public bool existeProductoCliente(long idCliente, long idProducto)
        {
            bool _existe = false;
            var encontrado = _context.ClienteProductos.FirstOrDefault(c => c.IdProducto == idProducto && c.IdCliente == idCliente);
            if (encontrado == null)
                _existe = true;
            else
                _existe = false;
            return _existe;
        }
        public long getIdProductoCliente(long idCliente, long idProducto)
        {
            long _id = 0;
            var encontrado = _context.ClienteProductos.FirstOrDefault(c => c.IdProducto == idProducto && c.IdCliente == idCliente);
            if (encontrado == null)
                _id = 0;
            else
                _id = encontrado.Id;
            return _id;
        }
        public List<CotizacionDTO> get_cotizacion(int id_visita)
        {
            List<CotizacionDTO> _consult = new List<CotizacionDTO>();
            var _visita = _context.Visita
                .Select(c => c)
                .Include(c => c.RelServicioProducto)
                .Include(c => c.IdServicioNavigation)
                .FirstOrDefault(c => c.Id == Convert.ToInt64(id_visita));

            if (_visita == null || _visita.Pagado)
            {
                //11860
                //id_visita = 11860;
                _consult = (from d in _context.RelServicioRefaccion
                            join e in _context.PiezasRepuesto on d.Id equals e.IdRelServicioRefaccion
                            join f in _context.CatMateriales on e.IdMaterial equals f.Id
                            join h in _context.CatListaPrecios on f.IdGrupoPrecio equals h.Id
                            join i in _context.ClienteProductos on d.IdProducto equals i.Id
                            join j in _context.CatProductos on i.IdProducto equals j.Id
                            where d.IdVista == id_visita
                            select new CotizacionDTO
                            {
                                id = d.Id,
                                id_materia = f.Id,
                                refaccion = f.Descripcion,
                                cantidad = e.Cantidad,
                                precio_sin_iva = h.PrecioSinIva,
                                garantia = e.Garantia,
                                total_cantidad = (from a in _context.PiezasRepuesto
                                                  join b in _context.RelServicioRefaccion on a.IdRelServicioRefaccion equals b.Id
                                                  where b.IdVista == id_visita
                                                  select a.Cantidad).Sum(),
                                total_precio = (from a in _context.PiezasRepuesto
                                                join aa in _context.RelServicioRefaccion on a.IdRelServicioRefaccion equals aa.Id
                                                join b in _context.CatMateriales on a.IdMaterial equals b.Id
                                                join c in _context.CatListaPrecios on b.IdGrupoPrecio equals Convert.ToInt32(c.GrupoPrecio)
                                                where aa.IdVista == id_visita
                                                select c.PrecioSinIva).Sum(),
                                reporte_visita = d.IdVistaNavigation.UrlPpdfReporte.Replace("Imagenes/", ""),
                                reporte_cotizacion = d.IdVistaNavigation.UrlPdfCotizacion.Replace("Imagenes/", ""),
                                mano_de_obra = (from mo in _context.RelCategoriaProductoTipoProducto
                                                where mo.IdCategoria == j.IdCategoria && mo.IdTipoServicio == d.IdVistaNavigation.IdServicioNavigation.IdTipoServicio
                                                select new ManObraDTO
                                                {
                                                    precio_hora_tecnico = Math.Round(Convert.ToDecimal(mo.PrecioHoraTecnico / 1.16), 2),
                                                    PrecioVisita = mo.PrecioVisita.Value
                                                }).Take(1).ToList()

                            }).ToList();
                if (_consult.Count > 0)
                    if (_consult[0].mano_de_obra.Count > 0)
                        _consult.Add(new CotizacionDTO()
                        {
                            cantidad = 1,
                            refaccion = "Costo por reparación",
                            precio_sin_iva = _consult[0].mano_de_obra[0].PrecioVisita
                        });
                    else
                        _consult.Add(new CotizacionDTO()
                        {
                            cantidad = 1,
                            refaccion = "Costo por reparación",
                            precio_sin_iva = 0
                        });
            }
            else
            {
                int[] _categoria = new int[_visita.RelServicioProducto.Count];
                int i = 0;
                foreach (var item in _visita.RelServicioProducto)
                {
                    _categoria[i] = item.IdCategoria;
                    i++;
                }
                var _hora = HoraServicio(_visita.IdServicioNavigation.IdTipoServicio, _categoria, _visita.IdDireccion);
                for (int k = 0; k < _hora.Count; k++)
                {
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Costo Visita",
                        precio_sin_iva = Convert.ToDecimal(_hora[k].PrecioVisita),
                        //precio_sin_iva = Convert.ToDecimal(_hora[i].PrecioVisita + (_hora[i].PrecioHoraTecnico * Convert.ToInt32(_hora[i].HorasTecnicos))),
                    });
                }
                if (_hora.Count > 0)
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Viaticos",
                        precio_sin_iva = Convert.ToDecimal(_hora[0].Viaticos),
                    });
            }
            return _consult;
        }


        public bool updateCotization(ServiceAddDTO _service)
        {
            bool transaction = false;
            var _visita = _context.Visita.FirstOrDefault(c => c.Id == _service.Id);
            if (_visita != null)
            {
                _visita.FechaVisita = _service.FechaServicio;
                _visita.Hora = _service.Hour;
                _visita.HoraFin = _service.HourEnd;
                _visita.CotizacionPagada = true;
                _visita.Pagado = true;
                _visita.PagoPendiente = false;
                _context.Visita.Update(_visita);
                _context.SaveChanges();
            }
            else
                transaction = true;

            return transaction;
        }



        public IActionResult get_mis_productos(int id)
        {

            var _consult = (from a in _context.CatLineaProducto
                            where a.IdSuperlinea == id
                            select new
                            {
                                a.Id,
                                a.Descripcion,
                                sub_linea = (from c in _context.CatSubLineaProducto
                                             where c.IdLineaProducto == a.Id && c.ShowApp == true
                                             select new
                                             {
                                                 c.Id,
                                                 c.Descripcion
                                             })
                            }).ToList(); //_context.CatSubLineaProducto.Include(x => x.IdLineaProductoNavigation).ThenInclude(y => y.IdSuperlineaNavigation).FirstOrDefault(x => x.IdLineaProductoNavigation.IdSuperlinea == id);

            return new ObjectResult(_consult);
        }

        //pasar a clientes sorry rodri se me paso que los hice aqui
        public Clientes getProfile(Clientes _cliente)
        {
            var _clienteLocal = _context.Clientes.Where(c => c.Id == _cliente.Id).Include(c => c.DatosFiscales).FirstOrDefault();
            if (_clienteLocal.DatosFiscales.Count == 0)
            {
                _clienteLocal.DatosFiscales.Add(new DatosFiscales()
                {
                    Cp = "",
                    Email = "",
                    Colonia = "",
                    RazonSocial = "",
                    Rfc = "",
                    CalleNumero = "",
                    IdCliente = _cliente.Id
                });
            }
            return _clienteLocal;
        }
        public Clientes setProfile(Clientes _cliente)
        {
            var _relUserClient = _context.UserClientsApp.Where(c => c.IdClient == _cliente.Id).Include(c => c.IdUserNavigation).FirstOrDefault();
            if (_relUserClient != null)
            {
                Users _user = _relUserClient.IdUserNavigation;
                _user.Name = _cliente.Nombre;
                _user.Materno = _cliente.Materno;
                _user.Paterno = _cliente.Paterno;
                _user.Telefono = _cliente.Telefono;
                _user.TelefonoMovil = _cliente.TelefonoMovil;
                _user.Email = _cliente.Email;
                _context.Users.Update(_user);
            }

            var _clienteLocal = _context.Clientes.FirstOrDefault(c => c.Id == _cliente.Id);
            _clienteLocal.Nombre = _cliente.Nombre;
            _clienteLocal.Materno = _cliente.Materno;
            _clienteLocal.Paterno = _cliente.Paterno;
            _clienteLocal.Telefono = _cliente.Telefono;
            _clienteLocal.TelefonoMovil = _cliente.TelefonoMovil;
            _clienteLocal.Email = _cliente.Email;
            _context.Clientes.Update(_clienteLocal);

            var _datosFiscales = _context.DatosFiscales.FirstOrDefault(c => c.Id == _cliente.DatosFiscales.First().Id);
            if (_datosFiscales == null)
            {
                _cliente.DatosFiscales.First().IdCliente = _cliente.Id;
                _context.DatosFiscales.Add(_cliente.DatosFiscales.First());
            }
            else
            {
                _datosFiscales.RazonSocial = _cliente.DatosFiscales.First().RazonSocial;
                _datosFiscales.Rfc = _cliente.DatosFiscales.First().Rfc;
                _datosFiscales.Email = _cliente.DatosFiscales.First().Email;
                _datosFiscales.Cp = _cliente.DatosFiscales.First().Cp;
                _datosFiscales.Colonia = _cliente.DatosFiscales.First().Colonia;
                _datosFiscales.CalleNumero = _cliente.DatosFiscales.First().CalleNumero;
                _datosFiscales.IdEstado = _cliente.DatosFiscales.First().IdEstado;
                _datosFiscales.IdMunicipio = _cliente.DatosFiscales.First().IdMunicipio;
                _datosFiscales.IntFact = _cliente.DatosFiscales.First().IntFact;
                _datosFiscales.ExtFact = _cliente.DatosFiscales.First().ExtFact;

                _context.DatosFiscales.Update(_datosFiscales);
            }
            _context.SaveChanges();
            return _context.Clientes.Where(c => c.Id == _cliente.Id).Include(c => c.DatosFiscales).FirstOrDefault();
        }
        //pasar a clientes sorry rodri se me paso que los hice aqui

        public Visita getDetalleAllServicio(long idVisita)
        {
            Visita _cotizacion = _context.Visita.Where(c => c.Id == idVisita).Include(c => c.IdServicioNavigation).FirstOrDefault();
            return _cotizacion;
        }
        public bool updateVisita(long id, bool app)
        {
            if (app)
            {
                var _visita = _context.VisitaApp.FirstOrDefault(c => c.Id == id);
                _visita.Pagado = true;
                _visita.PagoPendiente = false;
                //long _idServicio = _context.Visita.FirstOrDefault(c => c.Id == id).IdServicio;
                _context.VisitaApp.Update(_visita);
                _context.SaveChanges();
            }
            else
            {
                var _visita = _context.Visita.FirstOrDefault(c => c.Id == id);
                _visita.Pagado = true;
                _visita.PagoPendiente = false;
                _context.Visita.Update(_visita);
                _context.SaveChanges();
            }
            return true;
        }

        public decimal total_horas_servicio(dto_total_horas dto)
        {
            decimal y = 0;
            for (int x = 0; x < dto.productos.Count(); x++) {
                 y = y + Convert.ToDecimal(_context.RelCategoriaProductoTipoProducto.FirstOrDefault(c => c.IdTipoServicio == dto.tipo_servicio && c.IdCategoria == dto.productos[x].id_sublinea).HorasTecnicos) * Convert.ToDecimal(dto.productos[x].cantidad);
            };

            return Math.Round(y, 0);
        }


        #region restructura

        public List<clsServicioDTO> getAllServiceByClient(long _idClient, bool _general)
        {
            List<clsServicioDTO> _servicios = new List<clsServicioDTO>();

            //RelUserUserApp _ClientWeb = _context.RelUserUserApp.FirstOrDefault(c => c.IdClientApp == _idClient);
            //long? _idClientWeb = 0;
            //if (_ClientWeb != null)
            //    _idClientWeb = _ClientWeb.IdClient;

            List<clsServicioDTO> _serviciosWEB = new List<clsServicioDTO>();
            //if (_idClientWeb != 0)
            _serviciosWEB = (from servicio in _context.Servicio
                             join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                             join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                             where servicio.IdCliente == _idClient
                             select new clsServicioDTO
                             {
                                 IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                 DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                 IdServicio = Convert.ToInt32(servicio.Id),
                                 IdTipoServicio = servicio.IdTipoServicio,
                                 DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                 DescripcionServicio = servicio.DescripcionActividades,
                                 app = false,
                                 EstatusEncuesta = Convert.ToByte(servicio.Encuesta),
                                 Visitas = (from visita in _context.Visita
                                            join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id into visitaDTO
                                            from Visita in visitaDTO.DefaultIfEmpty()
                                            where visita.IdServicio == servicio.Id
                                            select new clsVisitaDTO
                                            {
                                                IdVisita = Convert.ToInt32(visita.Id),
                                                IdEstatusVisita = Convert.ToInt32(visita.Estatus) == 0 ? 2 : Convert.ToInt32(visita.Estatus),
                                                DescripcionEstatus = Visita.DescEstatusVisita.ToUpper(),
                                                EstatusProductos = (from relServicioRefaccion in _context.RelServicioRefaccion
                                                                    where relServicioRefaccion.IdVista == visita.Id
                                                                    select relServicioRefaccion.Estatus).ToArray(),
                                                CotizacionPagada = visita.CotizacionPagada,
                                                VisitaPagada = visita.Pagado,
                                                Semaforo = getEstatusSemaforoVisita(Convert.ToInt32(visita.Estatus)),
                                                IdDireccion = visita.IdDireccion,
                                                Productos = (from relServicioProducto in _context.RelServicioProducto
                                                             where relServicioProducto.IdVista == visita.Id
                                                             select new clsProductoDTO
                                                             {
                                                                 Garantia = relServicioProducto.Garantia,
                                                                 Estatusproducto = (from relServicioRefaccion in _context.RelServicioRefaccion
                                                                                    where relServicioRefaccion.IdVista == visita.Id && relServicioRefaccion.IdProducto == relServicioProducto.IdProducto
                                                                                    select relServicioRefaccion.Estatus).FirstOrDefault(),
                                                                 visita_estatus = Convert.ToInt32(visita.Estatus)
                                                             }).ToList()
                                            }).ToList()
                             }).ToList();
            var _serviciosApp = (from servicio in _context.ServicioApp
                                 join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                                 join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                                 where servicio.IdCliente == _idClient
                                 select new clsServicioDTO
                                 {
                                     IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                     DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                     IdServicio = Convert.ToInt32(servicio.Id),
                                     IdTipoServicio = servicio.IdTipoServicio,
                                     DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                     DescripcionServicio = servicio.DescripcionActividades,
                                     app = true,
                                     EstatusEncuesta = 0,
                                     Visitas = (from visita in _context.VisitaApp
                                                join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id
                                                where visita.IdServicio == servicio.Id
                                                select new clsVisitaDTO
                                                {
                                                    IdVisita = Convert.ToInt32(visita.Id),
                                                    IdEstatusVisita = Convert.ToInt32(visita.Estatus),
                                                    DescripcionEstatus = estatusVisita.DescEstatusVisita.ToUpper(),
                                                    EstatusProductos = new int[1],
                                                    IdDireccion = visita.IdDireccion,
                                                    CotizacionPagada = false,
                                                    VisitaPagada = visita.Pagado,
                                                    Semaforo = getEstatusSemaforoVisita(Convert.ToInt32(visita.Estatus)),
                                                    Productos = (from relServicioCategoriaApp in _context.RelServicioCategoriaApp
                                                                 where relServicioCategoriaApp.IdVisita == visita.Id
                                                                 select new clsProductoDTO
                                                                 {
                                                                     Garantia = false,
                                                                     Estatusproducto = 0,
                                                                     visita_estatus = Convert.ToInt32(visita.Estatus)
                                                                 }).ToList()
                                                }).ToList()
                                 }).ToList();

            for (int i = 0; i < _serviciosWEB.Count; i++)
            {
                _servicios.Add(new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosWEB[i].IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosWEB[i].DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosWEB[i].IdServicio),
                    IdTipoServicio = _serviciosWEB[i].IdTipoServicio,
                    DescripcionTipoServicio = _serviciosWEB[i].DescripcionTipoServicio,
                    DescripcionServicio = _serviciosWEB[i].DescripcionServicio,
                    app = _serviciosWEB[i].app,
                    EstatusEncuesta = Convert.ToByte(_serviciosWEB[i].EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosWEB[i].Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosWEB[i].IdEstatusServicio)),
                    Visitas = _serviciosWEB[i].Visitas.OrderBy(c => c.IdVisita).ToList()
                });
            }
            for (int i = 0; i < _serviciosApp.Count; i++)
            {
                _servicios.Add(new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosApp[i].IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosApp[i].DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosApp[i].IdServicio),
                    IdTipoServicio = _serviciosApp[i].IdTipoServicio,
                    DescripcionTipoServicio = _serviciosApp[i].DescripcionTipoServicio,
                    DescripcionServicio = _serviciosApp[i].DescripcionServicio,
                    app = _serviciosApp[i].app,
                    EstatusEncuesta = Convert.ToByte(_serviciosApp[i].EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosApp[i].Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosApp[i].IdEstatusServicio)),
                    Visitas = _serviciosApp[i].Visitas.OrderBy(c => c.IdVisita).ToList()
                });
            }
            if (!_general)
                _servicios = _servicios.Where(c => c.IdEstatusServicio < 15).ToList();
            _servicios = _servicios.OrderBy(c => c.IdServicio).ToList();
            return _servicios;
        }
        public clsVisitaServicioDTO getAllDetalleVisita(long _idVisita, bool _app)
        {
            clsVisitaServicioDTO _servicioDetalle = new clsVisitaServicioDTO();
            if (_app)
            {

                VisitaApp _visitaApp = _context.VisitaApp.Where(c => c.Id == _idVisita)
                    .Include("RelServicioCategoriaApp.IdSubLineaNavigation")
                    .Include("RelTecnicoVisitaApp.IdTecnicoNavigation")
                    .Include(c => c.IdServicioNavigation)
                    .ThenInclude(h => h.IdTipoServicioNavigation)
                    .FirstOrDefault();
                var _direccionConsulta = (from direccion in _context.CatDireccion
                                          join b in _context.CatEstado on direccion.IdEstado equals b.Id into _estado
                                          from estado in _estado.DefaultIfEmpty()
                                          join c in _context.CatMunicipio on direccion.IdMunicipio equals c.Id into _municipio
                                          from municipio in _municipio.DefaultIfEmpty()
                                          join d in _context.CatLocalidad on direccion.IdLocalidad equals d.Id into _localidad
                                          from localidad in _localidad.DefaultIfEmpty()
                                          where direccion.Id == _visitaApp.IdDireccion
                                          select new
                                          {
                                              direccion = getValString(direccion.CalleNumero) + " " + getValString(direccion.NumExt) + ", " + getValString(localidad.DescLocalidad) + ", " + getValString(direccion.Cp) + ", " + getValString(estado.DescEstado) + ", " + getValString(municipio.DescMunicipio),
                                          }).FirstOrDefault();

                List<productoDTO> _productos = new List<productoDTO>();
                foreach (var _servicioCategoria in _visitaApp.RelServicioCategoriaApp)
                {
                    _productos.Add(new productoDTO()
                    {
                        idProducto = _context.CatSubLineaProducto.FirstOrDefault(c => c.Id == _servicioCategoria.IdSubLinea).IdLineaProducto,
                        nombre = _servicioCategoria.IdSubLineaNavigation.Descripcion,
                        cantidad = _servicioCategoria.Cantidad,
                        descEstatus = "En espera de Visita",
                        estatus = 0,
                        mano_de_obra = (from mo in _context.RelCategoriaProductoTipoProducto
                                        where mo.IdCategoria == _servicioCategoria.IdSubLinea && mo.IdTipoServicio == _visitaApp.IdServicioNavigation.IdTipoServicio
                                        select new ManObraDTO
                                        {
                                            precio_hora_tecnico = Math.Round(Convert.ToDecimal(mo.PrecioHoraTecnico / 1.16), 2),
                                            PrecioVisita = mo.PrecioVisita.Value
                                        }).Take(1).ToList()
                    });
                }

                decimal _SubTotalProductos = 0M;
                foreach (var producto in _productos)
                {
                    
                    if (producto.mano_de_obra.Count > 0)
                    {
                        producto.SubtotalProducto += producto.mano_de_obra[0].precio_hora_tecnico;
                        _SubTotalProductos += producto.SubtotalProducto;
                    }

                }

                List<tecnicoDTO> _tecnicos = new List<tecnicoDTO>();
                foreach (var _tecnico in _visitaApp.RelTecnicoVisitaApp)
                {
                    Users _user = _context.Users.FirstOrDefault(c => c.Id == _tecnico.IdTecnico);
                    _tecnicos.Add(new tecnicoDTO()
                    {
                        idTecnico = _tecnico.IdTecnico,
                        nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                        avatar = _user.Avatar,
                        responsable = _tecnico.TecnicoResponsable,
                        automovil = _tecnico.IdTecnicoNavigation.VehiculoInfo,
                        placas = _tecnico.IdTecnicoNavigation.VehiculoPlacas
                    });
                }
                var _servicioDTO = getAllServiceByService(_visitaApp.IdServicio, _app);
                var _cotizacionVisita = getAllCotizationByVisita(_visitaApp.Id, _app);

                _servicioDetalle = new clsVisitaServicioDTO()
                {
                    IdEstatusServicio = _servicioDTO.IdEstatusServicio,
                    DescripcionEstatusServicio = _servicioDTO.DescripcionEstatusServicio,
                    IdServicio = _servicioDTO.IdServicio,
                    IdTipoServicio = _servicioDTO.IdTipoServicio,
                    DescripcionTipoServicio = _servicioDTO.DescripcionTipoServicio,
                    DescripcionServicio = _servicioDTO.DescripcionServicio,
                    app = _servicioDTO.app,
                    EstatusEncuesta = _servicioDTO.EstatusEncuesta,
                    DescripcionEstatusProductos = getMensajeSemaforo(_servicioDTO.Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(_servicioDTO.IdEstatusServicio),
                    SemaforoVisita = getEstatusSemaforoVisita(Convert.ToInt32(_visitaApp.Estatus)),
                    descripcion = _visitaApp.IdServicioNavigation.IdTipoServicioNavigation.DescTipoServicio,
                    direccion = _direccionConsulta.direccion,
                    IdDireccion = _visitaApp.IdDireccion,
                    fecha = _visitaApp.FechaVisita,
                    hora = _visitaApp.Hora + "," + _visitaApp.HoraFin,
                    productos = _productos,
                    tecnicos = _tecnicos,
                    costo = _cotizacionVisita,
                    pagadoVisita = _visitaApp.Pagado,
                    EstatusProductos = new int[1],
                    idEstatusVisita = Convert.ToInt32(_visitaApp.Estatus),
                    isAgendado = Convert.ToBoolean(_visitaApp.Regendada),
                    TotalVisita = _SubTotalProductos + getViatico(_visitaApp.IdDireccion),
                    visita_pagada = _visitaApp.Cantidad
    };
                _servicioDetalle.Iva = _servicioDetalle.TotalVisita * .16M;
                _servicioDetalle.TotalVisita = _servicioDetalle.TotalVisita + _servicioDetalle.Iva;
            }
            else
            {
                Visita _visita = _context.Visita.Where(c => c.Id == _idVisita)
                    .Include(c => c.RelServicioProducto)
                    .Include("RelTecnicoVisita.IdTecnicoNavigation")
                    .Include(c => c.IdServicioNavigation)
                    .ThenInclude(h => h.IdTipoServicioNavigation)
                    .Include(c => c.RelServicioRefaccion)
                    .FirstOrDefault();
                //var _direccion = _context.CatDireccion.FirstOrDefault(c=>c.Id == _visitaApp.IdDireccion);

                var _direccionConsulta = (from direccion in _context.CatDireccion
                                          join b in _context.CatEstado on direccion.IdEstado equals b.Id into _estado
                                          from estado in _estado.DefaultIfEmpty()
                                          join c in _context.CatMunicipio on direccion.IdMunicipio equals c.Id into _municipio
                                          from municipio in _municipio.DefaultIfEmpty()
                                          join d in _context.CatLocalidad on direccion.IdLocalidad equals d.Id into _localidad
                                          from localidad in _localidad.DefaultIfEmpty()
                                          where direccion.Id == _visita.IdDireccion
                                          select new
                                          {
                                              direccion = getValString(direccion.CalleNumero) + " " + getValString(direccion.NumExt) + ", " + getValString(localidad.DescLocalidad) + ", " + getValString(direccion.Cp) + ", " + getValString(estado.DescEstado) + ", " + getValString(municipio.DescMunicipio),
                                          }).FirstOrDefault();

                List<productoDTO> _productos = new List<productoDTO>();
                foreach (var _servicioCategoria in _visita.RelServicioProducto)
                {
                    var _idproducto = _context.ClienteProductos.Where(c => c.Id == _servicioCategoria.IdProducto).FirstOrDefault();
                    var _estatusProducto = _context.RelServicioRefaccion.FirstOrDefault(c => c.IdVista == _servicioCategoria.IdVista && c.IdProducto == _servicioCategoria.IdProducto);
                    var _producto = _context.CatProductos.Where(c => c.Id == _idproducto.IdProducto).Include(c => c.IdSublineaNavigation).FirstOrDefault();
                    _productos.Add(new productoDTO()
                    {
                        idProducto = Convert.ToInt64(_producto.IdLinea),
                        nombre = _producto.IdSublineaNavigation.Descripcion,
                        cantidad = 1,
                        estatus = _estatusProducto != null ? _estatusProducto.Estatus : 0,
                        descEstatus = _estatusProducto != null ? (_estatusProducto.Estatus != 0 ? _context.CatEstatusProducto.FirstOrDefault(c => c.Id == _estatusProducto.Estatus).DescEstatusProducto : "En espera de Visita") : "En espera de Visita",
                        mano_de_obra = (from mo in _context.RelCategoriaProductoTipoProducto
                                        where mo.IdCategoria == _producto.IdSublinea && mo.IdTipoServicio == _visita.IdServicioNavigation.IdTipoServicio
                                        select new ManObraDTO
                                        {
                                            precio_hora_tecnico = Math.Round(Convert.ToDecimal(mo.PrecioHoraTecnico / 1.16), 2),
                                            PrecioVisita = mo.PrecioVisita.Value
                                        }).Take(1).ToList(),
                        garantiaProducto = (from a in _context.RelServicioProducto
                                            where a.IdVista == _servicioCategoria.IdVista && a.IdProducto == _idproducto.Id
                                            select a.Garantia).First(),
                        Cotizacion = (from d in _context.RelServicioRefaccion
                                      join e in _context.PiezasRepuesto on d.Id equals e.IdRelServicioRefaccion
                                      join f in _context.CatMateriales on e.IdMaterial equals f.Id
                                      join h in _context.CatListaPrecios on f.IdGrupoPrecio equals h.Id
                                      join i in _context.ClienteProductos on d.IdProducto equals i.Id
                                      join j in _context.CatProductos on i.IdProducto equals j.Id
                                      where d.IdVista == _servicioCategoria.IdVista && d.IdProducto == _idproducto.Id
                                      select new CotizacionDTO
                                      {
                                          id = d.Id,
                                          id_materia = f.Id,
                                          refaccion = f.Descripcion,
                                          cantidad = e.Cantidad,
                                          precio_sin_iva = e.Garantia ? 0 : h.PrecioSinIva,
                                          garantia = e.Garantia,
                                          garantiaProducto = (from a in _context.RelServicioProducto
                                                              where a.IdVista == _servicioCategoria.IdVista && a.IdProducto == _idproducto.Id
                                                              select a.Garantia).First(),
                                          total_cantidad = (from a in _context.PiezasRepuesto
                                                            join b in _context.RelServicioRefaccion on a.IdRelServicioRefaccion equals b.Id
                                                            where b.IdVista == _servicioCategoria.IdVista && b.IdProducto == _idproducto.Id
                                                            select a.Cantidad).Sum(),
                                          total_precio = (from a in _context.PiezasRepuesto
                                                          join aa in _context.RelServicioRefaccion on a.IdRelServicioRefaccion equals aa.Id
                                                          join b in _context.CatMateriales on a.IdMaterial equals b.Id
                                                          join c in _context.CatListaPrecios on b.IdGrupoPrecio equals Convert.ToInt32(c.GrupoPrecio)
                                                          where aa.IdVista == _servicioCategoria.IdVista && aa.IdProducto == _idproducto.Id
                                                          select c.PrecioSinIva).Sum(),
                                          reporte_visita = d.IdVistaNavigation.UrlPpdfReporte.Replace("Imagenes/", ""),
                                          reporte_cotizacion = d.IdVistaNavigation.UrlPdfCotizacion.Replace("Imagenes/", "")
                                          //SubTotal = (e.Garantia ? 0 : h.PrecioSinIva) * e.Cantidad
                                      }).ToList()
                    });
                }
                decimal _SubTotalProductos = 0M;
                foreach (var producto in _productos)
                {
                    decimal _suma = 0M;
                    bool garantiaLocal = false;
                    foreach (var refaccion in producto.Cotizacion)
                    {
                        if (!refaccion.garantia && !refaccion.garantiaProducto)
                        {
                            refaccion.SubTotal = refaccion.precio_sin_iva * refaccion.cantidad;
                            _suma += refaccion.SubTotal;
                        }
                    }
                    garantiaLocal = producto.garantiaProducto;
                    producto.SubtotalRefacciones = _suma;
                    producto.SubtotalProducto = _suma;
                    if (producto.mano_de_obra.Count > 0 && !garantiaLocal)
                    {
                        producto.SubtotalProducto += producto.mano_de_obra[0].precio_hora_tecnico;
                        _SubTotalProductos += producto.SubtotalProducto;
                    }


                }
                List<tecnicoDTO> _tecnicos = new List<tecnicoDTO>();
                foreach (var _tecnico in _visita.RelTecnicoVisita)
                {
                    Users _user = _context.Users.FirstOrDefault(c => c.Id == _tecnico.IdTecnico);
                    _tecnicos.Add(new tecnicoDTO()
                    {
                        idTecnico = _tecnico.IdTecnico,
                        nombreTecnico = _user.Name + " " + _user.Paterno + " " + _user.Materno,
                        avatar = _user.Avatar,
                        responsable = _tecnico.TecnicoResponsable,
                        automovil = _tecnico.IdTecnicoNavigation.VehiculoInfo,
                        placas = _tecnico.IdTecnicoNavigation.VehiculoPlacas
                    });
                }

                var _servicioDTO = getAllServiceByService(_visita.IdServicio, _app);
                var _cotizacionVisita = getAllCotizationByVisita(_visita.Id, _app);
                var _estatusProductos = (_context.RelServicioRefaccion.Where(c => c.IdVista == _visita.Id).Select(c => c.Estatus)).ToArray();

                _servicioDetalle = new clsVisitaServicioDTO()
                {
                    IdEstatusServicio = _servicioDTO.IdEstatusServicio,
                    DescripcionEstatusServicio = _servicioDTO.DescripcionEstatusServicio,
                    IdServicio = _servicioDTO.IdServicio,
                    IdTipoServicio = _servicioDTO.IdTipoServicio,
                    DescripcionTipoServicio = _servicioDTO.DescripcionTipoServicio,
                    DescripcionServicio = _servicioDTO.DescripcionServicio,
                    app = _servicioDTO.app,
                    EstatusEncuesta = _servicioDTO.EstatusEncuesta,
                    DescripcionEstatusProductos = getMensajeSemaforo(_servicioDTO.Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(_servicioDTO.IdEstatusServicio),
                    SemaforoVisita = getEstatusSemaforoVisita(Convert.ToInt32(_visita.Estatus)),
                    descripcion = _visita.IdServicioNavigation.IdTipoServicioNavigation.DescTipoServicio,
                    direccion = _direccionConsulta.direccion == null ? "" : _direccionConsulta.direccion,
                    IdDireccion = _visita.IdDireccion,
                    fecha = _visita.FechaInicioVisita == null ? _visita.FechaVisita : _visita.FechaInicioVisita.Value,
                    hora = _visita.Hora + "," + _visita.HoraFin,
                    productos = _productos,
                    subTotalProductos = _SubTotalProductos,
                    TotalVisita = !_visita.Pagado || !_visita.CotizacionPagada ? _SubTotalProductos + getViatico(_visita.IdDireccion) : 0M,
                    Viaticos = getViatico(_visita.IdDireccion),
                    tecnicos = _tecnicos,
                    costo = _cotizacionVisita,
                    pagadoVisita = _visita.Pagado,
                    cotizacionPagada = _visita.CotizacionPagada,
                    EstatusProductos = _estatusProductos,
                    idEstatusVisita = Convert.ToInt32(_visita.Estatus),
                    urlReporteVisita = _visita.UrlPpdfReporte != null ? _visita.UrlPpdfReporte.Replace("Imagenes/", "") : "",
                    urlReporteCotizacion = _visita.UrlPdfCotizacion != null ? _visita.UrlPdfCotizacion.Replace("Imagenes/", "") : "",
                    isAgendado = Convert.ToBoolean(_visita.Regendada),
                    visita_pagada = _visita.Cantidad
    };
            }
            _servicioDetalle.Iva = _servicioDetalle.TotalVisita * .16M;
            _servicioDetalle.TotalVisita = _servicioDetalle.TotalVisita + _servicioDetalle.Iva;
            return _servicioDetalle;
        }
        public bool pagarCotizacion(long _idVisita)
        {
            bool _estatus = true;
            var _productos = _context.RelServicioRefaccion.Where(c => c.IdVista == _idVisita && c.Estatus == 3).ToList();
            for (int i = 0; i < _productos.Count; i++)
            {
                _productos[i].Estatus = 1;
            }

            var _visita = _context.Visita.FirstOrDefault(c => c.Id == _idVisita);
            _visita.CotizacionPagada = true;

            _context.Visita.Update(_visita);
            _context.RelServicioRefaccion.UpdateRange(_productos);
            _context.SaveChanges();

            _context.Notificaciones.Add(new Notificaciones()
            {
                Creado = DateTime.Now,
                Creadopor = 0,
                Descripcion = "La cotización del servicio con el folio " + _visita.IdServicio + " fue pagada a través de la app de clientes.",
                EstatusLeido = false,
                RolNotificado = 10010,
                Evento = "solicitar refacciones de la cotizacion",
                Url = "editarservicio/" + _visita.IdServicio
            });

            return _estatus;
        }
        public int saveCotization(ServiceAddDTO idVisita)
        {
            int transaction = 0;
            var _visita = _context.Visita
                            .Where(c => c.Id == idVisita.Id)
                            .Include(c => c.RelTecnicoVisita)
                            .Include(c => c.RelServicioProducto)
                            .Include(c => c.RelServicioRefaccion)
                            .FirstOrDefault();

            _visita.Estatus = 4;

            Visita _newVisita = new Visita()
            {
                Cantidad = _visita.Cantidad,
                Factura = _visita.Factura,
                Estatus = 2,
                FechaEntregaRefaccion = _visita.FechaEntregaRefaccion,
                FechaVisita = idVisita.FechaServicio,
                Hora = idVisita.Hour,
                HoraFin = idVisita.HourEnd,
                Garantia = _visita.Garantia,
                IdDireccion = _visita.IdDireccion,
                IdServicio = _visita.IdServicio,
                Pagado = true,
                PagoPendiente = false,
                TerminosCondiciones = _visita.TerminosCondiciones,
                FechaCancelacion = _visita.FechaCancelacion
            };

            _context.Visita.Update(_visita);
            _context.Visita.Add(_newVisita);
            _context.SaveChanges();

            List<RelTecnicoVisita> _tecnicos = new List<RelTecnicoVisita>();
            foreach (var item in _visita.RelTecnicoVisita)
            {
                _tecnicos.Add(new RelTecnicoVisita()
                {
                    IdTecnico = item.IdTecnico,
                    IdVista = _newVisita.Id,
                    TecnicoResponsable = item.TecnicoResponsable
                });
            }
            _context.RelTecnicoVisita.AddRange(_tecnicos);
            _context.SaveChanges();

            List<RelServicioProducto> _productos = new List<RelServicioProducto>();
            foreach (var item in _visita.RelServicioProducto)
            {
                var _buesqueda = _visita.RelServicioRefaccion.FirstOrDefault(c => c.IdProducto == item.IdProducto);
                if (_buesqueda != null && _buesqueda.Estatus == 1008)
                    _productos.Add(new RelServicioProducto()
                    {
                        IdCategoria = item.IdCategoria,
                        DescripcionCierre = item.DescripcionCierre,
                        Estatus = item.Estatus,
                        Garantia = item.Garantia,
                        IdProducto = item.IdProducto,
                        IdVista = _newVisita.Id,
                        NoSerie = item.NoSerie,
                        PrimeraVisita = false,
                        Reparacion = item.Reparacion
                    });
            }
            _context.RelServicioProducto.AddRange(_productos);
            _context.SaveChanges();

            transaction = Convert.ToInt32(_newVisita.Id);

            _context.Notificaciones.Add(new Notificaciones()
            {
                Creado = DateTime.Now,
                Creadopor = 0,
                Descripcion = "Hay una nueva visita para el servicio con el numero " + _newVisita.IdServicio + " fue agendada a través del app de clientes.",
                EstatusLeido = false,
                RolNotificado = 10008,
                Evento = "Seguimiento",
                Url = "editarservicio/" + _newVisita.IdServicio
            });
            _context.SaveChanges();

            return transaction;
        }
        public List<Visita> getAllVisitasCalendar(clsCalendar _calendar)
        {
            List<int> _categorias = _calendar.categorias.Select(h => h.idCategoria).ToList();
            var _consult = (from a in _context.Servicio
                            join b in _context.Visita on a.Id equals b.IdServicio
                            join tv in _context.RelTecnicoVisita on b.Id equals tv.IdVista
                            join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                            join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                            where
                                f.IdActividad == _calendar.tipoServicio
                                && _categorias.Contains(h.IdCategoriaProducto)
                                && b.FechaVisita >= _calendar.fechaInicio
                                && b.FechaVisita <= _calendar.fechaFin
                            select new Visita
                            {
                                Hora = getDateFormat(b.FechaVisita, b.Hora),
                                HoraFin = getDateFormat(b.FechaVisita, b.HoraFin),
                                FechaVisita = b.FechaVisita,
                                AsignacionRefacciones = _context.Clientes.FirstOrDefault(c => c.Email == "festivo@dia.mx").Id == a.IdCliente ? true : false
                            }).Distinct().ToList();
            var _consult2 = (from a in _context.ServicioApp
                             join b in _context.VisitaApp on a.Id equals b.IdServicio
                             join tv in _context.RelTecnicoVisitaApp on b.Id equals tv.IdVista
                             join f in _context.TecnicosActividad on tv.IdTecnico equals f.IdUser
                             join h in _context.TecnicosProducto on tv.IdTecnico equals h.IdUser
                             where
                                 f.IdActividad == _calendar.tipoServicio
                                && _categorias.Contains(h.IdCategoriaProducto)
                                && b.FechaVisita >= _calendar.fechaInicio
                                && b.FechaVisita <= _calendar.fechaFin
                             select new Visita
                             {
                                 Hora = getDateFormat(b.FechaVisita, b.Hora),
                                 HoraFin = getDateFormat(b.FechaVisita, b.HoraFin),
                                 FechaVisita = b.FechaVisita,
                                 AsignacionRefacciones = false
                             }).Distinct().ToList();

            _consult.Union(_consult2);
            return _consult;
        }
        public List<Tecnicos> getAllTecnicos(clsCalendar _calendar)
        {
            List<int> _categorias = _calendar.categorias.Select(h => h.idCategoria).ToList();
            var _consult = (from
                             d in _context.Users
                            join e in _context.Tecnicos on d.Id equals e.Id
                            join f in _context.TecnicosActividad on e.Id equals f.IdUser
                            join h in _context.TecnicosProducto on e.Id equals h.IdUser
                            where
                                f.IdActividad == _calendar.tipoServicio
                                && _categorias.Contains(h.IdCategoriaProducto)
                            select new Tecnicos
                            {
                                Id = e.Id,
                                VehiculoInfo = e.VehiculoInfo,
                                VehiculoPlacas = e.VehiculoPlacas,
                                Color = d.Name + ' ' + d.Paterno + ' ' + d.Materno,
                                Noalmacen = d.Avatar

                            }).Distinct().ToList();
            return _consult;
        }
        public CategoriaTipoProducto getFullCostoServicio(clsCalendar _calendar)
        {
            List<CategoriaTipoProducto> _list = getAllCostoServicio(_calendar);
            CategoriaTipoProducto _total = new CategoriaTipoProducto();
            foreach (CategoriaTipoProducto _product in _list)
            {
                _total.HorasTecnicos += _product.HorasTecnicos;
                _total.PrecioVisita += _product.PrecioVisita;
                _total.Viaticos = _product.Viaticos;
            }
            _list.OrderByDescending(c => c.NoTecnicos);
            if (_list.Count > 0)
                _total.NoTecnicos = _list[0].NoTecnicos;

            return _total;
        }
        public List<CategoriaTipoProducto> getAllCostoServicio(clsCalendar _calendar)
        {
            List<int> _categorias = _calendar.categorias.Select(h => h.idCategoria).ToList();
            List<RelCategoriaProductoTipoProducto> _costos = _context.RelCategoriaProductoTipoProducto
                                                                .Where(c =>
                                                                c.IdTipoServicio == _calendar.tipoServicio
                                                                && _categorias.Contains(c.IdCategoria)
                                                                ).OrderBy(c => c.Id).ToList();
            decimal _viatico = getViatico(_calendar.idDireccion);
            int suma = 0;

            List<CategoriaTipoProducto> _response = new List<CategoriaTipoProducto>();
            foreach (var _costo in _costos)
            {
                decimal _horas = 0M;
                decimal _precio = 0M;
                for (int i = 0; i < _calendar.categorias.FirstOrDefault(c => c.idCategoria == _costo.IdCategoria).cantidad; i++)
                {
                    _horas += Math.Ceiling(Convert.ToDecimal(_costo.HorasTecnicos));
                    if (_costo.IdTipoServicio == 1)
                        _precio = 0M;
                    else if (_costo.IdTipoServicio == 3)
                        if (suma > 0)
                            _precio += 490M;
                        else
                            _precio += Convert.ToDecimal(_costo.PrecioVisita);
                    else if (_costo.IdTipoServicio == 2)
                        _precio += Convert.ToDecimal(_costo.PrecioVisita);
                    if (_precio > 0)
                        suma++;
                }
                _response.Add(new CategoriaTipoProducto()
                {
                    Viaticos = _viatico,
                    HorasTecnicos = _horas.ToString(),
                    NoTecnicos = _costo.NoTecnicos,
                    PrecioHoraTecnico = _costo.PrecioHoraTecnico,
                    PrecioVisita = Convert.ToInt32(_precio),
                    Estatus = _costo.Estatus,
                    IdCategoria = _costo.IdCategoria,
                    IdTipoServicio = _costo.IdTipoServicio,
                    //iva = (_precio + _viatico) * .16M,
                    //subTotal = (_precio / 1.16M) + _viatico,
                    //Total = ((_precio + _viatico) * .16M) + (_precio / 1.16M) + _viatico
                    iva = (_precio - (_precio / 1.16M)) + (_viatico * 0.16M),
                    subTotal = (_precio / 1.16M),
                    Total = (_precio / 1.16M) + (_precio - (_precio / 1.16M)) + (_viatico * 0.16M)
                });
            }

            return _response;
        }

        public bool comprobarMerge(long idCliente)
        {
            var _encontrado = _context.RelUserUserApp.FirstOrDefault(c => c.IdClientApp == idCliente);
            if (_encontrado == null)
                return false;
            return true;
        }
        public long obtenerMerge(long idCliente)
        {
            var _encontrado = _context.RelUserUserApp.FirstOrDefault(c => c.IdClientApp == idCliente);
            if (_encontrado == null)
                return 0;
            return Convert.ToInt64(_encontrado.IdClient);
        }
        public bool setReagendar(long id, bool app)
        {
            if (app)
            {
                long _idcliente = _context.VisitaApp.Where(c => c.Id == id).Include(c => c.IdServicioNavigation).FirstOrDefault().IdServicioNavigation.IdCliente;
                Clientes clientes = _context.Clientes.FirstOrDefault(c => c.Id == _idcliente);
                var _visita = _context.VisitaApp.FirstOrDefault(c => c.Id == id);
                var dir = (from a in _context.DireccionesCliente
                           join b in _context.CatEstado on a.IdEstado equals b.Id
                           join c in _context.CatMunicipio on a.IdMunicipio equals c.Id
                           select new
                           {
                               a.CalleNumero,
                               NumExt = a.NumExt == null ? "" : a.NumInt,
                               NumInt = a.NumInt == null ? "" : a.NumInt,
                               a.Colonia,
                               a.Cp,
                               b.DescEstado,
                               c.DescMunicipio
                           }).ToList();

                _visita.Regendada = true;
                _context.Update(_visita);

                _context.Notificaciones.Add(new Notificaciones()
                {
                    Creado = DateTime.Now,
                    Creadopor = 0,
                    Descripcion = $"Hay un servicio creado desde la app que requiere reagendar la visita pero primero se necesita realizar el merge del usuario {clientes.Nombre} {clientes.Paterno} {clientes.Materno}, dirección {dir[0].CalleNumero} {dir[0].NumExt}, {dir[0].Colonia}, {dir[0].DescEstado}, {dir[0].DescMunicipio}",
                    EstatusLeido = false,
                    RolNotificado = 10008,
                    Evento = "Reagendar",
                    Url = "merge"
                });
                _context.SaveChanges();

                StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailReagendar.html"));
                string body = string.Empty;
                body = reader.ReadToEnd();
                body = body.Replace("{username}", $"{clientes.Nombre} {clientes.Paterno} {clientes.Materno}");
                EmailService emailService = new EmailService();
                emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Visita por reagendar", To = clientes.Email });
            }
            else
            {
                long _idcliente = _context.Visita.Where(c => c.Id == id).Include(c => c.IdServicioNavigation).FirstOrDefault().IdServicioNavigation.IdCliente;
                Clientes clientes = _context.Clientes.FirstOrDefault(c => c.Id == _idcliente);
                long _idServicio = _context.Visita.FirstOrDefault(c => c.Id == id).IdServicio;
                var _visita = _context.Visita.FirstOrDefault(c => c.Id == id);

                _visita.Regendada = true;
                _context.Update(_visita);

                _context.Notificaciones.Add(new Notificaciones()
                {
                    Creado = DateTime.Now,
                    Creadopor = 0,
                    Descripcion = "El servicio con el folio " + _idServicio + " requiere llamar para reagendar.",
                    EstatusLeido = false,
                    RolNotificado = 10008,
                    Evento = "Reagendar",
                    Url = "editarservicio/" + _idServicio
                });
                _context.SaveChanges();

                StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/EmailAvisoVisita.html"));
                string body = string.Empty;
                body = reader.ReadToEnd();
                body = body.Replace("{username}", $"{clientes.Nombre} {clientes.Paterno} {clientes.Materno}");
                EmailService emailService = new EmailService();
                emailService.SendEmail(new Email { Body = body, IsBodyHtml = true, Subject = "Visita por reagendar", To = clientes.Email });
            }
            return true;
        }
        #region Notificaciones App
        public bool saveToken(NotificationApp _notification)
        {
            var _token = _context.NotificationApp.FirstOrDefault(c => c.UserId == _notification.UserId && c.UidEquipo == _notification.UidEquipo);
            if (_token == null)
            {
                _token = _context.NotificationApp.FirstOrDefault(c => c.UserId == _notification.UserId);
                if (_token != null)
                    _notification.Notification = _token.Notification;
                _context.NotificationApp.Add(_notification);
                _context.SaveChanges();
            }
            else
            {
                _token.Token = _notification.Token;
                _context.NotificationApp.Update(_token);
                _context.SaveChanges();
            }
            return true;
        }
        public NotificationApp getNotification(NotificationApp _notification)
        {
            NotificationApp _consult = new NotificationApp();
            _consult = _context.NotificationApp.FirstOrDefault(c => c.UserId == _notification.UserId);
            return _consult;
        }
        public NotificationApp setNotification(NotificationApp _notification)
        {
            List<NotificationApp> _consult = new List<NotificationApp>();
            _consult = _context.NotificationApp.Where(c => c.UserId == _notification.UserId).ToList();
            for (int i = 0; i < _consult.Count; i++)
            {
                _consult[i].Notification = _notification.Notification;
            }
            _context.NotificationApp.UpdateRange(_consult);
            _context.SaveChanges();
            return _consult.First();
        }
        public bool sendNotificationClient(long _idVisita)
        {
            configLocal _config = getConfiguracionPettion();
            var _visitaGeneral = (from v in _context.Visita
                                  join s in _context.Servicio on v.IdServicio equals s.Id
                                  join c in _context.Clientes on s.IdCliente equals c.Id
                                  join ru in _context.RelUserUserApp on c.Id equals ru.IdClient
                                  join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                  join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                  where v.Id == _idVisita && notiUser.Notification == true
                                  select new
                                  {
                                      IdServicio = s.Id,
                                      v.Hora,
                                      Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                      v.Id,
                                      v.Estatus,
                                      v.Pagado,
                                      notiUser.Token,
                                  }).ToList();
            foreach (var _visita in _visitaGeneral)
            {
                var _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = $"Ahora puedes agendar una visita de seguimiento para tu servicio con folio {_visita.IdServicio} .",
                        title = "Agendar Visita de seguimiento",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _config.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visita.Id),
                        pagado = _visita.Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visita.Estatus)
                    },
                    data = new Data()
                    {
                        body = $"Ahora puedes agendar una visita de seguimiento para tu servicio con folio {_visita.IdServicio} .",
                        title = "Agendar Visita de seguimiento",
                        image = _config.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visita.Id),
                        pagado = _visita.Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visita.Estatus)
                    },
                    priority = "high",
                    to = _visita.Token
                };
                PetitionNotificationPush(_notiApp);
            }

            return true;
        }
        public bool sendNotification(int tipo)
        {

            switch (tipo)
            {
                case 1:
                    Caso1();
                    break;
                case 2:
                    Caso2();
                    break;
                case 3:
                    Caso3();
                    break;
                case 4:
                    Caso4();
                    break;
                case 5:
                    Caso5();
                    break;
                case 6:
                    Caso6();
                    break;
                case 7:
                    Caso7();
                    break;
                case 8:
                    Caso8();
                    break;
                case 9:
                    Caso8();
                    break;
                default:
                    break;
            }
            return false;
        }
        private void Caso1()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now.AddDays(1);
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + "00:00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.AddDays(1).ToString("dd/MM/yyyy") + " " + "00:00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var _visitaLocal = (from v in _context.Visita
                                join s in _context.Servicio on v.IdServicio equals s.Id
                                join c in _context.Clientes on s.IdCliente equals c.Id
                                join ru in _context.RelUserUserApp on c.Id equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where v.FechaVisita > _nowInicio && v.FechaVisita < _nowFin && notiUser.Notification == true
                                select new
                                {
                                    v.Hora,
                                    Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                    notiUser.Token,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Buenos días " + _visitaLocal[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocal[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion1.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Buenos días " + _visitaLocal[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocal[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion1.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
            var _visitaLocalApp = (from v in _context.VisitaApp
                                   join s in _context.ServicioApp on v.IdServicio equals s.Id
                                   join c in _context.Clientes on s.IdCliente equals c.Id
                                   join uca in _context.UserClientsApp on c.Id equals uca.IdClient
                                   join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                   where v.FechaVisita > _nowInicio && v.FechaVisita < _nowFin && notiUser.Notification == true
                                   select new
                                   {
                                       v.Hora,
                                       Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                       v.Id,
                                       v.Estatus,
                                       v.Pagado,
                                       notiUser.Token,
                                   }).ToList();
            for (int i = 0; i < _visitaLocalApp.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Buenos días " + _visitaLocalApp[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocalApp[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion1.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Buenos días " + _visitaLocalApp[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocalApp[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion1.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocalApp[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso2()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + "00:00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.AddDays(1).ToString("dd/MM/yyyy") + " " + "00:00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var _visitaLocal = (from v in _context.Visita
                                join s in _context.Servicio on v.IdServicio equals s.Id
                                join c in _context.Clientes on s.IdCliente equals c.Id
                                join ru in _context.RelUserUserApp on c.Id equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where v.FechaVisita > _nowInicio && v.FechaVisita < _nowFin && notiUser.Notification == true
                                select new
                                {
                                    v.Hora,
                                    Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                    notiUser.Token,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Buenos días " + _visitaLocal[i].Nombre + " le informamos que el día de hoy el técnico asistirá a su casa a las " + _visitaLocal[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion2.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Buenos días " + _visitaLocal[i].Nombre + " le informamos que el día de hoy el técnico asistirá a su casa a las " + _visitaLocal[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion2.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
            var _visitaLocalApp = (from v in _context.VisitaApp
                                   join s in _context.ServicioApp on v.IdServicio equals s.Id
                                   join c in _context.Clientes on s.IdCliente equals c.Id
                                   join uca in _context.UserClientsApp on c.Id equals uca.IdClient
                                   join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                   where v.FechaVisita > _nowInicio && v.FechaVisita < _nowFin && notiUser.Notification == true
                                   select new
                                   {
                                       v.Hora,
                                       Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                       v.Id,
                                       v.Estatus,
                                       v.Pagado,
                                       notiUser.Token,
                                   }).ToList();
            for (int i = 0; i < _visitaLocalApp.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Buenos días " + _visitaLocalApp[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocalApp[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion2.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Buenos días " + _visitaLocalApp[i].Nombre + " le informamos que el día de mañana el técnico asistirá a su casa a las " + _visitaLocalApp[i].Hora + ":00 horas. Favor de estar pendiente de la visita del técnico",
                        title = "Recordatorio de su visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion2.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocalApp[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso3()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.AddHours(1).ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var _visitaLocal = (from v in _context.Visita
                                join s in _context.Servicio on v.IdServicio equals s.Id
                                join c in _context.Clientes on s.IdCliente equals c.Id
                                join ru in _context.RelUserUserApp on c.Id equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where v.FechaFinVisita > _nowInicio && v.FechaFinVisita < _nowFin && notiUser.Notification == true && v.Estatus == 4
                                select new
                                {
                                    s.Actualizado,
                                    Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                    notiUser.Token,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Gracias por recibirnos en su casa el técnico ha conculido la visita. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Finalización de la Visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Gracias por recibirnos en su casa el técnico ha conculido la visita. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Finalización de la Visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
            var _visitaLocalApp = (from v in _context.VisitaApp
                                   join s in _context.ServicioApp on v.IdServicio equals s.Id
                                   join c in _context.Clientes on s.IdCliente equals c.Id
                                   join uca in _context.UserClientsApp on c.Id equals uca.IdClient
                                   join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                   where v.FechaFinVisita > _nowInicio && v.FechaFinVisita < _nowFin && notiUser.Notification == true && v.Estatus == 4
                                   select new
                                   {
                                       s.Actualizado,
                                       Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                       v.Id,
                                       v.Estatus,
                                       v.Pagado,
                                       notiUser.Token,
                                   }).ToList();
            for (int i = 0; i < _visitaLocalApp.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Gracias por recibirnos en su casa el técnico ha conculido la visita. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Finalización de la Visita",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Gracias por recibirnos en su casa el técnico ha conculido la visita. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Finalización de la Visita",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = true,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocalApp[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso4()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            _nowInicio = _nowInicio.AddHours(-48);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.AddHours(1).ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            _nowFin = _nowFin.AddHours(-48);
            var _visitaLocal = (from v in _context.Visita
                                join s in _context.RelServicioProducto on v.Id equals s.IdVista
                                join c in _context.ClienteProductos on s.IdProducto equals c.Id
                                join ep in _context.CatEstatusProducto on c.IdEsatusCompra equals ep.Id
                                join ru in _context.RelUserUserApp on c.IdCliente equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where ep.Id == 3 && (v.FechaCompletado > _nowInicio && v.FechaCompletado < _nowFin)
                                select new
                                {
                                    notiUser.Token,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Por favor ayúdenos a aceptar o rechazar la cotización enviada. De no ser aceptada el servicio no se llevará a cabo. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Pendiente Autorización",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion5.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Por favor ayúdenos a aceptar o rechazar la cotización enviada. De no ser aceptada el servicio no se llevará a cabo. Para mayor información favor de comunicarse a info@miele.com.mx o al 800MIELE00",
                        title = "Pendiente Autorización",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion5.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso5()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.AddHours(1).ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            int[] excluido = { 13, 15, 16 };

            var _visitaLocal = (from v in _context.Visita
                                join se in _context.Servicio on v.IdServicio equals se.Id
                                join s in _context.RelServicioProducto on v.Id equals s.IdVista
                                join c in _context.ClienteProductos on s.IdProducto equals c.Id
                                join ep in _context.CatEstatusProducto on c.IdEsatusCompra equals ep.Id
                                join ru in _context.RelUserUserApp on c.IdCliente equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where ep.Id == 1008 && (c.Actualizado > _nowInicio && c.Actualizado < _nowFin) && !(excluido.Contains(Convert.ToInt16(se.IdEstatusServicio)))
                                select new
                                {
                                    notiUser.Token,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Le inforamamos que sus refacciones ya estan disponibles, solicitamos su apoyo para seleccionar yuna nueva fecha de agendamiento para su visita",
                        title = "Refacciones listas para reparacion",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion5.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Le inforamamos que sus refacciones ya estan disponibles, solicitamos su apoyo para seleccionar yuna nueva fecha de agendamiento para su visita",
                        title = "Refacciones listas para reparacion",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion5.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso6()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.AddHours(1).ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var _visitaLocal = (from s in _context.Servicio
                                join v in _context.Visita on s.Id equals v.IdServicio
                                join c in _context.Clientes on s.IdCliente equals c.Id
                                join ru in _context.RelUserUserApp on c.Id equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where s.Actualizado > _nowInicio && s.Actualizado < _nowFin && notiUser.Notification == true && s.IdEstatusServicio == 15
                                select new
                                {
                                    s.Actualizado,
                                    Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                    v.Id,
                                    v.Estatus,
                                    v.Pagado,
                                    notiUser.Token,
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Gracias por recibirnos en su casa el equipo ha quedado reparado. Para mayor información favor de comunicarse a info@miele.com.mx o al 800 MIELE00",
                        title = "Servicio concluido",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Gracias por recibirnos en su casa el equipo ha quedado reparado. Para mayor información favor de comunicarse a info@miele.com.mx o al 800 MIELE00",
                        title = "Servicio concluido",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocal[i].Id),
                        pagado = _visitaLocal[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocal[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
            var _visitaLocalApp = (from s in _context.ServicioApp
                                   join v in _context.VisitaApp on s.Id equals v.IdServicio
                                   join c in _context.Clientes on s.IdCliente equals c.Id
                                   join uca in _context.UserClientsApp on c.Id equals uca.IdClient
                                   join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                   where s.Actualizado > _nowInicio && s.Actualizado < _nowFin && notiUser.Notification == true && s.IdEstatusServicio == 15
                                   select new
                                   {
                                       s.Actualizado,
                                       Nombre = c.Nombre + " " + c.Paterno + " " + c.Materno,
                                       v.Id,
                                       v.Estatus,
                                       v.Pagado,
                                       notiUser.Token,
                                   }).ToList();
            for (int i = 0; i < _visitaLocalApp.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Gracias por recibirnos en su casa el equipo ha quedado reparado. Para mayor información favor de comunicarse a info@miele.com.mx o al 800 MIELE00",
                        title = "Servicio concluido",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    data = new Data()
                    {
                        body = "Gracias por recibirnos en su casa el equipo ha quedado reparado. Para mayor información favor de comunicarse a info@miele.com.mx o al 800 MIELE00",
                        title = "Servicio concluido",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion3y6.png",
                        app = false,
                        idVisita = Convert.ToInt32(_visitaLocalApp[i].Id),
                        pagado = _visitaLocalApp[i].Pagado,
                        tipo = 1,
                        estatus = Convert.ToInt32(_visitaLocalApp[i].Estatus)
                    },
                    priority = "high",
                    to = _visitaLocalApp[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso7()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            _nowInicio = _nowInicio.AddMonths(-6).AddDays(-1);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            _nowFin = _nowFin.AddMonths(-6);
            var _visitaLocal = (from se in _context.Servicio
                                join v in _context.Visita on se.Id equals v.IdServicio
                                join ru in _context.RelUserUserApp on se.IdCliente equals ru.IdClient
                                join uca in _context.UserClientsApp on ru.IdClientApp equals uca.IdClient
                                join notiUser in _context.NotificationApp on uca.IdUser equals notiUser.UserId
                                where se.IdTipoServicio == 1 && se.IdEstatusServicio == 15 && (se.FechaServicio > _nowInicio && se.FechaServicio < _nowFin)
                                select new
                                {
                                    notiUser.Token
                                }).ToList();
            for (int i = 0; i < _visitaLocal.Count; i++)
            {
                _notiApp = new NotificacionesApp()
                {
                    notification = new Notification()
                    {
                        body = "Le recordamos que para mayor optimización debe realizar los mantenimientos de sus equipos Miele, cada 6 a 12 meses. No olvide agendarlo através del APP o comunicandose a info@miele.com.mx o al 800MIELE00 ",
                        title = "Recomendación mantenimiento",
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        sound = "default",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion7.png",
                        tipo = 3
                    },
                    data = new Data()
                    {
                        body = "Le recordamos que para mayor optimización debe realizar los mantenimientos de sus equipos Miele, cada 6 a 12 meses. No olvide agendarlo através del APP o comunicandose a info@miele.com.mx o al 800MIELE00 ",
                        title = "Recomendación mantenimiento",
                        image = _configuration.ConfigNotificacion.urlImagen + "Notificacion7.png"
                    },
                    priority = "high",
                    to = _visitaLocal[i].Token
                };
                PetitionNotificationPush(_notiApp);
            }
        }
        private void Caso8()
        {
            configLocal _configuration = getConfiguracionPettion();
            NotificacionesApp _notiApp = new NotificacionesApp();
            DateTime _now = DateTime.Now;
            DateTime _nowInicio = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _nowFin = DateTime.ParseExact((_now.ToString("dd/MM/yyyy") + " " + _now.AddHours(1).ToString("HH") + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var _visitaLocal = _context.NotificacionesComerciales.Where(c => DateTime.ParseExact((Convert.ToDateTime(c.Fecha).ToString("dd/MM/yyyy") + " " + c.Hora + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) >= _nowInicio && DateTime.ParseExact((Convert.ToDateTime(c.Fecha).ToString("dd/MM/yyyy") + " " + c.Hora + ":00:00"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) < _nowFin).ToList();
            var _visitacliente = _context.NotificationApp.Where(c => c.Notification == true).ToList();

            for (int i = 0; i < _visitacliente.Count; i++)
            {
                for (int j = 0; j < _visitaLocal.Count; j++)
                {
                    _notiApp = new NotificacionesApp()
                    {
                        notification = new Notification()
                        {
                            body = _visitaLocal[j].Cuerpo,
                            title = _visitaLocal[j].Titulo,
                            click_action = "FCM_PLUGIN_ACTIVITY",
                            sound = "default",
                            image = _visitaLocal[j].UrlImagen,
                            url = _visitaLocal[j].Url,
                            tipo = 2
                        },
                        data = new Data()
                        {
                            body = _visitaLocal[j].Cuerpo,
                            title = _visitaLocal[j].Titulo,
                            image = _visitaLocal[j].UrlImagen,
                            url = _visitaLocal[j].Url,
                            tipo = 2
                        },
                        priority = "high",
                        to = _visitacliente[i].Token
                    };
                }

                PetitionNotificationPush(_notiApp);
            }
        }

        private void PetitionNotificationPush(NotificacionesApp _notiApp)
        {
            configLocal _configuration = getConfiguracionPettion();
            PetitionHTTP _petition = new PetitionHTTP(_configuration.ConfigNotificacion.url);
            _petition.loadPetition(_configuration.ConfigNotificacion.controller, eMethodType.POST);
            for (int i = 0; i < _configuration.ConfigNotificacion.headers.Count; i++)
            {
                _petition.addHeader(_configuration.ConfigNotificacion.headers[i].key, _configuration.ConfigNotificacion.headers[i].Value);
            }
            _petition.addBody(JsonConverter.Object_Json(_notiApp));
            var response = _petition.makePetition();
        }
        private configLocal getConfiguracionPettion()
        {
            StreamReader reader = new StreamReader(Path.GetFullPath("TemplateMail/notification.json"));
            string body = string.Empty;
            body = reader.ReadToEnd();
            configLocal _configuration = new configLocal();
            _configuration = JsonConverter.Json_Object<configLocal>(body);
            return _configuration;
        }
        #endregion

        private clsServicioDTO getAllServiceByService(long _idService, bool _app)
        {
            clsServicioDTO _servicios = new clsServicioDTO();
            if (!_app)
            {
                clsServicioDTO _serviciosWEB = new clsServicioDTO();
                _serviciosWEB = (from servicio in _context.Servicio
                                 join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                                 join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                                 where servicio.Id == _idService
                                 select new clsServicioDTO
                                 {
                                     IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                     DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                     IdServicio = Convert.ToInt32(servicio.Id),
                                     IdTipoServicio = servicio.IdTipoServicio,
                                     DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                     DescripcionServicio = servicio.DescripcionActividades,
                                     app = false,
                                     EstatusEncuesta = Convert.ToByte(servicio.Encuesta),
                                     Visitas = (from visita in _context.Visita
                                                join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id
                                                where visita.IdServicio == servicio.Id
                                                select new clsVisitaDTO
                                                {
                                                    IdVisita = Convert.ToInt32(visita.Id),
                                                    IdEstatusVisita = Convert.ToInt32(visita.Estatus),
                                                    DescripcionEstatus = estatusVisita.DescEstatusVisita.ToUpper(),
                                                    Productos = (from relServicioProducto in _context.RelServicioProducto
                                                                 where relServicioProducto.IdVista == visita.Id
                                                                 select new clsProductoDTO
                                                                 {
                                                                     Garantia = relServicioProducto.Garantia,
                                                                     Estatusproducto = (from relServicioRefaccion in _context.RelServicioRefaccion
                                                                                        where relServicioRefaccion.IdVista == visita.Id && relServicioRefaccion.IdProducto == relServicioProducto.IdProducto
                                                                                        select relServicioRefaccion.Estatus).FirstOrDefault()
                                                                 }).ToList()
                                                }).ToList()
                                 }).FirstOrDefault();
                return new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosWEB.IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosWEB.DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosWEB.IdServicio),
                    IdTipoServicio = _serviciosWEB.IdTipoServicio,
                    DescripcionTipoServicio = _serviciosWEB.DescripcionTipoServicio,
                    DescripcionServicio = _serviciosWEB.DescripcionServicio,
                    app = _serviciosWEB.app,
                    EstatusEncuesta = Convert.ToByte(_serviciosWEB.EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosWEB.Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosWEB.IdEstatusServicio)),
                    Visitas = _serviciosWEB.Visitas
                };
            }
            else
            {
                var _serviciosApp = (from servicio in _context.ServicioApp
                                     join estatusServicio in _context.CatEstatusServicio on servicio.IdEstatusServicio equals estatusServicio.Id
                                     join tipoServicio in _context.CatTipoServicio on servicio.IdTipoServicio equals tipoServicio.Id
                                     where servicio.Id == _idService
                                     select new clsServicioDTO
                                     {
                                         IdEstatusServicio = Convert.ToInt32(servicio.IdEstatusServicio),
                                         DescripcionEstatusServicio = estatusServicio.DescEstatusServicio.ToUpper(),
                                         IdServicio = Convert.ToInt32(servicio.Id),
                                         IdTipoServicio = servicio.IdTipoServicio,
                                         DescripcionTipoServicio = tipoServicio.DescTipoServicio.ToUpper(),
                                         DescripcionServicio = servicio.DescripcionActividades,
                                         app = true,
                                         EstatusEncuesta = 0,
                                         Visitas = (from visita in _context.VisitaApp
                                                    join estatusVisita in _context.CatEstatusVisita on visita.Estatus equals estatusVisita.Id
                                                    where visita.IdServicio == servicio.Id
                                                    select new clsVisitaDTO
                                                    {
                                                        IdVisita = Convert.ToInt32(visita.Id),
                                                        IdEstatusVisita = Convert.ToInt32(visita.Estatus),
                                                        DescripcionEstatus = estatusVisita.DescEstatusVisita.ToUpper(),
                                                        Productos = (from relServicioCategoriaApp in _context.RelServicioCategoriaApp
                                                                     where relServicioCategoriaApp.IdVisita == visita.Id
                                                                     select new clsProductoDTO
                                                                     {
                                                                         Garantia = false,
                                                                         Estatusproducto = 0
                                                                     }).ToList()
                                                    }).ToList()
                                     }).FirstOrDefault();
                return new clsServicioDTO()
                {
                    IdEstatusServicio = Convert.ToInt16(_serviciosApp.IdEstatusServicio),
                    DescripcionEstatusServicio = _serviciosApp.DescripcionEstatusServicio,
                    IdServicio = Convert.ToInt16(_serviciosApp.IdServicio),
                    IdTipoServicio = _serviciosApp.IdTipoServicio,
                    DescripcionTipoServicio = _serviciosApp.DescripcionTipoServicio,
                    DescripcionServicio = _serviciosApp.DescripcionServicio,
                    app = _serviciosApp.app,
                    EstatusEncuesta = Convert.ToByte(_serviciosApp.EstatusEncuesta),
                    DescripcionEstatusProductos = getMensajeSemaforo(_serviciosApp.Visitas.Last().Productos),
                    Semaforo = getEstatusSemaforo(Convert.ToInt16(_serviciosApp.IdEstatusServicio)),
                    Visitas = _serviciosApp.Visitas
                };

            }
        }
        private List<CotizacionDTO> getAllCotizationByVisita(long _idVisita, bool _app)
        {
            List<CotizacionDTO> _consult = new List<CotizacionDTO>();

            if (_app)
            {
                var _visita = _context.VisitaApp
                 .Select(c => c)
                 .Include(c => c.IdServicioNavigation)
                 .Include(c => c.RelServicioCategoriaApp)
                 .FirstOrDefault(c => c.Id == _idVisita);

                int[] _categoria = new int[_visita.RelServicioCategoriaApp.Count];
                int i = 0;
                foreach (var item in _visita.RelServicioCategoriaApp)
                {
                    _categoria[i] = item.IdSubLinea;
                    i++;
                }
                var _hora = HoraServicio(_visita.IdServicioNavigation.IdTipoServicio, _categoria, _visita.IdDireccion);
                for (int k = 0; k < _hora.Count; k++)
                {
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Costo Visita",
                        precio_sin_iva = Convert.ToDecimal(_hora[k].PrecioVisita),
                        //precio_sin_iva = Convert.ToDecimal(_hora[i].PrecioVisita + (_hora[i].PrecioHoraTecnico * Convert.ToInt32(_hora[i].HorasTecnicos))),
                    });
                }
                if (_hora.Count > 0)
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Viaticos",
                        precio_sin_iva = Convert.ToDecimal(_hora[0].Viaticos),
                    });
            }
            else
            {
                var _visita = _context.Visita
                                .Select(c => c)
                                .Include(c => c.RelServicioProducto)
                                .Include(c => c.IdServicioNavigation)
                                .FirstOrDefault(c => c.Id == _idVisita);
                int[] _categoria = new int[_visita.RelServicioProducto.Count];
                int i = 0;
                foreach (var item in _visita.RelServicioProducto)
                {
                    _categoria[i] = item.IdCategoria;
                    i++;
                }
                var _hora = HoraServicio(_visita.IdServicioNavigation.IdTipoServicio, _categoria, _visita.IdDireccion);
                for (int k = 0; k < _hora.Count; k++)
                {
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Costo Visita",
                        precio_sin_iva = Convert.ToDecimal(_hora[k].PrecioVisita),
                        //precio_sin_iva = Convert.ToDecimal(_hora[i].PrecioVisita + (_hora[i].PrecioHoraTecnico * Convert.ToInt32(_hora[i].HorasTecnicos))),
                    });
                }
                if (_hora.Count > 0)
                    _consult.Add(new CotizacionDTO
                    {
                        cantidad = 1,
                        garantia = false,
                        refaccion = "Viaticos",
                        precio_sin_iva = Convert.ToDecimal(_hora[0].Viaticos),
                    });
            }
            return _consult;
        }
        private decimal getViatico(long idDireccion)
        {
            var viatico = _context.CatCoberturaCodigoPostal.FirstOrDefault(c => c.Codigo == (_context.CatDireccion.FirstOrDefault(h => h.Id == idDireccion).Cp));
            if (viatico != null)
                return viatico.Costo.Value;
            else
                return 0;
        }

        #region utilities       
        private string getMensajeSemaforo(List<clsProductoDTO> _productos)
        {
            string _mensaje = string.Empty;
            int _tipo_mensaje = 0;
            List<clsProductoDTO> _validacion = _productos.Where(c => c.Estatusproducto == 0).ToList();
            if (_validacion.Count == _productos.Count)
            {
                _mensaje = "En espera de Visita";
            }

            for (int i = 0; i < _productos.Count(); i++) {

                if (_productos[i].Estatusproducto == 3)
                {
                    _tipo_mensaje = 3;
                    break;
                }
                else
                {
                    if (_productos[i].Estatusproducto == 1 || _productos[i].Estatusproducto == 2)
                    {
                        _tipo_mensaje = 2;
                        break;
                    }
                    else
                    {
                        if (_productos[i].Estatusproducto == 1008)
                        {
                            _tipo_mensaje = 1008;
                            break;
                        }
                        else
                        {
                            if (_productos[i].Estatusproducto == 6)
                            {
                                _tipo_mensaje = 6;
                                break;
                            }
                            else
                            {
                                if (_productos[i].Estatusproducto == 7)
                                {
                                    _tipo_mensaje = 7;
                                    break;
                                }
                                else
                                {
                                    if (_productos[i].Estatusproducto == 4 || _productos[i].Estatusproducto == 5)
                                    {
                                        _tipo_mensaje = 4;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            switch (_tipo_mensaje)
            {
                case 3:
                    _mensaje = "Estamos esperando tu pago para continuar";
                    break;
                case 2:
                    _mensaje = "Estamos solicitando tus refacciones";
                    break;
                case 1008:
                    _mensaje = "Procede a agendar tu nueva visita";
                    break;
                case 6:
                    _mensaje = "Pendiente por instalar";
                    break;
                case 7:
                    _mensaje = "Listo para instalar";
                    break;
                case 4:
                    _mensaje = "Visita completada";
                    break;
                default:
                    // code block
                    break;
            }

            return _mensaje;
        }

        private int getEstatusSemaforo(int _estatusServicio)
        {
            if (_estatusServicio == 15)
                return 3;
            if (_estatusServicio == 13)
                return 1;
            else
                return 2;
        }
        private int getEstatusSemaforoVisita(int _estatusVisita)
        {
            if (_estatusVisita == 4 || _estatusVisita == 1005)
                return 3;
            if (_estatusVisita == 5 || _estatusVisita == 1002)
                return 1;
            else
                return 2;
        }
        private string getValString(string _text = null)
        {
            if (_text == null)
                return "";
            else
                return _text;
        }
        private string getDateFormat(DateTime _date, string _hour)
        {
            decimal _hora = Convert.ToDecimal(_hour);
            _hora = Math.Ceiling(_hora);
            string _horaFotmato = _hora.ToString("00") + ":00:00";
            return _date.ToString("yyyy-MM-dd") + "T" + _horaFotmato;
        }
        #endregion
        #endregion
    }

}
