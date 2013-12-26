using System;
using Aspose.Cells;
using SmartSchool.StudentRelated.RibbonBars.Export.ResponseHandler.Converter;
using SmartSchool.StudentRelated.RibbonBars.Export.ResponseHandler.DataType;

namespace SmartSchool.StudentRelated.RibbonBars.Export.ResponseHandler.Output
{
    public class ExportOutput : IOutput<Workbook>
    {
        private Workbook _book;
        public void SetSource(ExportTable source)
        {
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];

            foreach (ExportField field in source.Columns)
            {
                Cell wcell = sheet.Cells[0, field.ColumnIndex];
                wcell.PutValue(field.DisplayText);
            }

            foreach (ExportRow row in source.Rows)
            {
                foreach (ExportField field in source.Columns)
                {
                    Cell wcell = sheet.Cells[row.Index + 1, field.ColumnIndex];
                    ExportCell ecell = row.Cells[field.ColumnIndex];
                    string value = ecell.Value;

                    IConverter converter = ConverterFactory.GetInstance(field.Converter);
                    value = converter.Convert(value);

                    IDataType dataType = DataTypeFactory.GetInstance(field.DataType);
                    dataType.SetValue(value);
                    if (!dataType.IsValidDataType)
                        wcell.PutValue(value, false);
                    else
                    {
                        switch (field.DataType.ToLower())
                        {
                            case "integer":
                                //wcell.PutValue(int.Parse(value));
                                wcell.PutValue(value, true);
                                break;
                            case "double":
                                //wcell.PutValue(double.Parse(value));
                                wcell.PutValue(value, true);
                                break;
                            case "datetime":
                                //int styleIndex = book.Styles.Add();
                                //Style style = book.Styles[styleIndex];
                                //style.Number = 14;
                                //style.Custom = "yyyy/MM/dd;@";
                                //wcell.Style.Copy(style);
                                DateTime dt = Convert.ToDateTime(dataType.GetTypeValue());
                                wcell.PutValue(dt.ToShortDateString(), false);
                                //wcell.PutValue(DateTime.Parse(value));
                                break;
                            case "object":
                                wcell.PutValue((object)value);
                                break;
                            default:
                                wcell.PutValue(value, false);
                                break;
                        }
                    }
                }
            }

            int ColumnIndex = -1;

            for (int i = 0; i < sheet.Cells.MaxDataColumn; i++)
                if (sheet.Cells[0, i].Value.Equals("ネら"))
                {
                    sheet.Cells.InsertColumn(i+1, true);
                    sheet.Cells[0, i + 1].PutValue("チ郁ネら");
                    ColumnIndex = i + 1;
                    break;
                }



            if (ColumnIndex != -1)
            {
                for (int RowIndex = 1; RowIndex <= sheet.Cells.MaxDataRowInColumn(ColumnIndex-1); RowIndex++)
                {
                    if (sheet.Cells[RowIndex, ColumnIndex - 1].Value != null)
                    {
                        string DateStr = sheet.Cells[RowIndex, ColumnIndex - 1].StringValue;

                        DateTime ChineseDate = new DateTime();

                        if (DateTime.TryParse(DateStr,out ChineseDate))
                            sheet.Cells[RowIndex, ColumnIndex].PutValue(""+(ChineseDate.Year-1911)+"/"+ChineseDate.Month+"/"+ChineseDate.Day); 
                    }
                }
            }

            _book = book;
        }

        public Workbook GetOutput()
        {
            return _book;
        }

        public void Save(string filename)
        {
            _book.Save(filename, FileFormatType.Excel2000);
        }
    }
}
