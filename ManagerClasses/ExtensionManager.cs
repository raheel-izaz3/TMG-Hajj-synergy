namespace TMG.ManagerClasses
{
    public static class ExtensionManager
    {
        public static string[] AllowedExtensionsForImages = new string[]
        {
            ".jpeg", ".jpg", ".png", ".jfif", ".pdf"
        };


        public static string[] AllowedExtensionsForDocuments = new string[] 
        {
            ".doc", ".docx", ".pdf"
        };

        public static string GetExtensionWithoutPeriod(IFormFile file)
        {
            return "";
        }

        public static string GetExtension(IFormFile file)
        {
            return Path.GetExtension(file.FileName);
        }

        /// <summary>
        /// Validates all the extensions against the AllowedExtensionsForImages
        /// </summary>
        /// <param name="extensions">All Extensions of file wanted for upload</param>
        /// <returns>Boolean for if all the provided extensions are valid or not</returns>
        public static bool ValidateFileExtensions(params string[] extensions)
        {
            bool valid = true;
            for (int i = 0; i < extensions.Length; i++)
            {
                if (!AllowedExtensionsForImages.Contains(extensions[i]))
                {
                    valid = false;
                    break;
                }
            }

            return valid;
        }

    }
}
