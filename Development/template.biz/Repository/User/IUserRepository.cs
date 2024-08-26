using template.biz.Entities;

namespace template.biz.Repository
{
    public interface IUserRepository : IGenericRepository<Users>
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
        Users change_passwor(Users users);
        void change_DateUpdate(string email);
        bool confirmAccountStatusUpdate(int id);
        long createClient(Clientes clients, long id_user);
        bool updatePassword(Users user);
        long findClient(long id_user);

        void notificarRegistro(Users user);
        
    }
}
