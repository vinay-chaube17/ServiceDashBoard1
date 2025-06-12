using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using ServiceDashBoard1.Models;
using iText.Kernel.Colors; // Make sure this namespace is added
using iText.IO.Image; // Add this at the top for image support


using Path = System.IO.Path;


namespace ServiceDashBoard1.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;
        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public byte[] GenerateComplaintPdf(ServiceModel model)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Step 1: Writer & Document
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(10, 10, 10, 10); // Minimize margins to use more space

                // Step 2: Bold Font and Regular Font
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);


                // Load and add logo above the title
                string imagePath = Path.Combine(_env.WebRootPath, "images", "SilLOGO.png");
                ImageData imageData = ImageDataFactory.Create(imagePath);
                Image logo = new Image(imageData);
                logo.ScaleToFit(80, 60); // Resize as needed
                logo.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                logo.SetMarginBottom(0); // Reduce this value to minimize gap (try 0-5)
                document.Add(logo);

                // Title with no extra margin
                Paragraph title = new Paragraph()
                    .Add(new Text("Complaint Details")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetFontColor(ColorConstants.RED))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(0); // Remove top space if any
                    document.Add(title);


                // Step 4: Space (Optional)
                //document.Add(new Paragraph("\n"));

                // Step 5: Table with 100% Width and Dynamic Row Height
                // Table will span the full width of the page
                float[] columnWidths = { 2, 4 }; // Define column widths (in proportional values)
                Table table = new Table(columnWidths).UseAllAvailableWidth();

                // Set table to be centered and expand to fit the full width
                table.SetTextAlignment(TextAlignment.CENTER);
                table.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                // Add header row with bold font
                //table.AddCell(new Cell().Add(new Paragraph("Field").SetFont(boldFont)).SetTextAlignment(TextAlignment.CENTER));
                //table.AddCell(new Cell().Add(new Paragraph("Value").SetFont(boldFont)).SetTextAlignment(TextAlignment.CENTER));

                // Add rows for each complaint field
                table.AddCell(new Cell().Add(new Paragraph("Token").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.TokenNumber).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Machine Serial No").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.MachineSerialNo).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Company Name").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.CompanyName).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Email").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.Email).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Phone").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.PhoneNo).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Address").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.Address).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Description").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.ComplaintDescription).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Main Problem").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.MainProblemText).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Sub Problem").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.SubProblemText).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Remark").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.Remark).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Final Remark").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.FinalRemark).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                table.AddCell(new Cell().Add(new Paragraph("Status").SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));
                table.AddCell(new Cell().Add(new Paragraph(model.Status).SetFont(regularFont).SetTextAlignment(TextAlignment.LEFT)));

                // Step 6: Add the table to the document
                document.Add(table);

                // Step 7: Close the document
                document.Close();

                // Return the document as a byte array (for saving or sending the PDF)
                return stream.ToArray();
            }
        }


    }
}
