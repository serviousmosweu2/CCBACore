using CCBA.Integration.Core.DMF.Extensions.Attributes;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CCBA.Integration.Core.DMF.Extensions
{
    public class DMFPackageManager : IDMFPackageManager
    {
        private ILogger<DMFPackageManager> _logger;

        public DMFPackageManager(ILogger<DMFPackageManager> logger)
        {
            _logger = logger;
        }

        public byte[] CreateExcel<T>(List<T> idefFiles)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage ExcelPkg = new ExcelPackage();
            var cellData = new List<string[]>();

            // Get all the class attributes for the file and the sheet
            var name = typeof(T).GetAttributeValue((XLSheetAttribute x) => x.Name);
            var filename = typeof(T).GetAttributeValue((XLSheetAttribute x) => x.File);

            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(name);

            // get all the headers from the first file
            var firstFile = idefFiles.FirstOrDefault();
            var headerRowList = new List<string>();

            foreach (var prop in firstFile.GetType().GetProperties())
            {
                var attrs = (XLColumnAttribute[])prop.GetCustomAttributes(typeof(XLColumnAttribute), false);
                foreach (var attr in attrs)
                {
                    headerRowList.Add(attr.Name);
                }
            }

            // Determine the header range (e.g. A1:D1)
            string headerRange = "A1:" + char.ConvertFromUtf32(headerRowList[0].Length + 64) + "1";
            var headerRow = new List<string[]>() {
                headerRowList.ToArray()
            };

            // Popular header row data
            wsSheet1.Cells[headerRange].LoadFromArrays(headerRow);

            foreach (var item in idefFiles)
            {
                var cellRow = new List<string>();
                foreach (var prop in firstFile.GetType().GetProperties())
                {
                    var attrs = (XLColumnAttribute[])prop.GetCustomAttributes(typeof(XLColumnAttribute), false);
                    foreach (var attr in attrs)
                    {
                        cellRow.Add(prop.GetValue(item)?.ToString());
                    }
                }
                cellData.Add(cellRow.ToArray());
            }

            string cellRange = "A2:" + char.ConvertFromUtf32(cellData[0].Length + 64) + "1";
            wsSheet1.Cells[2, 1].LoadFromArrays(cellData);

            using (var stream = new MemoryStream())
            {
                ExcelPkg.SaveAs(stream);
            }
            return ExcelPkg.GetAsByteArray();
        }

        public static DataTable ReadExcel(Stream stream)
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var headers = new List<string>();

                var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true,

                        ReadHeaderRow = rowReader =>
                        {
                            for (var i = 0; i < rowReader.FieldCount; i++)
                                headers.Add(Convert.ToString(rowReader.GetValue(i)));
                        },

                        FilterColumn = (columnReader, columnIndex) =>
                            headers.IndexOf("string") != columnIndex
                    }
                });
                return ds.Tables[0];
            }
        }

        public async Task<byte[]> CreatePackageAsync<T>(List<T> objFiles, string connectionString, string shareName, string folderName)
        {
            /// Read all the files from storage
            var templateFiles = await DMFFileManager.PackageFiles(shareName, folderName, connectionString);
            var packageheader = templateFiles["PackageHeader.xml"];
            var manifest = templateFiles["Manifest.xml"];

            var xlBytes = CreateExcel(objFiles);
            var filename = typeof(T).GetAttributeValue((XLSheetAttribute x) => x.File);

            // create a working memory stream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // create a zip
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // add the item name to the zip
                    ZipArchiveEntry zipItem = zip.CreateEntry(filename);
                    // add the item bytes to the zip entry by opening the original file and copying the bytes
                    using (MemoryStream originalFileMemoryStream = new MemoryStream(xlBytes))
                    {
                        using (Stream entryStream = zipItem.Open())
                        {
                            originalFileMemoryStream.CopyTo(entryStream);
                        }
                    }

                    // add the item name to the zip
                    ZipArchiveEntry zipItem1 = zip.CreateEntry("PackageHeader.xml");
                    // add the item bytes to the zip entry by opening the original file and copying the bytes

                    using (Stream entryStream1 = zipItem1.Open())
                    {
                        packageheader.CopyTo(entryStream1);
                    }
                    // add the item name to the zip
                    ZipArchiveEntry zipItem2 = zip.CreateEntry("Manifest.xml");
                    // add the item bytes to the zip entry by opening the original file and copying the bytes

                    using (Stream entryStream2 = zipItem2.Open())
                    {
                        manifest.CopyTo(entryStream2);
                    }

                }

                return memoryStream.GetBuffer();
            }
        }
    }
}
