using Microsoft.AspNetCore.Mvc;
using mieleApp.api.Models.Service;
using mieleApp.biz.EntitiesDTO.Service;
using System;
using System.Collections.Generic;
using System.Text;
using template.biz.Entities;
using template.biz.Repository;

namespace mieleApp.biz.Repository.Service
{
    public interface IServiceRepository : IGenericRepository<ServicioApp>
    {
        List<Visita> ServiceOrder(DateTime _inicio, DateTime _fin, int Actividad, int[] Categorias);
        IActionResult GetDisponibilidad(DateTime fecha_visita, int Actividad, int[] Categorias, int tipo_servicio, int horas_visita, int no_tecnicos);
        List<Tecnicos> GetTecnicos(DateTime _inicio, DateTime _fin, int Actividad, int[] Categorias);
        List<RelCategoriaProductoTipoProducto> HoraServicio(int Actividad, int[] Categorias, long idDireccion);
        List<RelCategoriaProductoTipoProducto> HoraVisita(long idVisita);
        List<responseServices> GetAllServicesShow(long idCliente, bool pendientes);
        //bool ServiceOrderSaveVisita(long idCliente, long idServicio, string hora, string horaFin, DateTime fecha, int idDireccion, long[] idTecnicos);
        bool ServiceOrderSaveVisita(ServiceAddDTO _service);
        servicioDetalle getDetalleServicio(bool app, long idVisita);
        void updateEncuesta(long _idVisita, int estatusEncuesta);
        servicioDetalle getCancelarServicio(bool app, long idVisita, string exp);

        //bool ServiceOrderSaveVisita(long idCliente, long idServicio, string hora,string horaFin,DateTime fecha,int idDireccion,long[] idTecnicos);
        List<CotizacionDTO> get_cotizacion(int id_visita);
        int saveCotization(ServiceAddDTO _visita);
        bool updateCotization(ServiceAddDTO _service);
        bool pagarCotizacion(long _idVisita);
        bool comprobarMerge(long idCliente);
        long obtenerMerge(long idCliente);

        Servicio addService(Servicio _service, ServiceAddDTO _serviceAdd);
        IActionResult get_mis_productos(int id);

        Clientes getProfile(Clientes _cliente);
        Clientes setProfile(Clientes _cliente);
        bool setReagendar(long id, bool app);
        bool updateVisita(long id, bool app);
        Visita getDetalleAllServicio(long idVisita);
        void pruebaMail(string mail);
        responseServices getEstatusServicioVisita(long _idVista, bool app);

        #region Notificaciones App
        bool saveToken(NotificationApp _notification);
        NotificationApp getNotification(NotificationApp _notification);
        NotificationApp setNotification(NotificationApp _notification);
        bool sendNotificationClient(long _idVisita);
        bool sendNotification(int tipo);
        #endregion

        #region Restructura
        List<CategoriaTipoProducto> getAllCostoServicio(clsCalendar _calendar);
        List<clsServicioDTO> getAllServiceByClient(long _idClient, bool _general);
        clsVisitaServicioDTO getAllDetalleVisita(long _idVisita, bool _app);
        #endregion
    }
}
