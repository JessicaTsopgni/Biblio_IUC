using Ghostscript.NET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace BiblioIUC.Logics.Tools
{

    public static class OssFile
    {
        public static string GetBase64(string filePafth)
        {
            if (File.Exists(filePafth))
            {
                byte[] bytes = File.ReadAllBytes(filePafth);
                string file = Convert.ToBase64String(bytes);
                return file;
            }
            return null;
        }
        public static void ConvertPdfToImage(string libPath, string inputPDFFilePath, int pageNumber, string outputFilePath, int width, int height)
        {
            GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(libPath);

            GhostscriptPngDevice dev = new GhostscriptPngDevice(GhostscriptPngDeviceType.Png256);
            dev.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
            dev.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
            dev.ResolutionXY = new GhostscriptImageDeviceResolution(290, 290);
            dev.InputFiles.Add(inputPDFFilePath);
            dev.Pdf.FirstPage = pageNumber;
            dev.Pdf.LastPage = pageNumber;
            dev.CustomSwitches.Add("-dDOINTERPOLATE");
            dev.OutputPath = outputFilePath;
            dev.Process(gvi, true, null);
        }

        public static void DeleteFile(string filename, string mediaBasePath)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                string fileToDelete = Path.Combine(mediaBasePath, filename);
                if (File.Exists(fileToDelete))
                    File.Delete(fileToDelete);
            }
        }

        public static void DeleteFileInFolder(string likeFileName, string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    if(file.Contains(likeFileName))
                        File.Delete(file);
                }
            }
        }

        public static bool HasImage(IFormFile image)
        {
            return image != null && image.Length > 0 && image.ContentType.Contains("image");
        }

        public static bool HasFile(IFormFile file)
        {
            return file != null && file.Length > 0;
        }

        public static string GetMediaFolderBaseUrl(HttpRequest request, string relativeFolderPath)
        {
            var url = request.GetDisplayUrl();
            if (request.QueryString.HasValue)
                url = url.Replace(request.QueryString.ToString(), string.Empty);
            var baseUri = new Uri(url);
            var resourceBasePath = relativeFolderPath.Replace("~/", $@"{baseUri.Scheme}://{baseUri.Authority}/");
            return resourceBasePath;
        }
        
        public static string SaveImage(IFormFile file, int width, int height, 
            int minWeight, int maxWeight, string filePrefixName, 
            string baseFolderPath)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string newFileName = GetNewFileName(file, filePrefixName);
                var fileContent = GetBytes(reader.BaseStream, width, height, minWeight, maxWeight);
                File.WriteAllBytes(Path.Combine(baseFolderPath, newFileName), fileContent);
                return newFileName;
            }
        }

        public static string GetNewFileName(string fileName, string filePrefixName)
        {
            string extension = GetExtension(fileName);
            string newFileName = filePrefixName + Guid.NewGuid().ToString().ToLower() + extension;
            return newFileName;
        }

        public static string GetNewFileName(IFormFile file, string filePrefixName)
        {
            return GetNewFileName(file.FileName, filePrefixName);   
        }

        public static async Task<string> SaveFile(IFormFile file,  string filePrefixName,
            string relativeFolderPath)
        {
            string newFileName = GetNewFileName(file, filePrefixName);
            var filePath = Path.Combine(relativeFolderPath, newFileName);
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            return newFileName;
        }       

        public static byte[] GetBytes(string fileName)
        {
            try
            {
                if (fileName != null && System.IO.File.Exists(fileName))
                {
                    return System.IO.File.ReadAllBytes(fileName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static byte[] GetBytes(string fileName, int width, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (fileName != null && System.IO.File.Exists(fileName))
                {
                    return GetBytes(GetImage(fileName, width, height, minByteWeight, maxByteWeight));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static byte[] GetBytes(System.Drawing.Image image)
        {
            try
            {
                if (image != null)
                {
                    System.Drawing.ImageConverter convert = new System.Drawing.ImageConverter();
                    Byte[] buffer = (Byte[])(convert.ConvertTo(image, typeof(Byte[])));
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static Byte[] GetBytes(System.Drawing.Image image, int width, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (image != null)
                {
                    System.Drawing.Image newImage = RedimImage(image, width, height, minByteWeight, maxByteWeight);
                    System.Drawing.ImageConverter convert = new System.Drawing.ImageConverter();
                    Byte[] buffer = (Byte[])(convert.ConvertTo(image, typeof(Byte[])));
                    return buffer;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static byte[] GetBytes(byte[] array, int width, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (array != null)
                {
                    return GetBytes(GetStream(array, width, height, minByteWeight, maxByteWeight));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static Byte[] GetBytes(System.IO.Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    int initialLength = 32768;
                    byte[] buffer = new byte[initialLength];
                    int read = 0;
                    int chunk = stream.Read(buffer, read, buffer.Length - read);
                    while (chunk > 0)
                    {
                        read += chunk;
                        if (read == buffer.Length)
                        {
                            int nextByte = stream.ReadByte();
                            if (nextByte == -1)
                            {
                                return buffer;
                            }
                            byte[] newBuffer = new byte[buffer.Length * 2];
                            Array.Copy(buffer, newBuffer, buffer.Length);
                            newBuffer[read] = Convert.ToByte(nextByte);
                            buffer = newBuffer;
                            read += 1;
                        }
                        chunk = stream.Read(buffer, read, buffer.Length - read);
                    }
                    byte[] bytes = new byte[read];
                    Array.Copy(buffer, bytes, read);
                    stream.Dispose();
                    return bytes;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static Byte[] GetBytes(System.IO.Stream stream, int width, int height,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (stream != null)
                {
                    return GetBytes(GetBytes(stream), width, height, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(string fileName, int width = 0, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (fileName != null && System.IO.File.Exists(fileName))
                {
                    System.Drawing.Image Image = System.Drawing.Image.FromFile(fileName, true);
                    return RedimImage(Image, width, height, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(string fileName, System.Drawing.Size size,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (fileName != null && System.IO.File.Exists(fileName))
                {
                    System.Drawing.Image Image = System.Drawing.Image.FromFile(fileName, true);
                    return RedimImage(Image, size, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(System.IO.Stream stream, int width = 0, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (stream != null)
                {
                    stream.Position = 0;
                    System.Drawing.Image Image = System.Drawing.Image.FromStream(stream, true);
                    return RedimImage(Image, width, height, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(System.IO.Stream stream, System.Drawing.Size size,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (stream != null)
                {
                    stream.Position = 0;
                    System.Drawing.Image Image = System.Drawing.Image.FromStream(stream, true);
                    return RedimImage(Image, size, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(Byte[] array, int width = 0, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (array != null)
                {
                    System.IO.MemoryStream MS = new System.IO.MemoryStream(array);
                    MS.Position = 0;
                    System.Drawing.Image Image = System.Drawing.Image.FromStream(MS, true);
                    return RedimImage(Image, width, height, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.Drawing.Image GetImage(Byte[] array, System.Drawing.Size size,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            try
            {
                if (array != null)
                {
                    System.IO.MemoryStream MS = new System.IO.MemoryStream(array);
                    System.Drawing.Image Image = System.Drawing.Image.FromStream(MS, true);
                    return RedimImage(Image, size, minByteWeight, maxByteWeight);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public static System.IO.Stream GetStream(string fileName)
        {
            System.IO.FileStream F = null;
            try
            {
                if (fileName != null && System.IO.File.Exists(fileName))
                {
                    F = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                    F.Position = 0;
                    return F;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (F != null)
                {
                    F.Dispose();
                }
            }
            return null;
        }

        public static System.IO.Stream GetStream(Byte[] Array)
        {
            if (Array != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Array);
                return ms;
            }
            return null;
        }

        public static System.IO.Stream GetStream(Byte[] array, int width = 0, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            if (array != null)
            {
                System.Drawing.Image image = GetImage(array, width, height, minByteWeight, maxByteWeight);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(GetBytes(image));
                return ms;
            }
            return null;
        }

        public static System.IO.Stream GetStream(System.Drawing.Image image)
        {
            if (image != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(GetBytes(image));
                return ms;
            }
            return null;
        }

        public static System.IO.Stream GetStream(System.Drawing.Image image, int width, int height = 0,
            long minByteWeight = 0, long maxByteWeight = 0)
        {
            if (image != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(GetBytes(image, width, height, minByteWeight, maxByteWeight));
                return ms;
            }
            return null;
        }

        public static System.Drawing.Image OrientImage(System.Drawing.Image image)
        {

            int orientationId = 0x0112;
            if (image.PropertyIdList.Contains(orientationId))
            {
                int rotationValue = image.GetPropertyItem(0x0112).Value[0];
                switch (rotationValue)
                {
                    case 1: // landscape, do nothing
                        break;

                    case 8: // rotated 90 right
                            // de-rotate:
                        image.RotateFlip(rotateFlipType: System.Drawing.RotateFlipType.Rotate270FlipNone);
                        break;

                    case 3: // bottoms up
                        image.RotateFlip(rotateFlipType: System.Drawing.RotateFlipType.Rotate180FlipNone);
                        break;

                    case 6: // rotated 90 left
                        image.RotateFlip(rotateFlipType: System.Drawing.RotateFlipType.Rotate90FlipNone);
                        break;
                }
                image.RemovePropertyItem(orientationId);
            }

            return image;
        }

        public static byte[] ReduceWeightImage(System.Drawing.Image image, long minByteWeight, long maxByteWeight)
        {
            byte[] source = GetBytes(image);
            if (source.Length < maxByteWeight)
                return source;

            if (minByteWeight > 0 && maxByteWeight > 0)
            {
                if (minByteWeight > maxByteWeight)
                    (minByteWeight, maxByteWeight) = (maxByteWeight, minByteWeight);
                if (maxByteWeight - minByteWeight < 50)
                    maxByteWeight = minByteWeight + 50;
                System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();
                string mimeType = GetMimeType(image);
                System.Drawing.Imaging.ImageCodecInfo imageCodecInfo = codecs[1];

                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                System.Drawing.Imaging.EncoderParameters encoderParameters =
                    new System.Drawing.Imaging.EncoderParameters(1);

                return ReduceWeightImageProcess(source, source, minByteWeight, maxByteWeight, imageCodecInfo,
                    encoder, encoderParameters, source.LongLength * 85L / 1166927L);

            }
            return source;
        }

        private static byte[] ReduceWeightImageProcess(byte[] source, byte[] destination,
            long minByteWeight, long maxByteWeight, System.Drawing.Imaging.ImageCodecInfo imageCodecInfo,
            System.Drawing.Imaging.Encoder encoder, System.Drawing.Imaging.EncoderParameters encoderParameters,
            long ratio)
        {
            if (source == null || source.Length == 0)
                throw new ArgumentNullException("source");

            if (destination == null || destination.Length == 0)
                destination = source;

            var nextRatio = ratio;
            System.IO.MemoryStream ms;
            System.Drawing.Image image;
            System.Drawing.Bitmap bmp1;
            int previousLength = source.Length;
            while (nextRatio > 0 && (destination.Length < minByteWeight || destination.Length > maxByteWeight))
            {
                using (image = System.Drawing.Image.FromStream(new System.IO.MemoryStream(source)))
                {
                    using (bmp1 = new System.Drawing.Bitmap(image))
                    {
                        encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(encoder, nextRatio);
                        using (ms = new System.IO.MemoryStream())
                        {
                            bmp1.Save(ms, imageCodecInfo, encoderParameters);
                            destination = ms.ToArray();
                            if(destination.Length == previousLength)
                            {
                                break;
                            }
                            previousLength = destination.Length;
                            if (destination.LongLength > maxByteWeight)
                            {
                                if (nextRatio > 10)
                                {
                                    nextRatio -= 3;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (destination.LongLength < minByteWeight)
                            {
                                nextRatio += 5;
                            }
                        }
                    }
                }
            }
            return destination;
        }

        public static System.Drawing.Image RedimImage(System.Drawing.Image image, int width, int height)
        {
            image = OrientImage(image);
            double PW = 0;
            if (width != 0)
            {
                PW = (double)image.Width / width;
            }
            double PH = 0;
            if (height != 0)
            {
                PH = (double)image.Height / height;
            }
            double P = PW > PH ? PW : PH;
            if (P > 1)
            {
                width = (int)(image.Width / P);
                height = (int)(image.Height / P);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image, width, height);
                using (System.Drawing.Graphics graphicsHandle = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphicsHandle.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphicsHandle.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphicsHandle.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphicsHandle.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    //graphicsHandle.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphicsHandle.DrawImage(bitmap, 0, 0, width, height);
                }
                return bitmap;
            }
            return image;

        }

        public static System.Drawing.Image RedimImage(System.Drawing.Image image, int width, int height,
            long minByteWeight, long maxByteWeight)
        {

            var bitmap = RedimImage(image, width, height);

            if (minByteWeight > 0 && maxByteWeight > 0)
            {
                var b = ReduceWeightImage(bitmap, minByteWeight, maxByteWeight);

                return System.Drawing.Image.FromStream(new System.IO.MemoryStream(b));
            }
            return bitmap;

        }

        public static System.Drawing.Image RedimImage(System.Drawing.Image image, System.Drawing.Size size,
            long minByteWeight, long maxByteWeight)
        {
            return RedimImage(image, size.Width, size.Height, minByteWeight, maxByteWeight);
        }

        public static string GetMimeType(string FileName)
        {
            if (FileName != null)
            {
                string Mime = "application/unknown";
                string Ext = System.IO.Path.GetExtension(FileName).ToLower();
                Microsoft.Win32.RegistryKey RegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Ext);
                if (RegKey != null && RegKey.GetValue("Content Type") != null)
                {
                    Mime = RegKey.GetValue("Content Type").ToString();
                }
                return Mime;
            }
            return null;
        }

        public static string GetMimeType(System.Drawing.Image image)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == image.RawFormat.Guid)?.MimeType ?? "image/jpg";
        }

        public static string GetExtension(string fileName)
        {
            if (fileName != null)
            {
                return System.IO.Path.GetExtension(fileName);
            }
            return null;
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> MimeTypeToExtension = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> ExtensionToMimeType = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

        public static string GetExtensionWithMimeType(string mimeType)
        {
            if (!string.IsNullOrWhiteSpace(mimeType))
            {
                string key = string.Format("MIME\\Database\\Content Type\\{0}", mimeType);
                string result = null;
                if (MimeTypeToExtension.TryGetValue(key, out result))
                {
                    return result;
                }

                Microsoft.Win32.RegistryKey regKey;
                Object value;

                regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key, false);
                value = regKey != null ? regKey.GetValue("Extension", null) : null;
                result = value != null ? value.ToString() : string.Empty;

                MimeTypeToExtension[key] = result;
                return result;
            }
            return null;
        }

        public static string GetMimeTypeWithExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }
                string result = null;
                if (ExtensionToMimeType.TryGetValue(extension, out result))
                {
                    return result;
                }
                Microsoft.Win32.RegistryKey regKey;
                Object value;

                regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension, false);
                value = regKey != null ? regKey.GetValue("Content Type", null) : null;
                result = value != null ? value.ToString() : string.Empty;
                ExtensionToMimeType[extension] = result;
                return result;
            }
            return null;
        }
    }
}