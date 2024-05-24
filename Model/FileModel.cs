using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClosirisDesktop.Model {
    public class FileModel {

        public int Id { get; set; }
        public string FolderName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public string FileBase64 { get; set; }
        public double SizeInKB { get; private set; }
        public DateTime CreationDate { get; set; }
        public string FileImage { get; set; }
        public string Size { get; set; }
        public double FileSize {
            get {
                if (decimal.TryParse(Size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeInBytes)) {
                    return Math.Round((double)sizeInBytes / 1024, 2);
                }
                return 0;
            }
        }

        public string FormatCreationDate {
            get {
                return CreationDate.ToString("dd/MM/yyyy");
            }
        }
    }
}