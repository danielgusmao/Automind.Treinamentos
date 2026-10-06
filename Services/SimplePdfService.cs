using System.Globalization;
using System.Text;
using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class SimplePdfService
{
    private readonly IWebHostEnvironment _env;

    public SimplePdfService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public byte[] CreateEvidencePdf(Training training, TrainingCompletion completion, string trainingHash)
    {
        var c = new StringBuilder();
        PageBackground(c, 595, 842);
        DrawLogo(c, 44, 756, 214, 59);
        Text(c, "EVIDENCIA CORPORATIVA", 12, 516, 790, Font.Bold, Color.Magenta, Align.Right);
        Text(c, completion.Protocol, 9, 516, 773, Font.Regular, Color.Muted, Align.Right);

        Rect(c, 0, 654, 595, 78, Color.Dark);
        Text(c, "TREINAMENTO CONCLUIDO", 9, 44, 705, Font.Bold, Color.PinkLight);
        var titleLines = Wrap(training.Title, 48).Take(2).ToArray();
        var titleY = 683d;
        foreach (var line in titleLines)
        {
            Text(c, line, 19, 44, titleY, Font.Bold, Color.White);
            titleY -= 21;
        }

        Pill(c, 390, 681, 72, 23, "APROVADO", Color.Green, Color.GreenSoft, 8.5);
        Pill(c, 469, 681, 82, 23, $"NOTA {completion.Score}/{completion.Total}", Color.White, Color.Magenta, 8.5);

        Text(c, "RESUMO", 9, 44, 625, Font.Bold, Color.Magenta);
        Card(c, 44, 548, 507, 62, Color.Soft);
        Metric(c, "VERSAO", training.Version, 60, 571);
        Metric(c, "TEMPO ESTIMADO", $"{training.EstimatedMinutes} min", 207, 571);
        Metric(c, "TEMPO REALIZADO", FormatDuration(completion.DurationSeconds), 385, 571);

        Text(c, "COLABORADOR", 9, 44, 520, Font.Bold, Color.Magenta);
        Card(c, 44, 375, 507, 130, Color.White, Color.Line);
        LabelValue(c, "NOME", completion.DisplayName, 60, 472, 218);
        LabelValue(c, "LOGIN AD", completion.SamAccountName, 310, 472, 220);
        LabelValue(c, "E-MAIL", completion.Email, 60, 427, 218);
        LabelValue(c, "CARGO", completion.JobTitle, 310, 427, 220);
        LabelValue(c, "DEPARTAMENTO", completion.Department, 60, 389, 218);

        Text(c, "REGISTRO DE ACEITE", 9, 44, 346, Font.Bold, Color.Magenta);
        Card(c, 44, 252, 507, 79, Color.Soft);
        LabelValue(c, "INICIO", completion.StartedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"), 60, 304, 218);
        LabelValue(c, "CONCLUSAO", completion.AcceptedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"), 310, 304, 220);
        LabelValue(c, "TERMO DE CIENCIA", "ACEITO", 60, 266, 218, Color.Green);
        LabelValue(c, "DURACAO REGISTRADA", FormatDuration(completion.DurationSeconds), 310, 266, 220);

        Text(c, "DECLARACAO", 9, 44, 224, Font.Bold, Color.Magenta);
        Card(c, 44, 139, 507, 70, Color.White, Color.Line);
        var declaration = "Declaro que participei do treinamento, li e compreendi o seu conteudo e me comprometo a observar a Politica de Seguranca da Informacao e as normas internas de protecao de dados da AutoMind.";
        TextBlock(c, declaration, 9.2, 60, 187, 82, 14, Color.Ink);

        Text(c, "INTEGRIDADE", 8, 44, 111, Font.Bold, Color.Muted);
        Text(c, "SHA-256 do treinamento:", 7.5, 44, 96, Font.Bold, Color.Muted);
        Text(c, trainingHash, 7.1, 44, 83, Font.Regular, Color.Muted);
        Text(c, "Documento interno - Automind.Treinamentos v0.0.3", 7.5, 44, 51, Font.Regular, Color.Muted);
        Text(c, "Gerado automaticamente a partir do registro oficial no banco de dados.", 7.5, 551, 51, Font.Regular, Color.Muted, Align.Right);

        return BuildPdf(new List<string> { c.ToString() }, 595, 842, LoadLogoJpeg());
    }

    public byte[] CreateConsolidatedPdf(Training training, IReadOnlyCollection<TrainingCompletion> completions)
    {
        const double width = 842;
        const double height = 595;
        const int rowsPerPage = 17;
        var ordered = completions.OrderBy(x => x.DisplayName).ToList();
        var chunks = ordered.Count == 0
            ? new List<List<TrainingCompletion>> { new() }
            : ordered.Chunk(rowsPerPage).Select(x => x.ToList()).ToList();
        var pages = new List<string>();
        var avgSeconds = ordered.Count == 0 ? 0 : (int)Math.Round(ordered.Average(x => x.DurationSeconds));

        for (var pageIndex = 0; pageIndex < chunks.Count; pageIndex++)
        {
            var c = new StringBuilder();
            PageBackground(c, width, height);
            DrawLogo(c, 38, 510, 180, 50);
            Text(c, "RELATORIO CONSOLIDADO", 10, 800, 548, Font.Bold, Color.Magenta, Align.Right);
            Text(c, $"Pagina {pageIndex + 1} de {chunks.Count}", 8, 800, 532, Font.Regular, Color.Muted, Align.Right);

            Rect(c, 0, 420, width, 72, Color.Dark);
            Text(c, training.Title, 18, 38, 463, Font.Bold, Color.White);
            Text(c, $"Versao {training.Version}  |  Tempo estimado {training.EstimatedMinutes} min  |  {ordered.Count} aceite(s)  |  Media real {FormatDuration(avgSeconds)}", 9, 38, 440, Font.Regular, Color.PinkLight);

            var y = 389d;
            Rect(c, 38, y - 4, 766, 25, Color.Purple);
            Text(c, "NOME", 7.4, 48, y + 5, Font.Bold, Color.White);
            Text(c, "LOGIN", 7.4, 215, y + 5, Font.Bold, Color.White);
            Text(c, "E-MAIL", 7.4, 304, y + 5, Font.Bold, Color.White);
            Text(c, "AREA", 7.4, 468, y + 5, Font.Bold, Color.White);
            Text(c, "NOTA", 7.4, 555, y + 5, Font.Bold, Color.White);
            Text(c, "TEMPO", 7.4, 594, y + 5, Font.Bold, Color.White);
            Text(c, "DATA", 7.4, 655, y + 5, Font.Bold, Color.White);
            Text(c, "PROTOCOLO", 7.4, 735, y + 5, Font.Bold, Color.White);
            y -= 25;

            foreach (var item in chunks[pageIndex])
            {
                Rect(c, 38, y - 3, 766, 20, ((int)((389 - y) / 20)) % 2 == 0 ? Color.White : Color.Soft);
                Text(c, Truncate(item.DisplayName, 28), 7.2, 48, y + 4, Font.Regular, Color.Ink);
                Text(c, Truncate(item.SamAccountName, 16), 7.2, 215, y + 4, Font.Regular, Color.Ink);
                Text(c, Truncate(item.Email, 29), 7.0, 304, y + 4, Font.Regular, Color.Ink);
                Text(c, Truncate(item.Department, 13), 7.2, 468, y + 4, Font.Regular, Color.Ink);
                Text(c, $"{item.Score}/{item.Total}", 7.2, 555, y + 4, Font.Bold, Color.Green);
                Text(c, ShortDuration(item.DurationSeconds), 7.0, 594, y + 4, Font.Regular, Color.Ink);
                Text(c, item.AcceptedAtUtc.ToLocalTime().ToString("dd/MM/yy"), 7.0, 655, y + 4, Font.Regular, Color.Ink);
                Text(c, Truncate(item.Protocol, 18), 6.8, 735, y + 4, Font.Regular, Color.Ink);
                y -= 20;
            }

            Text(c, $"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}", 7.5, 38, 35, Font.Regular, Color.Muted);
            Text(c, "Automind.Treinamentos v0.0.3 - Documento interno", 7.5, 804, 35, Font.Regular, Color.Muted, Align.Right);
            pages.Add(c.ToString());
        }

        return BuildPdf(pages, width, height, LoadLogoJpeg());
    }

    private byte[]? LoadLogoJpeg()
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var path = Path.Combine(webRoot, "images", "automind-logo-positive.jpg");
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    private static byte[] BuildPdf(IReadOnlyList<string> pageContents, double width, double height, byte[]? logoJpeg)
    {
        var objects = new List<byte[]>();
        var catalogObj = Add(objects, "<< /Type /Catalog /Pages 2 0 R >>");
        var pagesObjIndex = objects.Count;
        objects.Add(Array.Empty<byte>());
        var regularFont = Add(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        var boldFont = Add(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");
        var italicFont = Add(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique /Encoding /WinAnsiEncoding >>");
        var imageObj = logoJpeg is null ? 0 : AddImageStream(objects, logoJpeg, 900, 250);
        var pageObjectNumbers = new List<int>();

        foreach (var content in pageContents)
        {
            var contentObj = AddStream(objects, content);
            var imageResource = imageObj > 0 ? $" /XObject << /Im1 {imageObj} 0 R >>" : "";
            var pageObj = Add(objects, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(width)} {F(height)}] /Resources << /Font << /F1 {regularFont} 0 R /F2 {boldFont} 0 R /F3 {italicFont} 0 R >>{imageResource} >> /Contents {contentObj} 0 R >>");
            pageObjectNumbers.Add(pageObj);
        }

        var kids = string.Join(" ", pageObjectNumbers.Select(x => $"{x} 0 R"));
        objects[pagesObjIndex] = EncodeLatin1($"<< /Type /Pages /Kids [ {kids} ] /Count {pageObjectNumbers.Count} >>");

        using var ms = new MemoryStream();
        WriteAscii(ms, "%PDF-1.4\n%PDFBINARY\n");
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(ms.Position);
            WriteAscii(ms, $"{i + 1} 0 obj\n");
            ms.Write(objects[i]);
            WriteAscii(ms, "\nendobj\n");
        }

        var xref = ms.Position;
        WriteAscii(ms, $"xref\n0 {objects.Count + 1}\n");
        WriteAscii(ms, "0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
            WriteAscii(ms, $"{offsets[i]:0000000000} 00000 n \n");
        WriteAscii(ms, $"trailer\n<< /Size {objects.Count + 1} /Root {catalogObj} 0 R >>\nstartxref\n{xref}\n%%EOF");
        return ms.ToArray();
    }

    private static void PageBackground(StringBuilder c, double width, double height)
    {
        Rect(c, 0, 0, width, height, Color.White);
        Rect(c, 0, height - 8, width, 8, Color.Magenta);
    }

    private static void DrawLogo(StringBuilder c, double x, double y, double w, double h)
    {
        c.AppendLine($"q {F(w)} 0 0 {F(h)} {F(x)} {F(y)} cm /Im1 Do Q");
    }

    private static void Card(StringBuilder c, double x, double y, double w, double h, Color fill, Color? border = null)
    {
        Rect(c, x, y, w, h, fill);
        if (border is not null)
        {
            var b = border.Value;
            c.AppendLine($"q {b.Stroke()} 0.7 w {F(x)} {F(y)} {F(w)} {F(h)} re S Q");
        }
    }

    private static void Metric(StringBuilder c, string label, string value, double x, double y)
    {
        Text(c, label, 7.3, x, y + 15, Font.Bold, Color.Muted);
        Text(c, value, 11, x, y, Font.Bold, Color.Ink);
    }

    private static void LabelValue(StringBuilder c, string label, string value, double x, double y, double maxWidth, Color? valueColor = null)
    {
        Text(c, label, 7.2, x, y + 14, Font.Bold, Color.Muted);
        var chars = Math.Max(12, (int)(maxWidth / 5.2));
        Text(c, Truncate(value, chars), 10, x, y, Font.Bold, valueColor ?? Color.Ink);
    }

    private static void Pill(StringBuilder c, double x, double y, double w, double h, string value, Color textColor, Color fill, double size)
    {
        Rect(c, x, y, w, h, fill);
        Text(c, value, size, x + (w / 2), y + 7.2, Font.Bold, textColor, Align.Center);
    }

    private static void TextBlock(StringBuilder c, string value, double size, double x, double y, int maxChars, double lineHeight, Color color)
    {
        foreach (var line in Wrap(value, maxChars))
        {
            Text(c, line, size, x, y, Font.Regular, color);
            y -= lineHeight;
        }
    }

    private static IEnumerable<string> Wrap(string value, int maxChars)
    {
        var words = (value ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = new StringBuilder();
        foreach (var word in words)
        {
            if (line.Length > 0 && line.Length + word.Length + 1 > maxChars)
            {
                yield return line.ToString();
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(word);
        }
        if (line.Length > 0) yield return line.ToString();
    }

    private static string Truncate(string? value, int max)
    {
        value ??= "";
        if (value.Length <= max) return value;
        return value[..Math.Max(1, max - 3)] + "...";
    }

    private static string FormatDuration(int seconds)
    {
        if (seconds <= 0) return "Nao registrado";
        var t = TimeSpan.FromSeconds(seconds);
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours} h {t.Minutes:D2} min";
        return t.Seconds > 0 ? $"{t.Minutes} min {t.Seconds:D2} s" : $"{t.Minutes} min";
    }

    private static string ShortDuration(int seconds)
    {
        if (seconds <= 0) return "-";
        var t = TimeSpan.FromSeconds(seconds);
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h{t.Minutes:D2}";
        return $"{t.Minutes}m{t.Seconds:D2}";
    }

    private static void Rect(StringBuilder c, double x, double y, double w, double h, Color color)
    {
        c.AppendLine($"q {color.Fill()} {F(x)} {F(y)} {F(w)} {F(h)} re f Q");
    }

    private static void Text(StringBuilder c, string text, double size, double x, double y, Font font, Color color, Align align = Align.Left)
    {
        var adjustedX = x;
        if (align != Align.Left)
        {
            var estimatedWidth = text.Length * size * 0.49;
            adjustedX = align == Align.Center ? x - (estimatedWidth / 2) : x - estimatedWidth;
        }
        var fontName = font switch { Font.Bold => "/F2", Font.Italic => "/F3", _ => "/F1" };
        c.AppendLine($"BT {fontName} {F(size)} Tf {color.Fill()} 1 0 0 1 {F(adjustedX)} {F(y)} Tm ({EscapePdfString(text)}) Tj ET");
    }

    private static string EscapePdfString(string value)
    {
        var sb = new StringBuilder();
        foreach (var ch in value ?? "")
        {
            if (ch == '\\' || ch == '(' || ch == ')') sb.Append('\\');
            if (ch <= 255) sb.Append(ch);
            else sb.Append('?');
        }
        return sb.ToString();
    }

    private static int Add(List<byte[]> objects, string body)
    {
        objects.Add(EncodeLatin1(body));
        return objects.Count;
    }

    private static int AddStream(List<byte[]> objects, string content)
    {
        var bytes = EncodeLatin1(content);
        using var ms = new MemoryStream();
        WriteAscii(ms, $"<< /Length {bytes.Length} >>\nstream\n");
        ms.Write(bytes);
        WriteAscii(ms, "\nendstream");
        objects.Add(ms.ToArray());
        return objects.Count;
    }

    private static int AddImageStream(List<byte[]> objects, byte[] jpeg, int width, int height)
    {
        using var ms = new MemoryStream();
        WriteAscii(ms, $"<< /Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpeg.Length} >>\nstream\n");
        ms.Write(jpeg);
        WriteAscii(ms, "\nendstream");
        objects.Add(ms.ToArray());
        return objects.Count;
    }

    private static byte[] EncodeLatin1(string value)
    {
        var bytes = new byte[value.Length];
        for (var i = 0; i < value.Length; i++) bytes[i] = value[i] <= 255 ? (byte)value[i] : (byte)'?';
        return bytes;
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private enum Font { Regular, Bold, Italic }
    private enum Align { Left, Center, Right }

    private readonly record struct Color(double R, double G, double B)
    {
        public string Fill() => $"{SimplePdfService.F(R)} {SimplePdfService.F(G)} {SimplePdfService.F(B)} rg";
        public string Stroke() => $"{SimplePdfService.F(R)} {SimplePdfService.F(G)} {SimplePdfService.F(B)} RG";

        public static readonly Color White = new(1, 1, 1);
        public static readonly Color Ink = new(0.11, 0.09, 0.13);
        public static readonly Color Muted = new(0.43, 0.40, 0.45);
        public static readonly Color Line = new(0.91, 0.89, 0.92);
        public static readonly Color Soft = new(0.97, 0.96, 0.98);
        public static readonly Color Dark = new(0.09, 0.07, 0.11);
        public static readonly Color Purple = new(0.31, 0.09, 0.43);
        public static readonly Color Magenta = new(0.90, 0.00, 0.35);
        public static readonly Color PinkLight = new(1.00, 0.72, 0.84);
        public static readonly Color Green = new(0.09, 0.41, 0.30);
        public static readonly Color GreenSoft = new(0.92, 0.98, 0.95);
    }
}
