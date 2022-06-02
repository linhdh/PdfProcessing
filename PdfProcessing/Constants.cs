namespace PdfProcessing
{
    public class Constants
    {
        public const string PDFPROC_FILE_IDENLANGUAGE = "Core14.profile.xml";
        public const string PDFPROC_FILE_CONFIGURATION = "Configuration.json";
        public const string PDFPROC_FILE_CONFIGQRCODE = "QrCodeConfig.json";

        public const string APP_DATASOURCES_ROOTNAME = "DataSources";
        public const string APP_DEU_DAYFORMAT = "dd.MM.yyyy";
        public const string APP_DEU_DATEFORMAT = "dd.MM.yyyy HH:mm:ss";
        public const string APP_EN_DATEFORMAT = "yyyy/MM/dd HH:mm:ss";
        public const string APP_EN_HOUR = "dd.MM.yyyy HH:mm";
        public const string APP_EN_DATEFORMATFULL = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public const string APP_EN_YEARFORMAT = "yyyy-MM-dd";
        public const string APP_LOGFILE_PATTERN = "Logs\\log.txt";

        public const string PROCESSING_ADD_TEXT_LAYER = "ON";

        public const string APP_CHARACTER_SPACE = " ";
        public const string APP_CHARACTER_EMPTY = "";
        public const string APP_CHARACTER_COMMA = ",";
        public const string APP_CHARACTER_DOT = ".";
        public const string APP_CHARACTER_TWO_DOT = "..";

        public const string PDF_EXTENSION_LOWERCASE = ".pdf";

        public const string HIGHQUEUE_URI_SEND = "queue:pdf_processing_high_queue";
        public const string LOWQUEUE_URI_SEND = "queue:pdf_processing_low_queue";
    }
}
