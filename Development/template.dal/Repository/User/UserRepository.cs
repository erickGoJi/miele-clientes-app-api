using template.biz.Entities;
using template.biz.Repository;
using template.dal.db_context;
using CryptoHelper;
using System;
using System.Linq;

namespace template.dal.Repository
{
    public class UserRepository : GenericRepository<Users>, IUserRepository
    {
        public UserRepository(Db_TemplateContext context) : base(context) { }

        public string HashPassword(string password)
        {
            return Crypto.HashPassword(password);
        }

        public bool VerifyPassword(string hash, string password)
        {
            return Crypto.VerifyHashedPassword(hash, password);
        }

        public override Users Add(Users user)
        {
            user.Password = HashPassword(user.Password);
            return base.Add(user);
        }

        public override Users Update(Users user, object id)
        {
            user.Actualizado = DateTime.Now;
            user.Actualizadopor = user.Id;
            return base.Update(user, id);
        }

        public Users change_passwor(Users users)
        {
            Users response = new Users();
            try
            {
                var user = _context.Users.SingleOrDefault(c => c.Email == users.Email);
                var password = $"{Guid.NewGuid().ToString().Substring(0, 6)}";
                user.Password = HashPassword(password);
                response = user;
                _context.Update(user);
                _context.SaveChanges();
                response.Password = password;
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public bool updatePassword(Users user)
        {
            bool estatus = false;
            var _user = _context.Users.FirstOrDefault(c => c.Id == user.Id);
            _user.Password = HashPassword(user.Password);
            _context.Users.Update(_user);
            _context.SaveChanges();
            return estatus;
        }

        public void change_DateUpdate(string email)
        {

            try
            {
                var user = _context.Users.SingleOrDefault(c => c.Email == email);

                user.Actualizado = DateTime.Now;

                _context.Update(user);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {

            }

        }

        public bool confirmAccountStatusUpdate(int id)
        {

            try
            {
                var user = _context.Users.SingleOrDefault(c => c.Id == id);

                user.Estatus = true;

                _context.Update(user);
                _context.SaveChanges();

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public long createClient(Clientes clients, long id_user)
        {
            try
            {

                _context.Clientes.Add(clients);


                _context.SaveChanges();

                _context.UserClientsApp.Add(new UserClientsApp { IdClient = clients.Id, IdUser = id_user });
                _context.SaveChanges();

                return clients.Id;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public long findClient(long id_user)
        {
            return (from cli in _context.UserClientsApp where cli.IdUser == id_user select cli.IdClient).FirstOrDefault();
        }

        public void notificarRegistro(Users user)
        {
            _context.Notificaciones.Add(new Notificaciones()
            {
                Creado = DateTime.Now,
                Creadopor = 0,
                Descripcion = $"Un usuario se a registrado con el nombre {user.Name} {user.Paterno} {user.Materno}",
                EstatusLeido = false,
                RolNotificado = 10008,
                Evento = "Usuario nuevo registrado.",
                Url = "merge"
            });
            _context.SaveChanges();
        }
    }
}
