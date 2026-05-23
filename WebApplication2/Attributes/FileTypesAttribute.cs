using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Attributes
{
    public class FileTypesAttribute: ValidationAttribute
    {
        public string[] Types { get; set; }
        public FileTypesAttribute(params string[] types)
        {
            Types = types;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            List<IFormFile> f = new List<IFormFile>();
            if (value is IFormFile file)
            {
                f.Add(file);
            }
            else if (value is List<IFormFile> files)
            {
                f = files;
            }
            foreach (var item in f)
            {
                if (!Types.Contains(item.ContentType))
                {
                    return new ValidationResult($"Only the following file types are allowed: {string.Join(", ", Types)}.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
