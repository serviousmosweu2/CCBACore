namespace CCBA.Integrations.DMF.Import.Models
{
    public class DmfImportError
    {
        public Error error { get; set; }

        public class Error
        {
            public string code { get; set; }
            public Innererror innererror { get; set; }
            public string message { get; set; }

            public class Innererror
            {
                public Internalexception internalexception { get; set; }
                public string message { get; set; }
                public string type { get; set; }

                public class Internalexception
                {
                    public Internalexception internalexception { get; set; }
                    public string message { get; set; }
                    public string type { get; set; }
                }
            }
        }
    }
}