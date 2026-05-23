namespace WebApplication2.Extensions
{
    public static class FileManager
    {
        public static async Task<string> SaveFileAsync(this IFormFile file, string rootpath)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(rootpath, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream); // async versiya
            }
            return fileName;
        }
        public static bool IsImage(this IFormFile file)
        {

            return file.ContentType.Contains("image/");

        }
        public static bool IsValidSize(this IFormFile file, int maxInMb)
        {
            return file.Length <= maxInMb * 1024 * 1024;
        }
    }

    
}
