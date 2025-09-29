namespace WebApi.MinimalApi.Models
{
    public class CreateUserRequest
    {
        public string? Login { get; set; }
        public string? FirstName { get; set; }  // опционально, по умолчанию "John"
        public string? LastName { get; set; }   // опционально, по умолчанию "Doe"
    }

    public class UpdateUserRequest
    {
        public string? Login { get; set; }      // обязателен для PUT / PATCH (после применения patch)
        public string? FirstName { get; set; }  // обязательны для PUT / PATCH (после применения patch)
        public string? LastName { get; set; }   // обязательны для PUT / PATCH (после применения patch)
    }
}