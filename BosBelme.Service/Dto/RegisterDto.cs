using BosBelme.Data.Entities;

namespace BosBelme.Service.Dto
{
    // DTO для передачи данных о зарегистрированном пользователе. Содержит идентификатор пользователя, имя и email.
    public record RegisterDto(int Id, string? Name, string? Email);
}