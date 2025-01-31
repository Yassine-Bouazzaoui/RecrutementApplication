namespace RecrutementApplication.Utility
{
    public static class FileUploadExtensions
    {
        public static async Task<string> SaveProfilePictureAsync(this IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                return null;

            // Générer un nom de fichier unique
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadPath = Path.Combine(webRootPath, "ProfilPics");

            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
