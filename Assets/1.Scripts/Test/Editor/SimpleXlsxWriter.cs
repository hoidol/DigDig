using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;

// 외부 라이브러리 없이 .xlsx 쓰기 (에디터 테스트 결과용). 시트마다 행 목록, 셀 = string(글씨) / 숫자(int·float·double)
// 첫 행은 머리글(굵게 + 틀 고정). 숫자는 천 단위 쉼표, 소수가 있으면 둘째 자리까지
public class SimpleXlsxWriter
{
    class Sheet
    {
        public string name;
        public List<object[]> rows = new();
        public float[] colWidths;
    }

    readonly List<Sheet> sheets = new();

    // colWidths: 열 너비(글자 수 기준, 비우면 기본)
    public List<object[]> AddSheet(string name, float[] colWidths = null)
    {
        var sheet = new Sheet { name = name, colWidths = colWidths };
        sheets.Add(sheet);
        return sheet.rows;
    }

    public void Save(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        // 임시 파일에 다 쓴 뒤 교체 → 엑셀이 열고 있으면 여기서 IOException (기존 파일은 그대로)
        string temp = path + ".tmp";
        using (FileStream fs = new FileStream(temp, FileMode.Create))
        using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            Write(zip, "[Content_Types].xml", ContentTypes());
            Write(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Write(zip, "xl/workbook.xml", Workbook());
            Write(zip, "xl/_rels/workbook.xml.rels", WorkbookRels());
            Write(zip, "xl/styles.xml", STYLES);
            for (int i = 0; i < sheets.Count; i++)
                Write(zip, $"xl/worksheets/sheet{i + 1}.xml", SheetXml(sheets[i]));
        }
        if (File.Exists(path))
            File.Delete(path);
        File.Move(temp, path);
    }

    static void Write(ZipArchive zip, string name, string content)
    {
        ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using StreamWriter w = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        w.Write(content);
    }

    string ContentTypes()
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
        sb.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
        sb.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
        sb.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
        sb.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
        for (int i = 0; i < sheets.Count; i++)
            sb.Append($"<Override PartName=\"/xl/worksheets/sheet{i + 1}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        sb.Append("</Types>");
        return sb.ToString();
    }

    string Workbook()
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
        for (int i = 0; i < sheets.Count; i++)
            sb.Append($"<sheet name=\"{Esc(sheets[i].name)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
        sb.Append("</sheets></workbook>");
        return sb.ToString();
    }

    string WorkbookRels()
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        for (int i = 0; i < sheets.Count; i++)
            sb.Append($"<Relationship Id=\"rId{i + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i + 1}.xml\"/>");
        sb.Append($"<Relationship Id=\"rId{sheets.Count + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        sb.Append("</Relationships>");
        return sb.ToString();
    }

    // 스타일 index: 0 기본, 1 머리글(굵게·회색 배경), 2 정수(#,##0), 3 소수(#,##0.00)
    const string STYLES = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
        + "<numFmts count=\"1\"><numFmt numFmtId=\"164\" formatCode=\"#,##0.00\"/></numFmts>"
        + "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>"
        + "<fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFDDEBF7\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>"
        + "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>"
        + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
        + "<cellXfs count=\"4\">"
        + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"
        + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\"/>"
        + "<xf numFmtId=\"3\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "</cellXfs></styleSheet>";

    static string SheetXml(Sheet sheet)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        sb.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");
        if (sheet.colWidths != null && sheet.colWidths.Length > 0)
        {
            sb.Append("<cols>");
            for (int i = 0; i < sheet.colWidths.Length; i++)
                sb.Append($"<col min=\"{i + 1}\" max=\"{i + 1}\" width=\"{sheet.colWidths[i].ToString(CultureInfo.InvariantCulture)}\" customWidth=\"1\"/>");
            sb.Append("</cols>");
        }
        sb.Append("<sheetData>");
        for (int r = 0; r < sheet.rows.Count; r++)
        {
            object[] row = sheet.rows[r];
            sb.Append($"<row r=\"{r + 1}\">");
            for (int c = 0; c < row.Length; c++)
            {
                object v = row[c];
                if (v == null)
                    continue;
                string cell = ColName(c) + (r + 1);
                bool header = r == 0;
                switch (v)
                {
                    case int i:
                        sb.Append($"<c r=\"{cell}\" s=\"{(header ? 1 : 2)}\"><v>{i}</v></c>");
                        break;
                    case float f:
                        AppendNumber(sb, cell, f, header);
                        break;
                    case double d:
                        AppendNumber(sb, cell, d, header);
                        break;
                    default:
                        sb.Append($"<c r=\"{cell}\" t=\"inlineStr\"{(header ? " s=\"1\"" : "")}><is><t>{Esc(v.ToString())}</t></is></c>");
                        break;
                }
            }
            sb.Append("</row>");
        }
        sb.Append("</sheetData></worksheet>");
        return sb.ToString();
    }

    static void AppendNumber(StringBuilder sb, string cell, double d, bool header)
    {
        if (double.IsNaN(d) || double.IsInfinity(d))
            return;
        int style = header ? 1 : (System.Math.Abs(d - System.Math.Round(d)) < 1e-9 ? 2 : 3);
        sb.Append($"<c r=\"{cell}\" s=\"{style}\"><v>{d.ToString("R", CultureInfo.InvariantCulture)}</v></c>");
    }

    static string ColName(int idx)
    {
        string s = "";
        idx++;
        while (idx > 0)
        {
            int m = (idx - 1) % 26;
            s = (char)('A' + m) + s;
            idx = (idx - 1) / 26;
        }
        return s;
    }

    static string Esc(string s) => SecurityElement.Escape(s) ?? "";
}
