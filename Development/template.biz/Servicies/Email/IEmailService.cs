using template.biz.Entities;

namespace template.biz.Servicies
{
    public interface IEmailService
    {
        void SendEmail(Email email);
    }
}
