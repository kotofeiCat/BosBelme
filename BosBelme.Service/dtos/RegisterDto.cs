using BosBelme.Data.Entities;

namespace BosBelme.Service.Dto
{
    public class RegisterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public static RegisterDto FromUser(Users user)
        {
            return new RegisterDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}