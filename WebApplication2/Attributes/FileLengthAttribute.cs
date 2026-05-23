using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Attributes
{
    public class FileLengthAttribute:ValidationAttribute
    {
        public int Length { get; set; }
        public FileLengthAttribute(int length)
        {
            Length = length;
        }
         protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            List<IFormFile> f=new List<IFormFile>();
            if (value is IFormFile file)
            {
                f.Add(file);
            }
            else if (value is List<IFormFile> files)
            {
                f=files;
            }
            foreach (var item in f)
            {
                if (item.Length > Length * 1024 * 1024)
                {
                    return new ValidationResult($"File size must be less than {Length}MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
